namespace UsageShowcaseService.Settings
{
    public sealed class MetricSettings
    {
        public string? RestIP { get; init; }

        public int RestPort { get; init; }

        public string? Patch { get; init; } = "/metrics";
    }
}