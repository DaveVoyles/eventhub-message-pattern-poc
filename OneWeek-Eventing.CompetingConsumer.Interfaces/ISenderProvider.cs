using OneWeek_Eventing.CompetingConsumer.Entities;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace OneWeek_Eventing.CompetingConsumer.Interfaces
{
    public interface ISenderProvider
    {
        Task Start(string instrument = null);
        Task Stop();

        Task SendMessageAsync(Trade trade);
    }
}
