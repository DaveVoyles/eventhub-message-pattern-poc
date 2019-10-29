using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OneWeek_Eventing.Common;
using OneWeek_Eventing.StreamingWithResend.Entities;

namespace OneWeek_Eventing.StreamingWithResend.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdatesController : ControllerBase
    {
        // GET api/sender
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var files = System.IO.Directory.EnumerateFiles(".", "*.updates");
            if (files != null)
            {
                return new ActionResult<IEnumerable<string>>(files);
            }
            else
            {
                return new string[] { "No update files found" };
            }
        }

        // GET api/trades/defaults.updates
        [HttpGet("{updatesFile}")]
        public ActionResult<string> Get(string updatesFile)
        {
            try
            {
                return System.IO.File.ReadAllText(updatesFile);
            }
            catch (Exception)
            {
                return "Didn't find this file";
            }
        }

        // POST api/sender
        [HttpPost]
        public void Post([FromQuery]string updatesFileIn, [FromQuery]int? numberOfUpdatesIn)
        {
            string updatesFile = updatesFileIn ?? Constants.UpdatesFile;
            int numberOfUpdates = numberOfUpdatesIn ?? 1000;
            var listUpdates = new List<Update>();

            var rand = new Random((int)DateTime.UtcNow.Ticks);
            Dictionary<string, Update> updateMessages = new Dictionary<string, Update>();

            for (int i = 0; i < numberOfUpdates; i++)
            {
                var instrument = Constants.Instruments[rand.Next(Constants.Instruments.Length - 1)];
                if (!updateMessages.TryGetValue(instrument, out Update update))
                {
                    var priceOpen = rand.NextDouble() * 200.0;
                    update = new Update()
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

                update.SequenceNumber = i;
                update.PriceLatest += (rand.NextDouble() > 0.5 ? 1 : -1) * (rand.NextDouble() * 0.5);
                if (update.PriceLatest < update.PriceLow)
                    update.PriceLow = update.PriceLatest;
                if (update.PriceLatest > update.PriceHigh)
                    update.PriceHigh = update.PriceLatest;
                update.VolumeLatest = rand.NextDouble() * 1000;
                update.VolumeTradedToday += update.VolumeLatest;
                update.PriceDate = DateTime.UtcNow;
                listUpdates.Add(update);
            }

            System.IO.File.WriteAllText(updatesFile, Newtonsoft.Json.JsonConvert.SerializeObject(listUpdates));
        }
    }
}