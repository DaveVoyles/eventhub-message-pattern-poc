﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OneWeek_Eventing.Common;
using OneWeek_Eventing.StreamingWithResend.Entities;
using OneWeek_Eventing.StreamingWithResend.Interfaces;
using OneWeek_Eventing.StreamingWithResend.Web.Workers;

namespace OneWeek_Eventing.StreamingWithResend.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SenderController : ControllerBase
    {
        private readonly ISenderProvider _senderProvider;
        private static SenderWorker _lastSenderWorker = null;
        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

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
        public void Post([FromQuery]string updatesFile, [FromQuery]int? delayBetweenUpdates)
        {
            Delete();

            // retrieve all the parameters
            if (string.IsNullOrEmpty(updatesFile))
                updatesFile = Constants.UpdatesFile; // revert back to the default one
            var updates = JsonConvert.DeserializeObject<IEnumerable<Update>>(System.IO.File.ReadAllText(updatesFile));

            if (updates != null)
            {
                _lastSenderWorker = new SenderWorker(_senderProvider);
                _ = _lastSenderWorker.RunAsync(updates, delayBetweenUpdates ?? 0, 0);
            }
        }

        // DELETE api/sender
        [HttpDelete()]
        public void Delete()
        {
            if (_lastSenderWorker != null)
            {
                var status = _lastSenderWorker.GetStatus();
                if (status.State != WorkerState.Stopped)
                {
                    _cancellationTokenSource.Cancel();
                }
                _lastSenderWorker = null;
            }
        }
    }
}