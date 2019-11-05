using OneWeek_Eventing.StreamingWithResend.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OneWeek_Eventing.StreamingWithResend.Interfaces
{
    public interface ISenderProvider
    {
        Task Start();
        Task Stop();

        Task SendMessageAsync(Update update);
    }
}
