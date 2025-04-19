﻿using ExprCalc.CoreLogic.Configuration;
using ExprCalc.CoreLogic.Instrumentation;
using ExprCalc.CoreLogic.Resources.CalculationsRegistry;
using ExprCalc.CoreLogic.Services.RegistryRepopulation;
using ExprCalc.Storage.Api.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Services.StorageCleanup
{
    /// <summary>
    /// Periodically removes old calculations from storage
    /// </summary>
    internal class StorageCleanupService : BackgroundService
    {
        private readonly ICalculationRepository _calculationsRepository;

        private readonly TimeSpan _expirationPeriod;
        private readonly TimeSpan _cleanupPeriod;

        private readonly ActivitySource _activitySource;
        private readonly ILogger<StorageCleanupService> _logger;

        public StorageCleanupService(
            ICalculationRepository calculationRepository,
            IOptions<CoreLogicConfig> config,
            ILogger<StorageCleanupService> logger,
            InstrumentationContainer instrumentation)
        {
            _calculationsRepository = calculationRepository;

            _expirationPeriod = config.Value.StorageCleanupExpiration;
            _cleanupPeriod = config.Value.StorageCleanupPeriod;

            _activitySource = instrumentation.ActivitySource;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_cleanupPeriod <= TimeSpan.Zero)
            {
                _logger.LogInformation("Storage cleanup was disabled");
                return;
            }

            // Delay first run slightly
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await RunCleanup(stoppingToken);
                await Task.Delay(_cleanupPeriod, stoppingToken);
            }
        }

        private async Task RunCleanup(CancellationToken token)
        {
            DateTime deleteBefore = DateTime.UtcNow.Subtract(_expirationPeriod);
            _logger.LogInformation("Cleanup procedure started");
            using var activity = _activitySource.StartActivity(nameof(StorageCleanupService) + ".Cleanup");

            Stopwatch sw = Stopwatch.StartNew();
            int deletedCount = await _calculationsRepository.DeleteCalculationsAsync(deleteBefore, token);
            _logger.LogInformation("Cleanup procedure removed {num} calculations. Procedure took {time}ms", deletedCount, sw.ElapsedMilliseconds);
        }
    }
}
