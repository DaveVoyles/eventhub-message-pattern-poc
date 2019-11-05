using Newtonsoft.Json;
using OneWeek_Eventing.Common.Providers.Redis;
using OneWeek_Eventing.CompetingConsumer.Entities;
using OneWeek_Eventing.CompetingConsumer.Interfaces;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace OneWeek_Eventing.CompetingConsumer.Provider.Redis
{
    public class RedisSenderProvider : ISenderProvider
    {
        private string _instrument;
        private int _partitionCount;
        private ConnectionMultiplexer _redis;

        // ""
        public Task Start(string instrument = null, int partitionCount = -1)
        {
            _instrument = instrument;
            _partitionCount = partitionCount;

            _redis = ConnectionMultiplexer.Connect(RedisConfiguration.GetConnectionString());
            return Task.CompletedTask;
        }

        public async Task SendMessageAsync(Trade trade)
        {
            if (String.IsNullOrEmpty(_instrument) || String.Compare(_instrument, trade.Instrument, false) == 0)
            {
                var subscriber = _redis.GetSubscriber();
                var tradeAsJson = JsonConvert.SerializeObject(trade);

                await subscriber.PublishAsync($"Trades-{trade.Instrument}", tradeAsJson);
                if (String.IsNullOrEmpty(_instrument))
                    await subscriber.PublishAsync("Trades-*", tradeAsJson);
                else if (_partitionCount != -1)
                    await subscriber.PublishAsync($"Trades-{trade.GetPartitionIndex(_partitionCount)}", tradeAsJson);
            }
        }

        public Task Stop()
        {
            _redis.Close();
            _redis.Dispose();
            _redis = null;
            return Task.CompletedTask;
        }
    }
}
