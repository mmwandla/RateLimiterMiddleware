using Microsoft.AspNetCore.Http;
using RateLimiterMiddleware.Interfaces;
using System.Collections.Concurrent;

namespace RateLimiterMiddleware.Strategies
{
    public class TokenBucketRateLimiter : IRateLimiter
    {
        private readonly int _bucketSize;
        private readonly int _tokensPerSecond;
        private readonly ConcurrentDictionary<string, (int tokens, DateTime lastRefill)> _buckets;
        private readonly TimeSpan _refillInterval;

        public TokenBucketRateLimiter(int bucketSize, int tokensPerSecond)
        {
            _bucketSize = bucketSize;
            _tokensPerSecond = tokensPerSecond;
            _refillInterval = TimeSpan.FromSeconds(1);
            _buckets = new ConcurrentDictionary<string, (int, DateTime)>();
        }

        public bool IsRequestAllowed(HttpContext context)
        {
            var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var currentTime = DateTime.UtcNow;

            var bucket = _buckets.GetOrAdd(key, (tokens: _bucketSize, lastRefill: currentTime));

            // Refill tokens periodically
            if (currentTime - bucket.lastRefill >= _refillInterval)
            {
                int newTokens = (int)((currentTime - bucket.lastRefill).TotalSeconds) * _tokensPerSecond;
                int newTokenCount = Math.Min(bucket.tokens + newTokens, _bucketSize);

                _buckets[key] = (newTokenCount, currentTime);
                bucket = _buckets[key];
            }

            if (bucket.tokens > 0)
            {
                // Allow request and consume a token
                _buckets[key] = (bucket.tokens - 1, bucket.lastRefill);
                return true;
            }

            return false;
        }
    }
}
