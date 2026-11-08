namespace UsageShowcaseService.Settings
{
    public sealed class AppSettings
    {
        public string? RestIP { get; init; }

        public int RestPort { get; init; }

        public string? PathBase { get; init; } = "/";
    }
}