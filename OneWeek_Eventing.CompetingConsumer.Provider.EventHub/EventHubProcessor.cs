using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using OneWeek_Eventing.CompetingConsumer.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OneWeek_Eventing.CompetingConsumer.Provider.EventHub
{
    public class EventHubProcessor : IEventProcessor
    {
        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine($"Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.");
            return Task.CompletedTask;
        }

        public Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine($"SimpleEventProcessor initialized. Partition: '{context.PartitionId}'");
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            Console.WriteLine($"Error on Partition: {context.PartitionId}, Error: {error.Message}");
            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (var eventData in messages)
            {
                var data    = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                var rawData = JsonConvert.DeserializeObject<Trade>(data);

                EventHubReceiverProvider.current.SendTradeEvent(rawData);
                Console.WriteLine($"Message received. Partition: '{context.PartitionId}', Data: '{rawData}'");
            }
            return context.CheckpointAsync();
        }
    }
}
