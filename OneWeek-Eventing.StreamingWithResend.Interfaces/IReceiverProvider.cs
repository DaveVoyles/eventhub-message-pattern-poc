using OneWeek_Eventing.StreamingWithResend.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OneWeek_Eventing.StreamingWithResend.Interfaces
{
    public interface IReceiverProvider
    {
        Task Start();
        Task Stop();

        event EventHandler<Latest> OnLatestReceived;
    }
}
