using Microsoft.AspNetCore.Http;
using RateLimiterMiddleware.Interfaces;
using System.Collections.Concurrent;

namespace RateLimiterMiddleware.Strategies
{
    public class FixedWindowRateLimiter : IRateLimiter
    {
        private readonly int _requestLimit;
        private readonly TimeSpan _windowDuration;
        private readonly ConcurrentDictionary<string, (DateTime windowStart, int requestCount)> _requests;

        public FixedWindowRateLimiter(int requestLimit, TimeSpan windowDuration)
        {
            _requestLimit = requestLimit;
            _windowDuration = windowDuration;
            _requests = new ConcurrentDictionary<string, (DateTime, int)>();
        }

        public bool IsRequestAllowed(HttpContext context)
        {
            var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var currentTime = DateTime.UtcNow;

            var entry = _requests.GetOrAdd(key, (currentTime, 0));

            if (currentTime - entry.windowStart >= _windowDuration)
            {
                // Reset the window
                _requests[key] = (currentTime, 1);
                return true;
            }

            if (entry.requestCount < _requestLimit)
            {
                // Increment request count
                _requests[key] = (entry.windowStart, entry.requestCount + 1);
                return true;
            }

            return false;
        }
    }
}
