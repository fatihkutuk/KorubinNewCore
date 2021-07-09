using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KorubinNewCore.Managers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KorubinNewCore
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            OpcManager opcManager = new OpcManager();
            opcManager.Start();
        }
    }
}
