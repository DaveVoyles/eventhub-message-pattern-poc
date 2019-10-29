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
        public int LatestReceived;
        public double AverageLatency;
    }


    public class ReceiverWorker : WorkerBase
    {
        private IReceiverProvider _receiverProvider;
        private int _latestReceived = 0;
        private double _aggregatedLatency = 0.0;
        private int _lastReceivedSequenceNumber = 0;

        public ReceiverWorker(IReceiverProvider receiverProvider)
        {
            _receiverProvider = receiverProvider;
            _receiverProvider.OnLatestReceived += OnLatestReceived;
        }

        private void OnLatestReceived(object sender, Update latest)
        {
            lock (this)
            {
                if (State != WorkerState.Error)
                {
                    if ((_lastReceivedSequenceNumber + 1) < latest.SequenceNumber)
                        // misaligned!
                        State = WorkerState.Error;
                    else
                    { 
                        _latestReceived++;
                        _aggregatedLatency += (DateTime.UtcNow - latest.PriceDate).TotalMilliseconds;
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
                    LatestReceived = _latestReceived,
                    AverageLatency = _aggregatedLatency / (double)_latestReceived
                };
            }
        }
    }
}
