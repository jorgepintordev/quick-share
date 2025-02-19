using quick_share.api.Models;
using quick_share.api.Logic.Contracts;
using quick_share.api.Logic.Utils;

namespace quick_share.api.Logic;

public class SessionService : ISessionService
{
    public async Task<string> Start()
    {
        var session = new Session { Id = Generator.NewId() };

        //save "session" into cache db
        return session.Id;
    }

    public Task<Session> GetSession(string id)
    {
        throw new NotImplementedException();
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