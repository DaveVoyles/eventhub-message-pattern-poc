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
        private int _updateSequenceNumber = 0;
        private List<Update> _listSentMessages = new List<Update>();

        public Task Start()
        {
            _redis = ConnectionMultiplexer.Connect(RedisConfiguration.GetConnectionString());
            var subscriber = _redis.GetSubscriber();

            subscriber.Subscribe("ResendUpdate-*", OnResendUpdate);

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _redis.Close();
            _redis.Dispose();
            _redis = null;
            return Task.CompletedTask;
        }

        public async Task SendMessageAsync(Update update)
        {
            lock (this)
            {
                int expectedSequenceNumber = _updateSequenceNumber + 1;

                // are we in sync?
                if (update.SequenceNumber != expectedSequenceNumber)
                    throw new IndexOutOfRangeException($"Expected SequenceNumber {expectedSequenceNumber}, got {update.SequenceNumber}.");

                _listSentMessages.Add(update);
                _updateSequenceNumber++;
            }
            var subscriber = _redis.GetSubscriber();
            var updateAsJson = JsonConvert.SerializeObject(update);

            await subscriber.PublishAsync("Update-*", updateAsJson);
        }

        private void OnResendUpdate(RedisChannel channel, RedisValue value)
        {
            var tokens = value.ToString().Split('-');
            var fromSequenceNumber = int.Parse(tokens[0]);
            var toSequenceNumber = int.Parse(tokens[1]);

            var subscriber = _redis.GetSubscriber();

            lock (this)
            {
                for (int index = fromSequenceNumber - 1; index < _listSentMessages.Count && index < toSequenceNumber; index++)
                {
                    subscriber.Publish("Update-*", JsonConvert.SerializeObject(_listSentMessages[index]));
                }
            }
        }
    }
}