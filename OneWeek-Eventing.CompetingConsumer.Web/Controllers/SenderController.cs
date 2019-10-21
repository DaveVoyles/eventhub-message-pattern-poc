using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OneWeek_Eventing.CompetingConsumer.Entities;
using OneWeek_Eventing.CompetingConsumer.Interfaces;
using OneWeek_Eventing.CompetingConsumer.Web.Workers;
using System.IO;
using System.Collections.Generic;
using OneWeek_Eventing.Common;

namespace OneWeek_Eventing.CompetingConsumer.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SenderController : ControllerBase
    {
        private readonly ISenderProvider _senderProvider;
        private static SenderWorker _lastSenderWorker = null;

        public SenderController(ISenderProvider senderProvider)
        {
            _senderProvider = senderProvider;
        }

        // GET api/sender
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            if (_lastSenderWorker != null)
            {
                var status = _lastSenderWorker.GetStatus();
                return new string[] { JsonConvert.SerializeObject(status) };
            }
            else
            {
                return new string[] { "No sender currently allocated" };
            }
        }

        // POST api/sender
        [HttpPost]
        public void Post([FromQuery]string tradesFile, [FromQuery]string instrument, [FromQuery]int? delayBetweenTrades)
        {
            Delete();

            // retrieve all the parameters
            if (string.IsNullOrEmpty(tradesFile))
                tradesFile = Constants.TradesFile; // revert back to the default one
            var trades = JsonConvert.DeserializeObject<IEnumerable<Trade>>(System.IO.File.ReadAllText(tradesFile));

            if (trades != null)
            {
                _lastSenderWorker = new SenderWorker(_senderProvider);
                _ = _lastSenderWorker.RunAsync(trades, instrument, delayBetweenTrades ?? 0, 0);
            }
        }

        // DELETE api/sender
        [HttpDelete()]
        public void Delete()
        {
            if (_lastSenderWorker != null)
            {
                var status = _lastSenderWorker.GetStatus();
                if (status.State == WorkerState.Stopped)
                {
                    _lastSenderWorker = null;
                }
                else
                {
                    /* TODO; fail with worker already in progress */
                }
                _lastSenderWorker = null;
            }
        }
    }
}