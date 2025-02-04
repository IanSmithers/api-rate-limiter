namespace api_rate_limiter;

public class ApiRateLimiter : IApiRateLimiter
{
    private const byte MaxApiRate = 4;
    private const byte MaxApiInterval = 10;

    private readonly Dictionary<string, Stack<DateTime>> _userRequestTimestamps = new();

    public bool HandleRateLimitedRequest(string identifier)
    {
        Console.WriteLine($"Query data is {DateTime.UtcNow}");

        if (RequestIsPermitted(identifier))
        {
            var stack = RetrieveRequests(identifier);

            if (stack != null && stack.Count != 0)
            {
                var latestRequestTime = stack.Peek();
                var currentRequestTime = DateTime.UtcNow;

                if (latestRequestTime.AddSeconds(MaxApiInterval) < currentRequestTime)
                {
                    ResetRequestLimitsForUser(identifier);
                }
            }

            RegisterRequest(identifier, DateTime.UtcNow);
            
            return true;
        }

        return false;
    }

    private bool RequestIsPermitted(string identifier)
    {
        var stack = RetrieveRequests(identifier);

        if (stack == null || stack.Count == 0) return true;

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (stack.Count < MaxApiRate) return true;

        if (stack.Count == MaxApiRate)
        {
            var latestRequestTime = stack.Peek();
            var currentRequestTime = DateTime.UtcNow;

            if (latestRequestTime.AddSeconds(MaxApiInterval) < currentRequestTime)
            {
                ResetRequestLimitsForUser(identifier);

                return true;
            }
        }

        return false;
    }

    private Stack<DateTime>? RetrieveRequests(string identifier)
    {
        return _userRequestTimestamps.GetValueOrDefault(identifier);
    }

    private void RegisterRequest(string identifier, DateTime timestamp)
    {
        if (!_userRequestTimestamps.ContainsKey(identifier)) _userRequestTimestamps.Add(identifier, new Stack<DateTime>());

        _userRequestTimestamps[identifier].Push(timestamp);
    }

    private void ResetRequestLimitsForUser(string identifier)
    {
        _userRequestTimestamps[identifier].Clear();
    }
}
