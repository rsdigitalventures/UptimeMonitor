using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UptimeMonitor.Services
{
    public class UptimeService : IHostedService, IDisposable
    {
        private readonly ILogger<UptimeService> _logger;
        private Timer _timer;

        public IServiceProvider Services { get; }
        public IConfiguration Configuration { get; }

        public UptimeService(ILogger<UptimeService> logger,
            IServiceProvider services,
            IConfiguration configuration)
        {
            _logger = logger;
            Services = services;
            Configuration = configuration;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            var interval = Configuration.GetValue<int>("UptimeIntervalMinutes");

            _logger.LogInformation($"Uptime Hosted Service running with interval of {interval} minutes.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(interval));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            try
            {
                using (var scope = Services.CreateScope())
                {
                    var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();

                    var httpClient = new HttpClient();
                    var websites = await dataService.GetEnabledWebsites();

                    foreach (var website in websites)
                    {
                        try
                        {
                            _logger.LogInformation($"Checking uptime of website at {website.Url}");
                            var response = await httpClient.GetAsync(website.Url);
                            var success = response.IsSuccessStatusCode;


                            await dataService.RecordUptime(website.Url, success);
                        }
                        catch (Exception)
                        {
                            await dataService.RecordUptime(website.Url, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error checking uptime");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Uptime Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
