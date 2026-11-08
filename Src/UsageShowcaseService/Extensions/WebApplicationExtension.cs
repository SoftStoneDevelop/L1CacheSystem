using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Prometheus;
using Scalar.AspNetCore;
using UsageShowcaseService.Settings;

namespace UsageShowcaseService.Extensions;

public static class WebApplicationExtension
{
    public static WebApplication Configure(this WebApplication app)
    {
        var appSettings = app.Services.GetRequiredService<IOptions<AppSettings>>().Value;
        if (!string.IsNullOrEmpty(appSettings.PathBase))
        {
            app.UsePathBase(appSettings.PathBase);
        }

        app.UseHttpsRedirection();
        app.UseResponseCompression();
        app.UseRequestDecompression();
        app.UseRouting();

        app.MapControllers();

        var metricSettings = app.Services.GetRequiredService<IOptions<MetricSettings>>().Value;
        app.UseHttpMetrics();

        app.UseMetricServer(metricSettings.RestPort, metricSettings.Patch);

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.AddServer(new ScalarServer(appSettings.PathBase!));
                options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }

        app.MapMetrics();

        return app;
    }
}