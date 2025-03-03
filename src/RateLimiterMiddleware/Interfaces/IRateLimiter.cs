using Microsoft.AspNetCore.Http;

namespace RateLimiterMiddleware.Interfaces
{
    public interface IRateLimiter
    {
        bool IsRequestAllowed(HttpContext context);
    }
}
