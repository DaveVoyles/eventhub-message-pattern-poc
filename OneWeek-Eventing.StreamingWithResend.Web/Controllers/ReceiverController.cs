using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OneWeek_Eventing.StreamingWithResend.Interfaces;
using OneWeek_Eventing.StreamingWithResend.Web.Workers;

namespace OneWeek_Eventing.StreamingWithResend.Web.Controllers
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
        public void Post()
        {
            Delete();

            _lastReceiverWorker = new ReceiverWorker(_receiverProvider);
            _ = _lastReceiverWorker.Start();
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