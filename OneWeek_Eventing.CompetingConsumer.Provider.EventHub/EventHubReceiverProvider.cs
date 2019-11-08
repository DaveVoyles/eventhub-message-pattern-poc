using System;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using OneWeek_Eventing.CompetingConsumer.Entities;
using OneWeek_Eventing.CompetingConsumer.Interfaces;

namespace OneWeek_Eventing.CompetingConsumer.Provider.EventHub
{
    public class EventHubReceiverProvider : IReceiverProvider
    {
        // Event Hub connections
        private const string EventHubConnectionString = "";
        private const string EventHubName             = "";

        // Storage
        private const string StorageContainerName              = "";
        private const string StorageAccountName                = "";
        private const string StorageAccountKey                 = "";
        private static readonly string StorageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);

        private EventProcessorHost eventProcessorHost;

        // Get context for EventHubReceiverProvider, to be used in/by EventHubProcessor
        public static EventHubReceiverProvider current;
        public EventHubReceiverProvider()
        {
            current = this;
        }

        /* ::Competing Consumers Pattern:: Enable multiple concurrent consumers to process messages received on 
         * the same messaging channel. This pattern enables a system to process multiple messages concurrently
         * to optimize throughput, improve scalability and availability, and to balance the workload. */
        public async Task Start(string instrument, bool usePartitions, int partitionIndex = -1, int partitionCount = -1)
        {
            // Connect to event processor using Event Hub & Storage credentials
            eventProcessorHost = new EventProcessorHost(
                EventHubName                              ,
                PartitionReceiver.DefaultConsumerGroupName,
                EventHubConnectionString                  ,
                StorageConnectionString                   , 
                StorageContainerName                     );

            /* Registering an event processor class with an instance of EventProcessorHost starts event processing.
             * Registering instructs the Event Hubs service to expect that the consumer app consumes
             * events from some of its partitions, and to invoke the IEventProcessor implementation 
             * code whenever it pushes events to consume. */
            await eventProcessorHost.RegisterEventProcessorAsync<EventHubProcessor>();      
        }

        // Trade events, invoked when a trade is received
        public event EventHandler<Trade> OnTradeReceived;
        public void SendTradeEvent(Trade t)
        {
            OnTradeReceived?.Invoke(this, t);
        }

        public async Task Stop()
        {
            // Disposes of the Event Processor Host lease, which allows other consumers to attach to partition.
            // Only a single reader at a time can read from any given partition within a consumer group.
            await eventProcessorHost.UnregisterEventProcessorAsync();
        }
    }
}
