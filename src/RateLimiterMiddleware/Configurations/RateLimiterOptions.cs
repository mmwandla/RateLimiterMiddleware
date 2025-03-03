namespace RateLimiterMiddleware.Configurations
{
    public class RateLimiterOptions
    {
        public required string Strategy { get; set; }
        public int RequestLimit { get; set; }
        public int TokensPerSecond { get; set; }
        public int BucketSize { get; set; }
        public TimeSpan WindowDuration { get; set; }
    }
}
