using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Riveet.Prerender.Services
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;

        public TimedHostedService(ILogger<TimedHostedService> logger, 
                                  IServiceProvider services, 
                                  IConfiguration configuration)
        {
            _logger = logger;
            _services = services;
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");

            var interval = TimeSpan.Parse(_configuration["Settings:Interval"]);
            _timer = new Timer(DoWork, null, TimeSpan.FromMinutes(0), interval);

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("Timed Background Service is working.");
            using (var scope = _services.CreateScope())
            {
                try
                {
                    var prerenderService = scope.ServiceProvider.GetRequiredService<PrerenderService>();
                    prerenderService.Start().Wait();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to prerender");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
