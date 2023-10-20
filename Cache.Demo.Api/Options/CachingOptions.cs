namespace Cache.Demo.Api.Options
{
    public class CachingOptions
    {
        public const string SectionName = "Caching";

        public int CacheDurationInSeconds { get; set; } = 60;
    }
}
