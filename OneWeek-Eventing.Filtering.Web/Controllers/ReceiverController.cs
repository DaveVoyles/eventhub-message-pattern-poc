using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;
using OneWeek_Eventing.Filtering.Interfaces;
using OneWeek_Eventing.Filtering.Web.Workers;

namespace OneWeek_Eventing.Filtering.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiverController : ControllerBase
    {
        private readonly IReceiverProvider _receiverProvider;
        private static ReceiverWorker _lastReceiverWorker = null;

        public ReceiverController(IReceiverProvider receiverProvider)
        {
            _receiverProvider = receiverProvider;
        }

        // GET api/sender
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            if (_lastReceiverWorker != null)
            {
                var status = _lastReceiverWorker.GetStatus();
                return new string[] { JsonConvert.SerializeObject(status) };
            }
            else
            {
                return new string[] { "No receiver currently allocated" };
            }
        }

        // POST api/sender
        [HttpPost]
        public void Post([FromQuery]string market, [FromQuery]string instrument)
        {
            Delete();

            _lastReceiverWorker = new ReceiverWorker(_receiverProvider);
            _ = _lastReceiverWorker.Start(market, instrument);
        }

        // DELETE api/receiver
        [HttpDelete()]
        public void Delete()
        {
            if (_lastReceiverWorker != null)
            {
                _ = _lastReceiverWorker.Stop();
                _lastReceiverWorker = null;
            }
        }
    }
}