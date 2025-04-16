using FluentResults;
using quick_share.api.Business.Models;

namespace quick_share.api.Business.Contracts;

public interface ISessionService
{
    Task<Result<string>> Start();
    Task<Result<Session>> GetSession(string sessionId);
    Task<Result> End(string sessionId);
    Task<Result<string>> AddSimpleItem(Session session, string itemValue);
    Task<Result<string>> AddBinaryItem(Session session, IFormFile formFile);
    Task<Result> DeleteItem(Session session, Guid itemId);
    Result<SharedItemBinaryResult> GetBinaryItem(Session session, Guid itemId);
}