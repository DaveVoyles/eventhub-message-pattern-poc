using OneWeek_Eventing.Filtering.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OneWeek_Eventing.Filtering.Interfaces
{
    public interface IReceiverProvider
    {
        Task Start(string market = null, string instrument = null);
        Task Stop();

        event EventHandler<Order> OnOrderReceived;
    }
}
