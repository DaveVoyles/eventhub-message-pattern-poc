using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Azure.EventHubs;
using OneWeek_Eventing.CompetingConsumer.Entities;
using OneWeek_Eventing.CompetingConsumer.Interfaces;
using System.Text;

namespace OneWeek_Eventing.CompetingConsumer.Provider.EventHub
{
    public class EventHubSenderProvider : ISenderProvider
    {
        // Event Hub connections
        private static EventHubClient eventHubClient;
        private const string EventHubConnectionString = "";
        private const string EventHubName             = "";

        // Vars specific to the trade
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
            if (String.IsNullOrEmpty(_instrument) || String.Compare(_instrument, trade.Instrument, false) == 0)
            {
                // Convert the trade to JSON, then encode in a format Event Hub accepts
                var tradeAsJson    = JsonConvert.SerializeObject(trade);
                var encodedTrade   = Encoding.UTF8.GetBytes(tradeAsJson);

                // Send message to Event Hub. Use trade.Instrument as the index to track for ordering.
                // All trades with the same instrument will be stored in the same partition in the Event Hub
                await eventHubClient.SendAsync(new EventData(encodedTrade), trade.Instrument);
            }
        }

        public Task Stop()
        {
            return Task.CompletedTask;
        }
    }
}

