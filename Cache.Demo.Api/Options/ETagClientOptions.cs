namespace Cache.Demo.Api.Options
{
    public class ETagClientOptions
    {
        public const string SectionName = "ETagClient";

        public Uri? BaseAddress { get; set; }

        public string UrlPath { get; set; } = "";
    }
}
