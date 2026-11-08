using L1CacheSystem;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Serilog;
using System;
using System.IO.Compression;
using System.Linq;
using UsageShowcaseService.HostedServices;
using UsageShowcaseService.Services;
using UsageShowcaseService.Services.Interfaces;
using UsageShowcaseService.Settings;

namespace UsageShowcaseService.Extensions;

public static class ServiceExtension
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddConfigures(configuration);
        services.AddServices();
        services.AddSerilog();
        services.AddHostedServices(configuration);
        services.AddHttpClient(configuration);

        services.AddControllers();
        services.AddOpenApi();
    }

    private static void AddConfigures(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));
        services.Configure<MetricSettings>(configuration.GetSection(nameof(MetricSettings)));
    }

    private static void AddHostedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<CacheSystemHostedService>();
        services.AddHostedService<EchoSuppressorHostedService>();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<ServiceCacheSystem>();
        services.AddSingleton<IL1CacheSystemBase>((provider) => provider.GetRequiredService<ServiceCacheSystem>());
        services.AddSingleton<IServiceCacheSystem>((provider) => provider.GetRequiredService<ServiceCacheSystem>());
    }

    private static void AddHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        services.UseHttpClientMetrics();
        services.AddRequestDecompression();
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/x-protobuf"]);
        });

        services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
        services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
    }
}