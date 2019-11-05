using OneWeek_Eventing.Common;
using OneWeek_Eventing.CompetingConsumer.Entities;
using OneWeek_Eventing.CompetingConsumer.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneWeek_Eventing.CompetingConsumer.Web.Workers
{
    public struct SenderStatus
    {
        public WorkerState State;
        public int TradesSent;
        public double TradesSentPerSecond;
    }

    public class SenderWorker : WorkerBase
    {
        private readonly ISenderProvider _senderProvider;

        private int _tradesSent = 0;
        private double _tradesSentPerSecond = 0.0;
        private readonly int ReportInterval = 1000;

        public SenderWorker(ISenderProvider senderProvider)
        {
            _senderProvider = senderProvider;
        }

        public async Task RunAsync(IEnumerable<Trade> trades, string instrument = null, int? partitionCount = null, int delayBetweenTrades = 0, int tradesPerSecond = 0 /* TODO */)
        {
            if (_senderProvider == null)
                throw new NullReferenceException("No concrete sender provider supplied.");

            await _senderProvider.Start(instrument, partitionCount ?? -1);
            lock(this)
            {
                State = WorkerState.Running;
            }

            DateTime startReportInterval = DateTime.UtcNow;
            int newTradesSent = _tradesSent;
            int sequenceNumber = 0;

            foreach (var trade in trades)
            {
                if (String.IsNullOrEmpty(instrument) || String.Compare(instrument, trade.Instrument, true) == 0)
                {
                    trade.SequenceNumber = sequenceNumber++;
                    trade.TradeDate = DateTime.UtcNow;
                    await _senderProvider.SendMessageAsync(trade);
                    lock (this)
                        newTradesSent++;
                    if (delayBetweenTrades > 0)
                        await Task.Delay(delayBetweenTrades);
                }

                if (DateTime.UtcNow > (startReportInterval + TimeSpan.FromMilliseconds(ReportInterval)))
                {
                    lock (this)
                    {
                        _tradesSentPerSecond = ((double)(newTradesSent - _tradesSent) / (DateTime.UtcNow - startReportInterval).TotalSeconds);
                        _tradesSent = newTradesSent;
                    }

                    startReportInterval = DateTime.UtcNow;
                }
            }
            lock(this)
            {
                _tradesSentPerSecond = ((double)(newTradesSent - _tradesSent) / (DateTime.UtcNow - startReportInterval).TotalSeconds);
                _tradesSent = newTradesSent;
            }

            await _senderProvider.Stop();
            lock (this)
            {
                State = WorkerState.Stopped;
            }
        }

        public SenderStatus GetStatus()
        {
            lock (this)
            {
                return new SenderStatus()
                {
                    State = State,
                    TradesSent = _tradesSent,
                    TradesSentPerSecond = _tradesSentPerSecond
                };
            }
        }
    }
}
