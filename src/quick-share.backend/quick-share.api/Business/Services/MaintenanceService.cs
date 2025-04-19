using FluentResults;
using Microsoft.Extensions.Options;
using quick_share.api.Business.Consts;
using quick_share.api.Business.Contracts;
using quick_share.api.Configuration;
using quick_share.api.Data;

namespace quick_share.api.Business.Services;

public class MaintenanceService(
        RedisDataContext redis, 
        ILogger<SessionService> log,
        IOptions<StorageOptions> storageOptions
    ) : IMaintenanceService
{
    public async Task<Result> FileCleanup()
    {
        string basePath = string.IsNullOrWhiteSpace(storageOptions.Value.UploadFileStorage) 
            ? Directory.GetCurrentDirectory()
            : storageOptions.Value.UploadFileStorage;
        string uploadPath = $"{basePath}/uploads";

        log.LogTrace(MaintenanceServiceMessages.Trace.ProcessingStart, uploadPath);

        try
        {
            foreach(var directory in Directory.GetDirectories(uploadPath))
            {
                var sessionId = new DirectoryInfo(directory).Name;
                var value = await redis.GetValueAsync(sessionId!);
                
                if (string.IsNullOrWhiteSpace(value))
                {
                    Directory.Delete(directory, true);
                    log.LogTrace(MaintenanceServiceMessages.Trace.DeletedPath, directory);
                }
            }
        }
        catch(Exception ex)
        {
            log.LogError(ex, MaintenanceServiceMessages.Error.FileCleanup + " {@Exception}", ex);
            return Result.Fail(MaintenanceServiceMessages.Error.FileCleanup);
        }

        log.LogTrace(MaintenanceServiceMessages.Trace.FileCleanupOk);
        return Result.Ok();
    }
}