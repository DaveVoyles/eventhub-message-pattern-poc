﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using OneWeek_Eventing.CompetingConsumer.Entities;
using OneWeek_Eventing.CompetingConsumer.Interfaces;


namespace OneWeek_Eventing.CompetingConsumer.Provider.EventHub
{
    public class EventHubReceiverProvider : IReceiverProvider
    {
        // Event Hub connections
        private const string EventHubConnectionString = "Endpoint=sb://dv-eventhub.servicebus.windows.net/;SharedAccessKeyName=manage-policy;SharedAccessKey=2Ryi+hFlavlPN3aEir0YsEEBhuCgtD7+kXnvvIs1VEw=;EntityPath=myeventhub";
        private const string EventHubName             = "myeventhub";
        private static EventHubClient eventHubClient;

        // Storage
        private const string StorageContainerName              = "hubcontainer";
        private const string StorageAccountName                = "dveventhub";
        private const string StorageAccountKey                 = "6XY4vaNFeafilm1n5rU4toc1a6Y4IlqfMRoWuMz1ESpTQjM8a2A5El2NEjKXNPEE87qUbs+y69V9A4uM+/YR9g==";
        private static readonly string StorageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);

        private string _instrument;
    
        // Get context for EventHubReceiverProvider, to be used in/by EventHubProcessor
        public static EventHubReceiverProvider current;
        public EventHubReceiverProvider()
        {
            current = this;
        }

        public async Task Start(string instrument, bool usePartitions, int partitionIndex = -1, int partitionCount = -1)
        {
            _instrument = instrument;

            // Connect to event processor using Event Hub & Storage credentials
            var eventProcessorHost = new EventProcessorHost(
                EventHubName,
                PartitionReceiver.DefaultConsumerGroupName,
                EventHubConnectionString,
                StorageConnectionString,
                StorageContainerName);

            // Registers the Event Processor Host and starts receiving messages
            await eventProcessorHost.RegisterEventProcessorAsync<EventHubProcessor>();

            // Disposes of the Event Processor Host
            await eventProcessorHost.UnregisterEventProcessorAsync();
        }

        // Trade events
        public event EventHandler<Trade> OnTradeReceived;
        public void SendTradeEvent(Trade t)
        {
            OnTradeReceived?.Invoke(this, t);
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }
    }
}