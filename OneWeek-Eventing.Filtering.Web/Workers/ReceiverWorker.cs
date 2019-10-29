using OneWeek_Eventing.Common;
using OneWeek_Eventing.Filtering.Entities;
using OneWeek_Eventing.Filtering.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneWeek_Eventing.Filtering.Web.Workers
{
    public struct ReceiverStatus
    {
        public WorkerState State;
        public int OrdersReceived;
        public double AverageLatency;
    }


    public class ReceiverWorker : WorkerBase
    {
        private IReceiverProvider _receiverProvider;
        private string _market;
        private string _instrument;
        private int _ordersReceived = 0;
        private double _aggregatedLatency = 0.0;

        public ReceiverWorker(IReceiverProvider receiverProvider)
        {
            _receiverProvider = receiverProvider;
            _receiverProvider.OnOrderReceived += OnOrderReceived;
        }

        private void OnOrderReceived(object sender, Order order)
        {
            lock (this)
            {
                if (State != WorkerState.Error)
                {
                    if ((String.IsNullOrEmpty(_market) || order.Market.CompareTo(_market) == 0) &&
                        (String.IsNullOrEmpty(_instrument) || order.Instrument.CompareTo(_instrument) == 0))
                    { 
                        _ordersReceived++;
                        _aggregatedLatency += (DateTime.UtcNow - order.OrderDate).TotalMilliseconds;
                    }
                    else
                        // misaligned!
                        State = WorkerState.Error;
                }
            }
        }

        public async Task Start(string market = null, string instrument = null)
        {
            await _receiverProvider.Start(market, instrument);
            State = WorkerState.Running;
        }

        public async Task Stop()
        {
            await _receiverProvider.Stop();
            _market = null;
            _instrument = null;
            State = WorkerState.Stopped;
        }

        public ReceiverStatus GetStatus()
        {
            lock (this)
            {
                return new ReceiverStatus()
                {
                    State = State,
                    OrdersReceived = _ordersReceived,
                    AverageLatency = _aggregatedLatency / (double)_ordersReceived
                };
            }
        }
    }
}
