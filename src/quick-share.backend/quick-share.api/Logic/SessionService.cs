using quick_share.api.Models;
using quick_share.api.Logic.Contracts;
using quick_share.api.Logic.Utils;
using quick_share.api.Data;
using System.Text.Json;

namespace quick_share.api.Logic;

public class SessionService(RedisDataContext redis) : ISessionService
{
    public async Task<string> Start()
    {
        var session = new Session { Id = Generator.NewId() };
        await redis.SaveValueAsync(session.Id,session.ToString());
        return session.Id;
    }

    public async Task<Session?> GetSession(string sessionId)
    {
        var value = await redis.GetValueAsync(sessionId);

        if (string.IsNullOrWhiteSpace(value))
        {
            //throw new KeyNotFoundException();
            return null;
        }

        return JsonSerializer.Deserialize<Session>(value);
    }

    public Task<bool> End(string sessionId)
    {
        throw new NotImplementedException();
    }

    public Task<string> AddSimpleItem(string sessionId)
    {
        throw new NotImplementedException();
    }

    public Task<string> AddBinaryItem(string sessionId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteItem(string sessionId, string itemId)
    {
        throw new NotImplementedException();
    }
}