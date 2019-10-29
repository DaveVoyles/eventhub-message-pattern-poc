using System;
using System.Collections.Generic;
using System.Text;

namespace OneWeek_Eventing.Filtering.Entities
{
    public class Order
    {
        public int SequenceNumber { get; set; }
        public string Market { get; set; }
        public string Instrument { get; set; }
        public bool IsBuy { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
