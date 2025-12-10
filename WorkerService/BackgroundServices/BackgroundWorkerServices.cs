using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService.BackgroundServices
{
    public class BackgroundWorkerServices(ILogger<BackgroundWorkerServices> logger) : IHostedService
    {
        readonly ILogger<BackgroundWorkerServices> _logger = logger;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
            
                _logger.LogInformation("Service started at: {time}", DateTime.Now);
                await Task.Delay(1000, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) 
        {
            _logger.LogInformation("Service stopped at:");

            return Task.CompletedTask;
        }


    }
}
