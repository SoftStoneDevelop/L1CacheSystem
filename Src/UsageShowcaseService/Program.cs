
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Net;
using System.Threading.Tasks;
using UsageShowcaseService.Extensions;
using UsageShowcaseService.Settings;

namespace UsageShowcaseService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddJsonFile(
                "appsettings.Development.json",
                optional: true,
                reloadOnChange: true);

            builder.WebHost.UseKestrel((options) =>
            {
                var appSettings = options.ApplicationServices.GetRequiredService<IOptions<AppSettings>>().Value;
                var adressRest = IPAddress.Parse(appSettings.RestIP!);
                var portRest = appSettings.RestPort;
                options.Listen(adressRest, portRest);

                var settingsMetrics = options.ApplicationServices.GetRequiredService<IOptions<MetricSettings>>().Value;
                var addressMetrics = IPAddress.Parse(settingsMetrics.RestIP!);
                var portMetrics = settingsMetrics.RestPort;

                if (object.Equals(adressRest, addressMetrics)
                    && portRest == portMetrics)
                {
                    return;
                }

                options.Listen(addressMetrics, portMetrics);
            });

            builder.Services.ConfigureServices(builder.Configuration);
            builder.Host.UseSerilog((hostingContext, serviceProvider, loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
            });

            var app =
                builder
                .Build()
                .Configure();

            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                Log.Logger.Fatal("UnhandledException: {exception}", e.ExceptionObject);
                Log.CloseAndFlush();
            };

            await app.RunAsync();
        }
    }
}
