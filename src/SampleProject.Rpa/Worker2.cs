using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MeuAtelie.Rpa
{
    public class Worker2 : BackgroundService
    {
        private readonly ILogger<Worker2> _logger;
        public Worker2(ILogger<Worker2> logger)
        {
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested) {

            _logger.LogInformation("Iniciando worker2");
            await Task.Delay(5000, stoppingToken);
            _logger.LogInformation("Finalizando worker2");
            //}
        }
    }
}
