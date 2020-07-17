// ************************************************************************************************
// 
//  © 2019       General Electric Company
// 
//  Description  See class summary below.
// 
//  History      See source code control system.
// 
// ************************************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using AutoMapper;

using Common.Errors;
using Common.Model.Document;
using Common.Model.Repositories;

using CsvHelper;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PostMonitor.Config;
using PostMonitor.Model;

using Timer = System.Timers.Timer;

namespace PostMonitor.HostedServices
{
    public class CsvExport : IHostedService, IDisposable
    {
        private readonly ILogger<CsvExport> _logger;
        private readonly IMonitoredPostRepository _repo;
        private CsvExportConfiguration _config;
        private Timer _timer;
        private IMapper _mapper;

        public CsvExport(
            ILogger<CsvExport> logger
            , IMonitoredPostRepository repo
            , IOptions<CsvExportConfiguration> config
            , IMapper mapper
        )
        {
            _logger = logger;
            _repo = repo;
            _config = config.Value;
            _mapper = mapper;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_config.IsEnabled)
            {
                return Task.CompletedTask;
            }

            if (!Directory.Exists(_config.DestinationPath))
            {
                Directory.CreateDirectory(_config.DestinationPath);
            }
            _logger.LogInformation("Starting CSV export service");
            _timer = new Timer
            {
                Interval = TimeSpan.FromHours(_config.ExportPeriodicityInHour).TotalMilliseconds,
                AutoReset = true,
                Enabled = true
            };
            var progress = new Progress<Exception>();
            _timer.Elapsed += (sender, args) => HandleExport(progress);
            progress.ProgressChanged += (sender, exception) => throw exception;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping CSV export service.");

            _timer.Enabled = false;

            return Task.CompletedTask;
        }

        private void HandleExport(IProgress<Exception> progress)
        {
            _logger.LogInformation("Starting data export");
            var toExport = _repo
                           .FindAllPostAndVersions()
                           .Select(p => _mapper.Map<CsvRow>(p));
            string timestamp = DateTime.Now.ToString("u").Replace(" ", "_").Replace(":", "-");
            string exportFileName = $"export_{timestamp}.csv";
            
            string csvPath = Path.GetFullPath(Path.Combine(_config.DestinationPath, exportFileName));

            try
            {
                using (var writer = new StreamWriter(csvPath))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(toExport);
                    }
                }

                _logger.LogInformation("Export done to {FilePath}", csvPath);
            }
            catch (Exception exception)
            {
                progress.Report(exception);
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
