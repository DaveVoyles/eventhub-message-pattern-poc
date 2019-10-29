using System;
using System.Collections.Generic;
using System.Text;

namespace OneWeek_Eventing.Common
{
    public static class Constants
    {
        public static readonly string[] Instruments =
        {
            "ERIC B",
            "HM B",
            "SAND",
            "TELI",
            "TELE2"
        };

        public static readonly string[] Markets =
        {
            "SFB",
            "Nasdaq",
            "NYSE"
        };

        public static readonly string TradesFile = "default.trades";
        public static readonly string OrdersFile = "default.orders";
        public static readonly string UpdatesFile = "default.updates";
    }
}
