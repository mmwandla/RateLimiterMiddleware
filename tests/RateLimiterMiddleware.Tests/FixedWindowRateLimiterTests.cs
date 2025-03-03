using Moq;
using Microsoft.AspNetCore.Http;
using RateLimiterMiddleware.Strategies;

public class FixedWindowRateLimiterTests
{
    private readonly FixedWindowRateLimiter _rateLimiter;
    private readonly Mock<HttpContext> _httpContextMock;

    public FixedWindowRateLimiterTests()
    {
        _rateLimiter = new FixedWindowRateLimiter(2, TimeSpan.FromSeconds(10)); // Limit: 2 requests per 10 seconds
        _httpContextMock = new Mock<HttpContext>();
        _httpContextMock.SetupGet(c => c.Connection.RemoteIpAddress).Returns(System.Net.IPAddress.Loopback);
    }

    [Fact]
    public void IsRequestAllowed_ShouldAllowFirstRequest()
    {
        // Arrange
        var context = _httpContextMock.Object;

        // Act
        var result = _rateLimiter.IsRequestAllowed(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsRequestAllowed_ShouldAllowSecondRequest()
    {
        // Arrange
        var context = _httpContextMock.Object;
        _rateLimiter.IsRequestAllowed(context); // First request

        // Act
        var result = _rateLimiter.IsRequestAllowed(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsRequestAllowed_ShouldDenyThirdRequest()
    {
        // Arrange
        var context = _httpContextMock.Object;
        _rateLimiter.IsRequestAllowed(context); // First request
        _rateLimiter.IsRequestAllowed(context); // Second request

        // Act
        var result = _rateLimiter.IsRequestAllowed(context);

        // Assert
        Assert.False(result);
    }
}
