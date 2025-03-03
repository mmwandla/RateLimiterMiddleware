using Microsoft.AspNetCore.Http;
using RateLimiterMiddleware.Interfaces;
using System.Collections.Concurrent;

namespace RateLimiterMiddleware.Strategies
{
    public class SlidingWindowRateLimiter : IRateLimiter
    {
        private readonly int _requestLimit;
        private readonly TimeSpan _windowDuration;
        private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _requests;

        public SlidingWindowRateLimiter(int requestLimit, TimeSpan windowDuration)
        {
            _requestLimit = requestLimit;
            _windowDuration = windowDuration;
            _requests = new ConcurrentDictionary<string, ConcurrentQueue<DateTime>>();
        }

        public bool IsRequestAllowed(HttpContext context)
        {
            var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var currentTime = DateTime.UtcNow;

            var queue = _requests.GetOrAdd(key, new ConcurrentQueue<DateTime>());

            // Clean up old requests
            while (queue.TryPeek(out var oldestRequest) && currentTime - oldestRequest > _windowDuration)
            {
                queue.TryDequeue(out _);
            }

            if (queue.Count < _requestLimit)
            {
                // Allow the request
                queue.Enqueue(currentTime);
                return true;
            }

            return false;
        }
    }
}
