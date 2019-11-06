using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Azure.EventHubs;
using OneWeek_Eventing.CompetingConsumer.Entities;
using OneWeek_Eventing.CompetingConsumer.Interfaces;


namespace OneWeek_Eventing.CompetingConsumer.Provider.EventHub
{
    public class EventHubReceiverProvider : IReceiverProvider
    {

        // Event Hub connections
        // TODO: Refactor to have these in dot-net secrets instead
        private const string EventHubConnectionString = "Endpoint=sb://dv-eventhub.servicebus.windows.net/;SharedAccessKeyName=manage-policy;SharedAccessKey=2Ryi+hFlavlPN3aEir0YsEEBhuCgtD7+kXnvvIs1VEw=;EntityPath=myeventhub";
        private const string EventHubName             = "myeventhub";
        private static EventHubClient eventHubClient;

        private string _instrument;

        public Task Start(string instrument, bool usePartitions, int partitionIndex = -1, int partitionCount = -1)
        {
            _instrument = instrument;

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EventHubConnectionString)
            {
                EntityPath = EventHubName
            };
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            return Task.CompletedTask;
        }

        public event EventHandler<Trade> OnTradeReceived;

        public Task Start(string instrument, bool usePartitions, int partitionIndex = -1, int partitionCount = -1)
        {
            throw new NotImplementedException();
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }
    }
}
