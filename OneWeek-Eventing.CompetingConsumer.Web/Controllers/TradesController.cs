using Microsoft.AspNetCore.Mvc;
using OneWeek_Eventing.Common;
using OneWeek_Eventing.CompetingConsumer.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

namespace OneWeek_Eventing.CompetingConsumer.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradesController : ControllerBase
    {
        // GET api/sender
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var files = System.IO.Directory.EnumerateFiles(".", "*.trades");
            if (files != null)
            {
                return new ActionResult<IEnumerable<string>>(files);
            }
            else
            {
                return new string[] { "No trade files found" };
            }
        }

        // GET api/trades/defaults.trade
        [HttpGet("{tradesFile}")]
        public ActionResult<string> Get(string tradesFile)
        {
            try
            {
                return System.IO.File.ReadAllText(tradesFile);
            }
            catch (Exception)
            {
                return "Didn't find this file";
            }
        }

        // POST api/sender
        [HttpPost]
        public void Post([FromQuery]string tradesFileIn, [FromQuery]int? numberOfTradesIn, [FromQuery]string marketIn)
        {
            string tradesFile = tradesFileIn ?? Constants.TradesFile;
            string market = marketIn ?? "SFB";
            int numberOfTrades = numberOfTradesIn ?? 1000;

            var rand = new Random((int)DateTime.UtcNow.Ticks);
            var listTrades = new List<Trade>();
            var tradeNumbers = new Dictionary<string, int>();
            int maxInstrumentIndex = Math.Max(Constants.Instruments.Length - 1, 0);

            for (int i = 0; i < numberOfTrades; i++)
            {
                var instrument = Constants.Instruments[(int)((rand.NextDouble() * (double)maxInstrumentIndex) + 0.5)];
                if (tradeNumbers.TryGetValue(instrument, out int tradeNumber))
                    tradeNumber++;
                else
                    tradeNumber = 1;
                listTrades.Add(new Trade
                {
                    Instrument = instrument,
                    SequenceNumber = i,
                    TradeNumber = tradeNumber,
                    TradeDate = DateTime.UtcNow,
                    Volume = rand.NextDouble() * 10000.0,
                    Price = rand.NextDouble() * 200.0,
                    Buyer = "DAG",
                    Seller = "PETER"
                });
                tradeNumbers[instrument] = tradeNumber;
            }

            System.IO.File.WriteAllText(tradesFile, Newtonsoft.Json.JsonConvert.SerializeObject(listTrades));
        }
    }
}