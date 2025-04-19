using FluentResults;

namespace quick_share.api.Business.Contracts;

public interface IMaintenanceService
{
    Task<Result> FileCleanup();
}