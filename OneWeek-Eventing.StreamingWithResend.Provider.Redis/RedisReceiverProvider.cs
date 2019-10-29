using Newtonsoft.Json;
using OneWeek_Eventing.Common.Providers.Redis;
using OneWeek_Eventing.StreamingWithResend.Entities;
using OneWeek_Eventing.StreamingWithResend.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OneWeek_Eventing.StreamingWithResend.Provider.Redis
{
    public class RedisReceiverProvider : IReceiverProvider
    {
        private ConnectionMultiplexer _redis;
        private int _latestSequenceNumber = 0;
        private List<Update> _queuedMessages = new List<Update>();
        private List<Tuple<int, int>> _resendRequests = new List<Tuple<int, int>>();

        public event EventHandler<Update> OnLatestReceived;

        public Task Start()
        {
            _redis = ConnectionMultiplexer.Connect(RedisConfiguration.GetConnectionString());
            var subscriber = _redis.GetSubscriber();

            subscriber.Subscribe("Latest-*", OnLatest);
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _redis.Close();
            _redis.Dispose();
            _redis = null;
            return Task.CompletedTask;
        }

        private void OnLatest(RedisChannel channel, RedisValue value)
        {
            lock (this)
            {
                var latest = JsonConvert.DeserializeObject<Update>(value);
                var expectedSeqenceNumber = _latestSequenceNumber + 1;

                if (latest.SequenceNumber > expectedSeqenceNumber)
                {
                    int queueIndex = 0;
                    for (queueIndex = 0; queueIndex < _queuedMessages.Count; queueIndex++)
                    {
                        if (latest.SequenceNumber < _queuedMessages[queueIndex].SequenceNumber)
                            break;
                    }

                    if (queueIndex < _queuedMessages.Count)
                        _queuedMessages.Insert(queueIndex, latest);
                    else
                        _queuedMessages.Add(latest);

                    var lowSequenceNumber = expectedSeqenceNumber;
                    var highSequenceNumber = _queuedMessages[0].SequenceNumber - 1;
                    foreach (var resendRequest in _resendRequests)
                    {
                        if (resendRequest.Item1 <= lowSequenceNumber && resendRequest.Item2 >= highSequenceNumber)
                            return;
                    }
                    var subscriber = _redis.GetSubscriber();
                    subscriber.Publish("ResendLatest-*", $"{lowSequenceNumber}-{highSequenceNumber}");
                    _resendRequests.Add(new Tuple<int, int>(lowSequenceNumber, highSequenceNumber));
                }
                else
                {
                    OnLatestReceived?.Invoke(this, latest);
                    _latestSequenceNumber = latest.SequenceNumber;

                    while (_queuedMessages.Count > 0 && _queuedMessages[0].SequenceNumber == (_latestSequenceNumber + 1))
                    {
                        OnLatestReceived?.Invoke(this, latest);
                        _latestSequenceNumber = latest.SequenceNumber;
                        _queuedMessages.RemoveAt(0);
                    }
                }
            }
        }
    }
}
