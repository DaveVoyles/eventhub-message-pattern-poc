using Newtonsoft.Json;
using OneWeek_Eventing.Common.Providers.Redis;
using Order = OneWeek_Eventing.Filtering.Entities.Order;
using OneWeek_Eventing.Filtering.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OneWeek_Eventing.Filtering.Provider.Redis
{
    public class RedisReceiverProvider : IReceiverProvider
    {
        private string _market;
        private string _instrument;
        private ConnectionMultiplexer _redis;

        public event EventHandler<Order> OnOrderReceived;

        public Task Start(string market = null, string instrument =null)
        {
            _market = market;
            _instrument = instrument;

            _redis = ConnectionMultiplexer.Connect(RedisConfiguration.GetConnectionString());

            var subscriber = _redis.GetSubscriber();

            if (!String.IsNullOrEmpty(_market) && !String.IsNullOrEmpty(_instrument))
                subscriber.Subscribe($"Orders-{_market}-{_instrument}", OnOrder);
            else if (!String.IsNullOrEmpty(_instrument))
                subscriber.Subscribe($"Orders-*-{_instrument}", OnOrder);
            else
                subscriber.Subscribe($"Orders-*-*", OnOrder);
            return Task.CompletedTask;
        }

        private void OnOrder(RedisChannel channel, RedisValue value)
        {
            OnOrderReceived?.Invoke(this, JsonConvert.DeserializeObject<Order>(value));
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
