using quick_share.api.Models;

namespace quick_share.api.Logic.Contracts;

public interface ISessionService
{
    Task<string> Start();
    Task<Session> GetSession(string sessionId);
    Task<bool> End(string sessionId);
    Task<string> AddSimpleItem(string sessionId);
    Task<string> AddBinaryItem(string sessionId);
    Task<bool> DeleteItem(string sessionId, string itemId);
}