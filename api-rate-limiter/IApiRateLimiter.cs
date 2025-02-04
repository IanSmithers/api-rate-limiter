namespace api_rate_limiter;

public interface IApiRateLimiter
{
    bool HandleRateLimitedRequest(string identifier);
}
