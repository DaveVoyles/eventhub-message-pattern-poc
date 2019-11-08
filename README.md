# Event Hub message pattern PoC
PoC to explore the [Competing Consumers Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/competing-consumers) with event hubs

#### Author(s): Dave Voyles | [@DaveVoyles](http://www.twitter.com/DaveVoyles)
#### URL: [www.DaveVoyles.com](http://www.davevoyles.com)

-----
This project was built in tandem with Microsoft's CSE (Commercial Software Engineering) team during our annual One Week hack in Seattle
to explore various messaging patterns utilizing a variety of 
Microsoft servives, including Event Hubs, Service Bus, Event Grid, Kafka, and Redis.

I worked on the Event Hub portion of this project, which is well documented and can be found in [feature/event-hub/OneWeek-Eventing.CompetitingConsumer.Provider.EventHub](https://github.com/DaveVoyles/eventhub-message-pattern-poc/tree/feature/event-hub/OneWeek-Eventing.CompetitingConsumer.Provider.EventHub)
directory.

### About the Competiting Consumers Pattern
Enable multiple concurrent consumers to process messages received on the same messaging channel. This enables a system to process multiple messages concurrently to optimize throughput, to improve scalability and availability, and to balance the workload.

---- 
### Instructions
You'll need to replace the connection strings and storage connections in the EventHubReceiverProvider.cs & EventHubSenderProvider.cs files. They are currently hardcoded for the PoC.

### Monitoring/Debug
To monitor/debug the messages in real time, install Service Bus Explorer. Once installed, in the menu bar select Actions-> Create Event Hub Listener.

Set the connection string to the one you use in the event hub namespace not the event hub itself. Set Event Hub Path to the name of the event hub you are listening in on. Set the Consumer group to one of the consumer groups you create in the Azure Portal. 

(instructions below). Do not use $Default, as the web app will use that one. 

### Setup
1. Create an [Event Hub Namespace & Event Hub][1] in Azure
		a. Create at least two consumer groups in the event hub
			i. $Default is used by the web app
			ii. The second group is used by Service Bus Explorer to monitor/debug the event hub
2. In Azure portal, add an SAS policy to the event hub with Manage permissions
		a. Copy your primary key & connection string-primary key
3. Create a storage account for Event Processor Host
4. Run the OneWeek-Eventing.CompetitingConsumer.Web project
5. Run the Postman scripts below


## Configure Postman Scripts
1. Using [Postman](https://www.getpostman.com/downloads/), load the *Event Hub Message PoC.postman_collection.json* file
2. Run scripts in this order:
  a. Generate Updates File
  b. Start Receiver
  c. Start Sender
  
You can then monitor their progress with Service Bus Explorer, or by running the *Get Sender Status* and *Get Receiver Status* Postman scripts. 
