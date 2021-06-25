using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Polly;
using Riveet.AspNetCore.HealthCheck;
using Riveet.Prerender.Contexts;
using Riveet.Prerender.Services;

namespace Riveet.Prerender
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<WebsiteContext>(options =>
            {
                options.UseNpgsql(_configuration.GetConnectionString("Sql"), optionBuilder => 
                       {
                           optionBuilder.EnableRetryOnFailure();
                       })
                       .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
            });

            services.AddHttpClient<PrerenderService>()
                    .AddTransientHttpErrorPolicy(p => p.RetryAsync(3));
            services.AddTransient<WebsiteContext>();
            services.AddTransient<WebPageService>();

            services.AddDapper(option =>
            {
                option.ConnectionString = _configuration.GetConnectionString("Sql");
            });
            services.AddHostedService<TimedHostedService>();
            services.AddRazorPages();
            services.AddControllers();
            services.AddHealthChecks()
                    .AddCheck<ReadinessCheck>("db-check", HealthStatus.Unhealthy, new[] { "ready" });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapHealthChecks();
            });
        }
    }
}
