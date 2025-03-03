using Moq;
using Microsoft.AspNetCore.Http;
using RateLimiterMiddleware.Strategies;

public class TokenBucketRateLimiterTests
{
    private readonly TokenBucketRateLimiter _rateLimiter;
    private readonly Mock<HttpContext> _httpContextMock;

    public TokenBucketRateLimiterTests()
    {
        _rateLimiter = new TokenBucketRateLimiter(5, 1); // Bucket size: 5, 1 token per second
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
    public void IsRequestAllowed_ShouldDenyAfterBucketExhausted()
    {
        // Arrange
        var context = _httpContextMock.Object;
        _rateLimiter.IsRequestAllowed(context); // First request
        _rateLimiter.IsRequestAllowed(context); // Second request
        _rateLimiter.IsRequestAllowed(context); // Third request
        _rateLimiter.IsRequestAllowed(context); // Fourth request
        _rateLimiter.IsRequestAllowed(context); // Fifth request

        // Act
        var result = _rateLimiter.IsRequestAllowed(context); // Sixth request, should be denied

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsRequestAllowed_ShouldAllowAfterTokensRefilled()
    {
        // Arrange
        var context = _httpContextMock.Object;
        _rateLimiter.IsRequestAllowed(context); // First request
        _rateLimiter.IsRequestAllowed(context); // Second request
        _rateLimiter.IsRequestAllowed(context); // Third request

        // Simulate waiting for token refill
        System.Threading.Thread.Sleep(1000); // Wait for 1 second for the token bucket to refill

        // Act
        var result = _rateLimiter.IsRequestAllowed(context); // Should be allowed after refill

        // Assert
        Assert.True(result);
    }
}
