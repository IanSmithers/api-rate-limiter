using System.Net;

namespace api_rate_limiter;

public class ApiRateLimiterMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext, IApiRateLimiter apiRateLimiter)
    {
        var endpoint = httpContext.GetEndpoint();
        var attribute = endpoint?.Metadata.GetMetadata<ApiRateLimited>();

        if (attribute != null)
        {
            if (httpContext.Connection.RemoteIpAddress is not null)
            {
                var result = apiRateLimiter.HandleRateLimitedRequest(httpContext.Connection.RemoteIpAddress.ToString());

                if (!result)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    await httpContext.Response.WriteAsync("API Rate-Limited Request");
                    
                    return;
                }
            }
        }

        await next(httpContext);;
    }
}
