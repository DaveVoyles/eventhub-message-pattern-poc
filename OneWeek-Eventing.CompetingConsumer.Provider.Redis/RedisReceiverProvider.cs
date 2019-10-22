using Newtonsoft.Json;
using OneWeek_Eventing.Common;
using OneWeek_Eventing.Common.Providers.Redis;
using OneWeek_Eventing.CompetingConsumer.Entities;
using OneWeek_Eventing.CompetingConsumer.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using System.Threading.Tasks;

namespace OneWeek_Eventing.CompetingConsumer.Provider.Redis
{
    public class RedisReceiverProvider : IReceiverProvider
    {
        private string _instrument;
        private ConnectionMultiplexer _redis;

        public event EventHandler<Trade> OnTradeReceived;
        
        public Task Start(string instrument, bool usePartitions, int partitionId = -1, int partitionCount = -1)
        {
            _instrument = instrument;

            _redis = ConnectionMultiplexer.Connect(RedisConfiguration.GetConnectionString());

            var subscriber = _redis.GetSubscriber();

            if (String.IsNullOrEmpty(_instrument))
            {
                if (usePartitions)
                {
                    if (partitionId == -1 && partitionCount == -1)
                    {
                        foreach (var instrumentPartition in Constants.Instruments)
                        {
                            subscriber.Subscribe($"Trades-{instrumentPartition}", OnTrade);
                        }
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                    subscriber.Subscribe("Trades-*", OnTrade);
            }
            else
                subscriber.Subscribe($"Trades-{_instrument}", OnTrade);
            return Task.CompletedTask;
        }

        private void OnTrade(RedisChannel channel, RedisValue value)
        {
            OnTradeReceived?.Invoke(this, JsonConvert.DeserializeObject<Trade>(value));
        }

        public Task Stop()
        {
            var subscriber = _redis.GetSubscriber();
            subscriber.UnsubscribeAll();

            _redis.Close();
            _redis.Dispose();
            _redis = null;
            return Task.CompletedTask;
        }
    }
}
