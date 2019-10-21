using System;

namespace OneWeek_Eventing.CompetingConsumer.Entities
{
    public class Trade
    {
        public int SequenceNumber { get; set; }
        public string Instrument { get; set; }
        public int TradeNumber { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }
        public DateTime TradeDate { get; set; }
        public string Buyer { get; set; }
        public string Seller { get; set; }
    }
}
