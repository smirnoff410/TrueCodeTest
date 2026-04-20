using Common.Command;
using CurrencyService.Application.Request;
using CurrencyService.Application.Services;
using CurrencyService.Web.Settings;
using Mapster;
using Microsoft.Extensions.Options;

namespace CurrencyService.Web.BackgroundServices
{
    public class CurrencyBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CurrencyBackgroundService> _logger;
        private readonly CurrencyBackgroundSettings _settings;

        public CurrencyBackgroundService(IServiceProvider serviceProvider, ILogger<CurrencyBackgroundService> logger, IOptions<CurrencyBackgroundSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var currencyGrabber = scope.ServiceProvider.GetRequiredService<ICurrencyGrabber>();
                    var currencies = await currencyGrabber.Grab();

                    var applyCurrencyCommand = scope.ServiceProvider.GetRequiredService<ICommandHandler<List<ApplyCurrencyRequest>, bool>>();
                    var commandResult = await applyCurrencyCommand.Execute(currencies.Adapt<List<ApplyCurrencyRequest>>());

                    _logger.LogInformation("Currencies updated");
                    await Task.Delay(TimeSpan.FromSeconds(_settings.IntervalSec), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Currencies update error. Message: {message}", ex.Message);
                    await Task.Delay(TimeSpan.FromSeconds(_settings.IntervalSec), stoppingToken);
                }
            }
        }
    }
}
