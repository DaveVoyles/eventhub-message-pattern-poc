using OneWeek_Eventing.CompetingConsumer.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OneWeek_Eventing.CompetingConsumer.Interfaces
{
    public interface IReceiverProvider
    {
        Task Start(string instrument, bool usePartitions, int partitionId = -1, int partitionCount = -1);
        Task Stop();

        event EventHandler<Trade> OnTradeReceived;
    }
}
