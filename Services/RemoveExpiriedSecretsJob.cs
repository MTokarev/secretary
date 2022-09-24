using System;
using Microsoft.Extensions.Options;
using Secretary.Interfaces;
using Secretary.Options;

namespace Secretary.Services
{
    public class RemoveExpiriedSecretsJob : BackgroundService
    {
        private readonly ILogger<RemoveExpiriedSecretsJob> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly SecretOptions _secretOptions;
        private readonly PeriodicTimer _periodicTimer;

        public RemoveExpiriedSecretsJob(ILogger<RemoveExpiriedSecretsJob> logger,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<SecretOptions> secretOptions)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _secretOptions = secretOptions.Value;
            _periodicTimer = new(TimeSpan.FromMinutes(_secretOptions.FindExpiriedSecretsInMinute));
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Remove expired secrets job is initializing...");
            var scope = _serviceScopeFactory.CreateScope();
            var secretService = scope.ServiceProvider.GetRequiredService<ISecretService>();

            while (await _periodicTimer.WaitForNextTickAsync(stoppingToken) &&
                !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var secretsToRemove = await secretService
                        .GetSecretsAsync(s => DateTime.UtcNow > s.AvailableUntilUtc);

                    _logger.LogInformation($"Found '{secretsToRemove.Count()}' expired secrets.");

                    foreach (var secret in secretsToRemove)
                    {
                        await secretService.RemoveSecretAsync(secret);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception occured in '{nameof(RemoveExpiriedSecretsJob)}'. Message '{ex.Message}'. Exception: '{ex}'");
                }

                _logger.LogInformation($"Scavenging expired secrets cycle is complete. Next run is sceduled in '{_secretOptions.FindExpiriedSecretsInMinute}' minutes.");
            }

        }

    }
}

