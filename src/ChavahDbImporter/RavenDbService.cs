using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChavahDbImporter
{
    public class RavenDbService : BackgroundService
    {
        private bool _isCompleted = false;
        private readonly ILogger<BackgroundService> _logger;
        private int _totalCount;

        public RavenDbService(ILogger<BackgroundService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested || _isCompleted)
            {
                _totalCount += 1;

                if (_totalCount == 10){
                    _isCompleted = true;
                }
                _logger.LogInformation("Running...");
            }
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}
