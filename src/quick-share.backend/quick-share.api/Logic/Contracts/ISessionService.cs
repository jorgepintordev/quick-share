using quick_share.api.Models;

namespace quick_share.api.Logic.Contracts;

public interface ISessionService
{
    Task<string> Start();
    Task<Session?> GetSession(string sessionId);
    Task<bool> End(string sessionId);
    Task<string?> AddSimpleItem(Session session, string itemValue);
    Task<string?> AddBinaryItem(Session session, IFormFile formFile);
    Task<bool> DeleteItem(Session session, Guid itemId);
}