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
        public int UpdatesSent;
        public double UpdatesSentPerSecond;
    }

    public class SenderWorker : WorkerBase
    {
        private readonly ISenderProvider _senderProvider;

        private int _updatesSent = 0;
        private double _updatesSentPerSecond = 0.0;
        private readonly int ReportInterval = 1000;

        public SenderWorker(ISenderProvider senderProvider)
        {
            _senderProvider = senderProvider;
        }

        public async Task RunAsync(IEnumerable<Update> updates, int delayBetweenUpdates = 0, int updatesPerSecond = 0 /* TODO */)
        {
            if (_senderProvider == null)
                throw new NullReferenceException("No concrete sender provider supplied.");

            await _senderProvider.Start();
            lock (this)
            {
                State = WorkerState.Running;
            }

            DateTime startReportInterval = DateTime.UtcNow;
            int newUpdatesSent = _updatesSent;
            int sequenceNumber = 0;

            foreach (var update in updates)
            {
                update.SequenceNumber = ++sequenceNumber;
                update.PriceDate = DateTime.UtcNow;
                await _senderProvider.SendMessageAsync(update);

                lock (this)
                    newUpdatesSent++;

                if (delayBetweenUpdates > 0)
                    await Task.Delay(delayBetweenUpdates);

                if (DateTime.UtcNow > (startReportInterval + TimeSpan.FromMilliseconds(ReportInterval)))
                {
                    lock (this)
                    {
                        _updatesSentPerSecond = ((double)(newUpdatesSent - _updatesSent) / (DateTime.UtcNow - startReportInterval).TotalSeconds);
                        _updatesSent = newUpdatesSent;
                    }

                    startReportInterval = DateTime.UtcNow;
                }
            }

            lock (this)
            {
                _updatesSentPerSecond = ((double)(newUpdatesSent - _updatesSent) / (DateTime.UtcNow - startReportInterval).TotalSeconds);
                _updatesSent = newUpdatesSent;
            }

            /*
            await _senderProvider.Stop();
            lock (this)
            {
                State = WorkerState.Stopped;
            }
            */
        }

        public SenderStatus GetStatus()
        {
            lock (this)
            {
                return new SenderStatus()
                {
                    State = State,
                    UpdatesSent = _updatesSent,
                    UpdatesSentPerSecond = _updatesSentPerSecond
                };
            }
        }
    }
}
