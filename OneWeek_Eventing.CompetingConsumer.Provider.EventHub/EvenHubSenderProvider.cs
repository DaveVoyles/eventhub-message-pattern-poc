using System;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using OneWeek_Eventing.CompetingConsumer.Entities;
using OneWeek_Eventing.CompetingConsumer.Interfaces;

namespace OneWeek_Eventing.CompetingConsumer.Provider.EventHub
{
    public class EvenHubSenderProvider : ISenderProvider
    {
        private static EventHubClient eventHubClient;
        private const string EventHubConnectionString = "Endpoint=sb://dv-eventhub.servicebus.windows.net/;SharedAccessKeyName=manage-policy;SharedAccessKey=2Ryi+hFlavlPN3aEir0YsEEBhuCgtD7+kXnvvIs1VEw=;EntityPath=myeventhub";
        private const string EventHubName = "myeventhub";

        public Task Start(string instrument = null, int partitionCount = -1)
        {
            // Creates an EventHubsConnectionStringBuilder object from the connection string, and sets the EntityPath.
            // Typically, the connection string should have the entity path in it, but this simple scenario
            // uses the connection string from the namespace.
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EventHubConnectionString)
            {
                EntityPath = EventHubName
            };

            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            //await SendMessagesToEventHub(100);
            //await eventHubClient.CloseAsync();

            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();

            return Task.CompletedTask;
        }

        public async Task SendMessageAsync(Trade trade)
        {
        }
        public Task Stop()
        {
            // TODO: Probably need to replace this line
            return Task.CompletedTask;
        }

        // Uses the event hub client to send 100 messages to the event hub.
        //private static async Task SendMessagesToEventHub(int numMessagesToSend)
        //{
        //    for (var i = 0; i < numMessagesToSend; i++)
        //    {
        //        try
        //        {
        //            var message = $"Message {i}";
        //            Console.WriteLine($"Sending message: {message}");
        //            await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
        //        }
        //        catch (Exception exception)
        //        {
        //            Console.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
        //        }
        //        await Task.Delay(10);
        //    }
        //    Console.WriteLine($"{numMessagesToSend} messages sent.");
        //}


    }
}

