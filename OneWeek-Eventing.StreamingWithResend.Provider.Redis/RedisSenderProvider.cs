using Newtonsoft.Json;
using OneWeek_Eventing.Common.Providers.Redis;
using OneWeek_Eventing.StreamingWithResend.Entities;
using OneWeek_Eventing.StreamingWithResend.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneWeek_Eventing.StreamingWithResend.Provider.Redis
{
    public class RedisSenderProvider : ISenderProvider
    {
        private ConnectionMultiplexer _redis;
        private int _latestSequenceNumber = 0;
        private List<Latest> _listSentMessages = new List<Latest>();

        public Task Start()
        {
            _redis = ConnectionMultiplexer.Connect(RedisConfiguration.GetConnectionString());
            var subscriber = _redis.GetSubscriber();

            subscriber.Subscribe("ResendLatest-*", OnResendLatest);

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _redis.Close();
            _redis.Dispose();
            _redis = null;
            return Task.CompletedTask;
        }

        public async Task SendMessageAsync(Latest latest)
        {
            lock (this)
            {
                int expectedSequenceNumber = _latestSequenceNumber + 1;

                // are we in sync?
                if (latest.SequenceNumber != expectedSequenceNumber)
                    throw new IndexOutOfRangeException($"Expected SequenceNumber {expectedSequenceNumber}, got {latest.SequenceNumber}.");

                _listSentMessages.Add(latest);
                _latestSequenceNumber++;
            }
            var subscriber = _redis.GetSubscriber();
            var latestAsJson = JsonConvert.SerializeObject(latest);

            await subscriber.PublishAsync("Latest-*", latestAsJson);
        }

        private void OnResendLatest(RedisChannel channel, RedisValue value)
        {
            var tokens = value.ToString().Split('-');
            var fromSequenceNumber = int.Parse(tokens[0]);
            var toSequenceNumber = int.Parse(tokens[1]);

            var subscriber = _redis.GetSubscriber();

            lock (this)
            {
                for (int index = fromSequenceNumber - 1; index < _listSentMessages.Count && index < toSequenceNumber; index++)
                {
                    subscriber.Publish("Latest-*", JsonConvert.SerializeObject(_listSentMessages[index]));
                }
            }
        }
    }
}