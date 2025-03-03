using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RateLimiterMiddleware.Configurations;
using RateLimiterMiddleware.Interfaces;
using RateLimiterMiddleware.Strategies;

namespace RateLimiterMiddleware
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure services
            builder.Services.AddSingleton<IRateLimiter, FixedWindowRateLimiter>();  // Default to FixedWindow strategy
            builder.Services.AddSingleton<IRateLimiter>(serviceProvider =>
            {
                var options = new RateLimiterOptions
                {
                    Strategy = "FixedWindow",  // Change to "SlidingWindow" or "TokenBucket" to switch strategies
                    RequestLimit = 100,
                    WindowDuration = TimeSpan.FromMinutes(1),
                    TokensPerSecond = 5,
                    BucketSize = 50
                };

                return options.Strategy switch
                {
                    "FixedWindow" => new FixedWindowRateLimiter(options.RequestLimit, options.WindowDuration),
                    "SlidingWindow" => new SlidingWindowRateLimiter(options.RequestLimit, options.WindowDuration),
                    "TokenBucket" => new TokenBucketRateLimiter(options.BucketSize, options.TokensPerSecond),
                    _ => throw new InvalidOperationException("Invalid rate limiter strategy.")
                };
            });

            var app = builder.Build();

            // Rate Limiting Middleware
            app.Use(async (context, next) =>
            {
                var rateLimiter = app.Services.GetRequiredService<IRateLimiter>();

                if (!rateLimiter.IsRequestAllowed(context))
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                    return;
                }

                await next();
            });

            // Example endpoint
            app.MapGet("/", () => "Welcome to Rate Limiter Middleware!");

            app.Run();
        }
    }
}

