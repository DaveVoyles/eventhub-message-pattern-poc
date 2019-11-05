using OneWeek_Eventing.Common;
using OneWeek_Eventing.CompetingConsumer.Entities;
using OneWeek_Eventing.CompetingConsumer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneWeek_Eventing.CompetingConsumer.Web.Workers
{
    public struct ReceiverStatus
    {
        public WorkerState State;
        public int TradesReceived;
        public double AverageLatency;
    }


    public class ReceiverWorker : WorkerBase
    {
        private IReceiverProvider _receiverProvider;
        private int _tradesReceived = 0;
        private double _aggregatedLatency = 0.0;
        private List<Trade> _receivedTrades = new List<Trade>();
        private Dictionary<string, int> _receivedTradeNumbers = new Dictionary<string, int>();

        public ReceiverWorker(IReceiverProvider receiverProvider)
        {
            _receiverProvider = receiverProvider;
            _receiverProvider.OnTradeReceived += OnTradeReceived;
        }

        private void OnTradeReceived(object sender, Trade trade)
        {
            lock (this)
            {
                if (State != WorkerState.Error)
                {
                    if (!_receivedTradeNumbers.TryGetValue(trade.Instrument, out int tradeNumber))
                        tradeNumber = 0;

                    if ((tradeNumber + 1) > trade.TradeNumber)
                        // skip this since we already received it (we want to receive it exactly once)
                        return;
                    else if ((tradeNumber + 1) == trade.TradeNumber)
                    {
                        // it's in order - let's store and move on
                        _receivedTrades.Add(trade);
                        _receivedTradeNumbers[trade.Instrument] = trade.TradeNumber;
                        _tradesReceived++;
                        _aggregatedLatency += (DateTime.UtcNow - trade.TradeDate).TotalMilliseconds;
                    }
                    else
                        // misaligned!
                        State = WorkerState.Error;
                }
            }
        }

        public async Task Start(string instrument, bool usePartitions, int partitionIndex = -1, int partitionCount = -1)
        {
            await _receiverProvider.Start(instrument, usePartitions, partitionIndex, partitionCount);
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
                    TradesReceived = _tradesReceived,
                    AverageLatency = _aggregatedLatency / (double)_tradesReceived
                };
            }
        }
    }
}