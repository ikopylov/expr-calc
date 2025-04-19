﻿using ExprCalc.Entities;
using ExprCalc.Storage.Api.Exceptions;
using ExprCalc.Storage.Api.Repositories;
using ExprCalc.Storage.Instrumentation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Repositories
{
    internal class CalculationRepositoryInMemory : ICalculationRepository
    {
        private readonly ILogger<CalculationRepositoryInMemory> _logger;
        private readonly ActivitySource _activitySource;

        private readonly Dictionary<Guid, Calculation> _data;
        private readonly Lock _lock;

        public CalculationRepositoryInMemory(
            ILogger<CalculationRepositoryInMemory> logger,
            InstrumentationContainer instrumentation)
        {
            _logger = logger;
            _activitySource = instrumentation.ActivitySource;
            _data = new Dictionary<Guid, Calculation>();
            _lock = new Lock();
        }

        public Calculation CreateCalculation(Calculation calculation)
        {
            _logger.LogTrace(nameof(CreateCalculation) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInMemory) + "." + nameof(CreateCalculation));

            lock (_lock)
            {
                if (_data.ContainsKey(calculation.Id))
                {
                    _logger.LogDebug("Calculation with the same key is already existed. Key = {key}", calculation.Id);
                    activity?.SetStatus(ActivityStatusCode.Error, "Calculation with the same key is already existed");
                    throw new StorageDuplicateEntityException("Calculation with the same key is already existed");
                }

                _data.Add(calculation.Id, calculation.Clone());
                return calculation;
            }
        }

        public bool UpdateCalculationStatus(CalculationStatusUpdate calculationStatusUpdate)
        {
            _logger.LogTrace(nameof(UpdateCalculationStatus) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInMemory) + "." + nameof(UpdateCalculationStatus));

            lock (_lock)
            {
                if (_data.TryGetValue(calculationStatusUpdate.Id, out var calculation))
                {
                    if (!calculation.TryChangeStatus(calculationStatusUpdate.Status, calculationStatusUpdate.UpdatedAt, out _))
                        throw new InvalidOperationException("Unexpected status change received by InMemoryStorage");
                    return true;
                }
                return false;
            }
        }

        public List<Calculation> GetCalculationsList()
        {
            _logger.LogTrace(nameof(GetCalculationsList) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInMemory) + "." + nameof(GetCalculationsList));

            lock (_lock)
            {
                return _data.Values.Select(o => o.Clone()).ToList();
            }
        }



        public Task<Calculation> CreateCalculationAsync(Calculation calculation, CancellationToken token)
        {
            try
            {
                return Task.FromResult(CreateCalculation(calculation));
            }
            catch (Exception ex)
            {
                return Task.FromException<Calculation>(ex);
            }
        }

        public Task<List<Calculation>> GetCalculationsListAsync(CancellationToken token)
        {
            try
            {
                return Task.FromResult(GetCalculationsList());
            }
            catch (Exception ex)
            {
                return Task.FromException<List<Calculation>>(ex);
            }
        }

        public Task<bool> UpdateCalculationStatusAsync(CalculationStatusUpdate calculationStatusUpdate, CancellationToken token)
        {
            try
            {
                return Task.FromResult(UpdateCalculationStatus(calculationStatusUpdate));
            }
            catch (Exception ex)
            {
                return Task.FromException<bool>(ex);
            }
        }
    }
}
