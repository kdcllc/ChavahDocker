using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Smuggler;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ChavahDbImporter
{
    public class RavenDbService : BackgroundService
    {
        private bool _isNotCompleted = true;
        private int _retries;
        private readonly ILogger<BackgroundService> _logger;
        private readonly DatabaseSettings _settings;
        private readonly string _importFilePath;

        public RavenDbService(
            ILogger<BackgroundService> logger,
            DatabaseSettings settings,
            IHostingEnvironment hosting)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _importFilePath = Path.Combine(hosting.ContentRootPath,_settings.ImportFile);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_isNotCompleted) break;

                try
                {
                    var docStore = new DocumentStore
                    {
                        Urls = new[] { _settings.Url },
                        Database = _settings.Name
                    };

                    docStore.Initialize();

                    var operation = new GetDatabaseNamesOperation(0, 1000); // 1000 is safe enough for me.
                    var databaseNames = docStore.Maintenance.Server.Send(operation);

                    docStore.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(_settings.Name)));

                    var importOperation = await docStore.Smuggler.ImportAsync(new DatabaseSmugglerImportOptions
                    {
                        OperateOnTypes = DatabaseItemType.Documents
                    }, _importFilePath, stoppingToken);

                    await importOperation.WaitForCompletionAsync();

                    _logger.LogInformation("Completed the import...");
                    _isNotCompleted = false;

                    break;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("already exists"))
                    {
                        _isNotCompleted = false;
                    }
                    _logger.LogError(ex.Message);
                }

                _retries += 1;
                _logger.LogDebug("Retrying to import: {retries}", _retries);

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}
