using OneWeek_Eventing.Common;
using OneWeek_Eventing.Filtering.Entities;
using OneWeek_Eventing.Filtering.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneWeek_Eventing.Filtering.Web.Workers
{
    public struct SenderStatus
    {
        public WorkerState State;
        public int OrdersSent;
        public double OrdersSentPerSecond;
    }

    public class SenderWorker : WorkerBase
    {
        private readonly ISenderProvider _senderProvider;

        private int _ordersSent = 0;
        private double _ordersSentPerSecond = 0.0;
        private readonly int ReportInterval = 1000;

        public SenderWorker(ISenderProvider senderProvider)
        {
            _senderProvider = senderProvider;
        }

        public async Task RunAsync(IEnumerable<Order> orders, int delayBetweenOrders = 0, int ordersPerSecond = 0 /* TODO */)
        {
            if (_senderProvider == null)
                throw new NullReferenceException("No concrete sender provider supplied.");

            await _senderProvider.Start();
            lock (this)
            {
                State = WorkerState.Running;
            }

            DateTime startReportInterval = DateTime.UtcNow;
            int newOrdersSent = _ordersSent;
            int sequenceNumber = 0;

            foreach (var order in orders)
            {
                order.SequenceNumber = sequenceNumber++;
                order.OrderDate = DateTime.UtcNow;
                await _senderProvider.SendMessageAsync(order);
                lock (this)
                    newOrdersSent++;
                if (delayBetweenOrders > 0)
                    await Task.Delay(delayBetweenOrders);

                if (DateTime.UtcNow > (startReportInterval + TimeSpan.FromMilliseconds(ReportInterval)))
                {
                    lock (this)
                    {
                        _ordersSentPerSecond = ((double)(newOrdersSent - _ordersSent) / (DateTime.UtcNow - startReportInterval).TotalSeconds);
                        _ordersSent = newOrdersSent;
                    }

                    startReportInterval = DateTime.UtcNow;
                }
            }
            lock (this)
            {
                _ordersSentPerSecond = ((double)(newOrdersSent - _ordersSent) / (DateTime.UtcNow - startReportInterval).TotalSeconds);
                _ordersSent = newOrdersSent;
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
                    OrdersSent = _ordersSent,
                    OrdersSentPerSecond = _ordersSentPerSecond
                };
            }
        }
    }
}
