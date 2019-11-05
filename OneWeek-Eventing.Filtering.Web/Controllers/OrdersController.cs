using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OneWeek_Eventing.Common;
using OneWeek_Eventing.Filtering.Entities;

namespace OneWeek_Eventing.Filtering.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        // GET api/sender
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var files = System.IO.Directory.EnumerateFiles(".", "*.orders");
            if (files != null)
            {
                return new ActionResult<IEnumerable<string>>(files);
            }
            else
            {
                return new string[] { "No order files found" };
            }
        }

        // GET api/trades/defaults.orders
        [HttpGet("{ordersFile}")]
        public ActionResult<string> Get(string ordersFile)
        {
            try
            {
                return System.IO.File.ReadAllText(ordersFile);
            }
            catch (Exception)
            {
                return "Didn't find this file";
            }
        }

        // POST api/sender
        [HttpPost]
        public void Post([FromQuery]string ordersFileIn, [FromQuery]int? numberOfOrdersIn)
        {
            string ordersFile = ordersFileIn ?? Constants.OrdersFile;
            int numberOfOrders = numberOfOrdersIn ?? 1000;
            var listOrders = new List<Order>();

            var rand = new Random((int)DateTime.UtcNow.Ticks);
            int maxMarketIndex = Math.Max(Constants.Markets.Length - 1, 0);
            int maxInstrumentIndex = Math.Max(Constants.Instruments.Length - 1, 0);

            for (int i = 0; i < numberOfOrders; i++)
            {
                var market = Constants.Markets[(int)((rand.NextDouble() * (double)maxMarketIndex) + 0.5)];
                var instrument = Constants.Instruments[(int)((rand.NextDouble() * (double)maxInstrumentIndex) + 0.5)];
                listOrders.Add(new Order
                {
                    SequenceNumber = i,
                    Market = market,
                    Instrument = instrument,
                    IsBuy = rand.NextDouble() > 0.5 ? true : false,
                    Volume = rand.NextDouble() * 10000.0,
                    Price = rand.NextDouble() * 200.0,
                    OrderDate = DateTime.UtcNow
                }) ;
            }

            System.IO.File.WriteAllText(ordersFile, Newtonsoft.Json.JsonConvert.SerializeObject(listOrders));
        }
    }
}