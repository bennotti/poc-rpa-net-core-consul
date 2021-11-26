using Consul;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleProject.Core.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SampleProject.Infra.HostedService
{
    public class ConsulRpaHostedService : BackgroundService
    {
        private CancellationTokenSource _cts;
        private readonly IConsulClient _consulClient;
        private readonly IOptions<ConsulConfig> _consulConfig;
        private readonly ILogger<ConsulRpaHostedService> _logger;
        private string _registrationID;

        public ConsulRpaHostedService(IConsulClient consulClient, IOptions<ConsulConfig> consulConfig, ILogger<ConsulRpaHostedService> logger)
        {
            _logger = logger;
            _consulConfig = consulConfig;
            _consulClient = consulClient;

        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _consulClient.Agent.PassTTL(_registrationID, string.Empty);
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a linked token so we can trigger cancellation outside of this token's cancellation
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _registrationID = $"{_consulConfig.Value.ServiceID}-rpa";
            var registration = new AgentServiceRegistration()
            {
                ID = _registrationID,
                Name = _consulConfig.Value.ServiceName,
                Tags = new[] { "rpa-service" }
            };

            _logger.LogInformation("Registering in Consul");
            await _consulClient.Agent.ServiceDeregister(registration.ID, _cts.Token);
            await _consulClient.Agent.ServiceRegister(registration, _cts.Token);
            await _consulClient.Agent.CheckRegister(new AgentCheckRegistration()
            {
                ID = _registrationID,
                Name = _consulConfig.Value.ServiceName,
                Status = HealthStatus.Passing,
                TTL = TimeSpan.FromSeconds(15),
                ServiceID = _registrationID,
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(60), // 60sec
            });

            await _consulClient.Agent.PassTTL(_registrationID, string.Empty);

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            _logger.LogInformation("Deregistering from Consul");
            try
            {
                await _consulClient.Agent.ServiceDeregister(_registrationID, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Deregisteration failed");
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
