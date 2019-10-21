using System;
using System.Collections.Generic;
using System.Text;

namespace OneWeek_Eventing.StreamingWithResend.Entities
{
    public class Latest
    {
        public int SequenceNumber { get; set; }
        public string Instrument { get; set; }
        public DateTime PriceDate { get; set; }
        public double PriceOpen { get; set; }
        public double PriceClose { get; set; }
        public double PriceHigh { get; set; }
        public double PriceLow { get; set; }
        public double PriceLatest { get; set; }
        public double VolumeLatest { get; set; }
        public double VolumeTradedToday { get; set; }

        // we can add a lot more here, but it's fleshed out enough for this scenario
    }
}
