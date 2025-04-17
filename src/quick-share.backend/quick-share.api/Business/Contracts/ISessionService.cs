using FluentResults;
using quick_share.api.Business.Commands;
using quick_share.api.Business.Models;

namespace quick_share.api.Business.Contracts;

public interface ISessionService
{
    Task<Result<string>> StartSession();
    Task<Result<Session>> GetSession(GetSessionCommand command);
    Task<Result> EndSession(EndSessionCommand command);
    Task<Result<string>> AddSimpleItem(AddSimpleItemCommand command);
    Task<Result<string>> AddBinaryItem(AddBinaryItemCommand command);
    Task<Result> DeleteItem(DeleteItemCommand command);
    Result<SharedItemBinaryResult> GetBinaryItem(GetBinaryItemCommand command);
}