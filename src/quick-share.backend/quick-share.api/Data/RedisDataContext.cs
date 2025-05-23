using StackExchange.Redis;

namespace quick_share.api.Data;

public class RedisDataContext(IConnectionMultiplexer redis)
{
    private readonly IDatabase db = redis.GetDatabase();

    public async Task<bool> SaveValueAsync(string key, string value, TimeSpan? expiry = null)
    {
        expiry ??= TimeSpan.FromDays(1); // Default expiration is 1 day
        return await db.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var value = await db.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<bool> DeleteValueAsync(string key)
    {
        var result = await db.StringGetDeleteAsync(key);
        return result.HasValue;
    }
}