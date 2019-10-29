using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OneWeek_Eventing.Common;
using OneWeek_Eventing.Filtering.Entities;
using OneWeek_Eventing.Filtering.Interfaces;
using OneWeek_Eventing.Filtering.Web.Workers;

namespace OneWeek_Eventing.Filtering.Web.Controllers
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
        public void Post([FromQuery]string ordersFile, [FromQuery]int? delayBetweenOrders)
        {
            Delete();

            // retrieve all the parameters
            if (string.IsNullOrEmpty(ordersFile))
                ordersFile = Constants.TradesFile; // revert back to the default one
            var orders = JsonConvert.DeserializeObject<IEnumerable<Order>>(System.IO.File.ReadAllText(ordersFile));

            if (orders != null)
            {
                _lastSenderWorker = new SenderWorker(_senderProvider);
                _ = _lastSenderWorker.RunAsync(orders, delayBetweenOrders ?? 0, 0);
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
