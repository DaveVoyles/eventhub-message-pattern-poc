using Newtonsoft.Json;
using OneWeek_Eventing.Common.Providers.Redis;
using OneWeek_Eventing.Filtering.Interfaces;
using Order = OneWeek_Eventing.Filtering.Entities.Order;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace OneWeek_Eventing.Filtering.Provider.Redis
{
    public class RedisSenderProvider : ISenderProvider
    {
        private string _instrument;
        private ConnectionMultiplexer _redis;

        public Task Start()
        {
            _redis = ConnectionMultiplexer.Connect(RedisConfiguration.GetConnectionString());
            return Task.CompletedTask;
        }

        public async Task SendMessageAsync(Order order)
        {
            var subscriber = _redis.GetSubscriber();
            var orderAsJson = JsonConvert.SerializeObject(order);

            await subscriber.PublishAsync($"Orders-{order.Market}-{order.Instrument}", orderAsJson);
            await subscriber.PublishAsync($"Orders-*-{order.Instrument}", orderAsJson);
            await subscriber.PublishAsync("Trades-*-*", orderAsJson);
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
