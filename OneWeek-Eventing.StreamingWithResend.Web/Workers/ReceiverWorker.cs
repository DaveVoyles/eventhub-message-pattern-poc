using OneWeek_Eventing.Common;
using OneWeek_Eventing.StreamingWithResend.Entities;
using OneWeek_Eventing.StreamingWithResend.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneWeek_Eventing.StreamingWithResend.Web.Workers
{
    public struct ReceiverStatus
    {
        public WorkerState State;
        public int UpdateReceived;
        public double AverageLatency;
    }


    public class ReceiverWorker : WorkerBase
    {
        private IReceiverProvider _receiverProvider;
        private int _updatesReceived = 0;
        private double _aggregatedLatency = 0.0;
        private int _lastReceivedSequenceNumber = 0;

        public ReceiverWorker(IReceiverProvider receiverProvider)
        {
            _receiverProvider = receiverProvider;
            _receiverProvider.OnUpdateReceived += OnUpdateReceived;
        }

        private void OnUpdateReceived(object sender, Update update)
        {
            lock (this)
            {
                if (State != WorkerState.Error)
                {
                    if ((_lastReceivedSequenceNumber + 1) < update.SequenceNumber)
                        // misaligned!
                        State = WorkerState.Error;
                    else
                    { 
                        _updatesReceived++;
                        _lastReceivedSequenceNumber = update.SequenceNumber;
                        _aggregatedLatency += (DateTime.UtcNow - update.PriceDate).TotalMilliseconds;
                    }
                }
            }
        }

        public async Task Start()
        {
            await _receiverProvider.Start();
            State = WorkerState.Running;
        }

        public async Task Stop()
        {
            await _receiverProvider.Stop();
            State = WorkerState.Stopped;
        }

        public ReceiverStatus GetStatus()
        {
            lock (this)
            {
                return new ReceiverStatus()
                {
                    State = State,
                    UpdateReceived = _updatesReceived,
                    AverageLatency = _aggregatedLatency / (double)_updatesReceived
                };
            }
        }
    }
}
