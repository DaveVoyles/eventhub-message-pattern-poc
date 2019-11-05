using Newtonsoft.Json;
using OneWeek_Eventing.Common.Providers.Redis;
using OneWeek_Eventing.StreamingWithResend.Entities;
using OneWeek_Eventing.StreamingWithResend.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OneWeek_Eventing.StreamingWithResend.Provider.Redis
{
    public class RedisReceiverProvider : IReceiverProvider
    {
        private ConnectionMultiplexer _redis;
        private int _updateSequenceNumber = 0;
        private List<Update> _queuedMessages = new List<Update>();
        private List<Tuple<int, int>> _resendRequests = new List<Tuple<int, int>>();

        public event EventHandler<Update> OnUpdateReceived;

        public Task Start()
        {
            _redis = ConnectionMultiplexer.Connect(RedisConfiguration.GetConnectionString());
            var subscriber = _redis.GetSubscriber();

            subscriber.Subscribe("Update-*", OnUpdate);
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _redis.Close();
            _redis.Dispose();
            _redis = null;
            return Task.CompletedTask;
        }

        private void OnUpdate(RedisChannel channel, RedisValue value)
        {
            lock (this)
            {
                var update = JsonConvert.DeserializeObject<Update>(value);
                var expectedSequenceNumber = _updateSequenceNumber + 1;

                if (update.SequenceNumber > expectedSequenceNumber)
                {
                    int queueIndex = 0;
                    for (queueIndex = 0; queueIndex < _queuedMessages.Count; queueIndex++)
                    {
                        if (update.SequenceNumber < _queuedMessages[queueIndex].SequenceNumber)
                            break;
                    }

                    if (queueIndex < _queuedMessages.Count)
                        _queuedMessages.Insert(queueIndex, update);
                    else
                        _queuedMessages.Add(update);

                    var lowSequenceNumber = expectedSequenceNumber;
                    var highSequenceNumber = _queuedMessages[0].SequenceNumber - 1;
                    foreach (var resendRequest in _resendRequests)
                    {
                        if (resendRequest.Item1 <= lowSequenceNumber && resendRequest.Item2 >= highSequenceNumber)
                            return;
                    }
                    var subscriber = _redis.GetSubscriber();
                    subscriber.Publish("ResendUpdate-*", $"{lowSequenceNumber}-{highSequenceNumber}");
                    _resendRequests.Add(new Tuple<int, int>(lowSequenceNumber, highSequenceNumber));
                }
                else
                {
                    OnUpdateReceived?.Invoke(this, update);
                    _updateSequenceNumber = update.SequenceNumber;

                    while (_queuedMessages.Count > 0 && _queuedMessages[0].SequenceNumber == (_updateSequenceNumber + 1))
                    {
                        OnUpdateReceived?.Invoke(this, _queuedMessages[0]);
                        _updateSequenceNumber = _queuedMessages[0].SequenceNumber;
                        _queuedMessages.RemoveAt(0);
                    }
                }
            }
        }
    }
}
