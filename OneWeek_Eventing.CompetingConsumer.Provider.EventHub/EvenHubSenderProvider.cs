using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Azure.EventHubs;
using OneWeek_Eventing.CompetingConsumer.Entities;
using OneWeek_Eventing.CompetingConsumer.Interfaces;
using System.Text;

namespace OneWeek_Eventing.CompetingConsumer.Provider.EventHub
{
    public class EvenHubSenderProvider : ISenderProvider
    {
        private static EventHubClient eventHubClient;
        private const string EventHubConnectionString = "Endpoint=sb://dv-eventhub.servicebus.windows.net/;SharedAccessKeyName=manage-policy;SharedAccessKey=2Ryi+hFlavlPN3aEir0YsEEBhuCgtD7+kXnvvIs1VEw=;EntityPath=myeventhub";
        private const string EventHubName             = "myeventhub";

        private string _instrument;
        private int    _partitionCount;

        public Task Start(string instrument = null, int partitionCount = -1)
        {
            _instrument     = instrument;
            _partitionCount = partitionCount;

            // Creates an EventHubsConnectionStringBuilder object from the connection string, and sets the EntityPath.
            // Typically, the connection string should have the entity path in it, but this simple scenario
            // uses the connection string from the namespace.
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EventHubConnectionString)
            {
                EntityPath = EventHubName
            };
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            return Task.CompletedTask;
        }

        public async Task SendMessageAsync(Trade trade)
        {
            //if (String.IsNullOrEmpty(_instrument) || String.Compare(_instrument, trade.Instrument, false) == 0)
            //{
            //    var subscriber = _redis.GetSubscriber();
            //    var tradeAsJson = JsonConvert.SerializeObject(trade);

            //    await subscriber.PublishAsync($"Trades-{trade.Instrument}", tradeAsJson);
            //    if (String.IsNullOrEmpty(_instrument))
            //        await subscriber.PublishAsync("Trades-*", tradeAsJson);
            //    else if (_partitionCount != -1)
            //        await subscriber.PublishAsync($"Trades-{trade.GetPartitionIndex(_partitionCount)}", tradeAsJson);
            //}


            if (String.IsNullOrEmpty(_instrument) || String.Compare(_instrument, trade.Instrument, false) == 0)
            {
                var tradeAsJson    = JsonConvert.SerializeObject(trade);

                await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(tradeAsJson))); // TODO: put trade here
            }
        }

        public Task Stop()
        {
            // TODO: Probably need to replace this line
            return Task.CompletedTask;
        }
    }
}

