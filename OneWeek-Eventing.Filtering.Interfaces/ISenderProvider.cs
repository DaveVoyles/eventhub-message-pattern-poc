using OneWeek_Eventing.Filtering.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OneWeek_Eventing.Filtering.Interfaces
{
    public interface ISenderProvider
    {
        Task Start();
        Task Stop();

        Task SendMessageAsync(Order order);
    }
}
