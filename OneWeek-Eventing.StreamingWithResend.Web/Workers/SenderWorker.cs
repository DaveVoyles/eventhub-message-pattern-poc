using OneWeek_Eventing.Common;
using OneWeek_Eventing.StreamingWithResend.Entities;
using OneWeek_Eventing.StreamingWithResend.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OneWeek_Eventing.StreamingWithResend.Web.Workers
{
    public struct SenderStatus
    {
        public WorkerState State;
        public int LatestSent;
        public double LatestSentPerSecond;
    }

    public class SenderWorker : WorkerBase
    {
        private readonly ISenderProvider _senderProvider;

        private int _latestSent = 0;
        private double _latestSentPerSecond = 0.0;
        private readonly int ReportInterval = 1000;
        private Dictionary<string, Latest> _latestMessages = new Dictionary<string, Latest>();

        public SenderWorker(ISenderProvider senderProvider)
        {
            _senderProvider = senderProvider;
        }

        public async Task RunAsync(CancellationToken cancellationToken, int delayBetweenLatest = 0, int latestPerSecond = 0 /* TODO */)
        {
            if (_senderProvider == null)
                throw new NullReferenceException("No concrete sender provider supplied.");

            await _senderProvider.Start();
            lock (this)
            {
                State = WorkerState.Running;
            }

            DateTime startReportInterval = DateTime.UtcNow;
            int newLatestSent = _latestSent;
            int sequenceNumber = 0;
            var rand = new Random((int)DateTime.UtcNow.Ticks);

            while (!cancellationToken.IsCancellationRequested)
            {
                var instrument = Constants.Instruments[rand.Next(Constants.Instruments.Length - 1)];
                if (!_latestMessages.TryGetValue(instrument, out Latest latest))
                {
                    var priceOpen = rand.NextDouble() * 200.0;
                    latest = new Latest()
                    {
                        Instrument = instrument,
                        PriceClose = priceOpen + 2.0,
                        PriceOpen = priceOpen,
                        PriceHigh = priceOpen,
                        PriceLow = priceOpen,
                        PriceLatest = priceOpen,
                        VolumeTradedToday = 0
                    };
                }

                latest.SequenceNumber = ++sequenceNumber;
                latest.PriceLatest += (rand.NextDouble() > 0.5 ? 1 : -1) * (rand.NextDouble() * 0.5);
                if (latest.PriceLatest < latest.PriceLow)
                    latest.PriceLow = latest.PriceLatest;
                if (latest.PriceLatest > latest.PriceHigh)
                    latest.PriceHigh = latest.PriceLatest;
                latest.VolumeLatest = rand.NextDouble() * 1000;
                latest.VolumeTradedToday += latest.VolumeLatest;
                latest.PriceDate = DateTime.UtcNow;

                await _senderProvider.SendMessageAsync(latest);
                _latestMessages[latest.Instrument] = latest;

                lock (this)
                    newLatestSent++;

                if (delayBetweenLatest > 0)
                    await Task.Delay(delayBetweenLatest);

                if (DateTime.UtcNow > (startReportInterval + TimeSpan.FromMilliseconds(ReportInterval)))
                {
                    lock (this)
                    {
                        _latestSentPerSecond = ((double)(newLatestSent - _latestSent) / (DateTime.UtcNow - startReportInterval).TotalSeconds);
                        _latestSent = newLatestSent;
                    }

                    startReportInterval = DateTime.UtcNow;
                }
            }
            lock (this)
            {
                _latestSentPerSecond = ((double)(newLatestSent - _latestSent) / (DateTime.UtcNow - startReportInterval).TotalSeconds);
                _latestSent = newLatestSent;
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
                    LatestSent = _latestSent,
                    LatestSentPerSecond = _latestSentPerSecond
                };
            }
        }
    }
}
