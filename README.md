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
