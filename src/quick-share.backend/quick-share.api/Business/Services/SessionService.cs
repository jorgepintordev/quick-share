using quick_share.api.Business.Models;
using quick_share.api.Business.Contracts;
using quick_share.api.Business.Utils;
using quick_share.api.Business.Extensions;
using quick_share.api.Data;
using System.Text.Json;
using FluentResults;
using quick_share.api.Business.Consts;
using quick_share.api.Business.Commands;
using FluentValidation;
using quick_share.api.Configuration;
using Microsoft.Extensions.Options;

namespace quick_share.api.Business.Services;

public class SessionService(
        RedisDataContext redis, 
        ILogger<SessionService> log,
        IValidator<GetSessionCommand> getSessionCommandValidator,
        IValidator<EndSessionCommand> endSessionCommandValidator,
        IValidator<AddSimpleItemCommand> addSimpleItemCommandValidator,
        IValidator<AddBinaryItemCommand> addBinaryItemCommandValidator,
        IValidator<DeleteItemCommand> deleteItemCommandValidator,
        IValidator<GetBinaryItemCommand> getBinaryItemCommandValidator,
        IOptions<StorageOptions> storageOptions
    ) : ISessionService
{
    public async Task<Result<string>> StartSession()
    {
        var session = new Session { Id = Generator.NewId() };
        var result = await redis.SaveValueAsync(session.Id,session.ToString());
        if (!result)
        {
            log.LogError(SessionServiceMessages.Error.SessionStartFail);
            return Result.Fail(SessionServiceMessages.Error.SessionStartFail);
        }

        log.LogTrace(SessionServiceMessages.Trace.StartSessionOk, session.Id);
        return Result.Ok(session.Id);
    }

    public async Task<Result<Session>> GetSession(GetSessionCommand command)
    {
        var validationResult = getSessionCommandValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            log.LogError(SessionServiceMessages.Error.ValidationFail + " {@Errors}", validationResult.Errors);
            return Result.Fail<Session>(SessionServiceMessages.Error.ValidationFail);
        }

        var value = await redis.GetValueAsync(command.SessionId);

        if (string.IsNullOrWhiteSpace(value))
        {
            log.LogError(SessionServiceMessages.Error.SessionNotFound + " {SessionId}", command.SessionId);
            return Result.Fail<Session>(SessionServiceMessages.Error.SessionNotFound);
        }

        var sessionValue = JsonSerializer.Deserialize<Session>(value);
        if (sessionValue is null)
        {
            log.LogError(SessionServiceMessages.Error.DeserializeEmpty + " {SessionId} {RedisValue}", command.SessionId, value);
            return Result.Fail<Session>(SessionServiceMessages.Error.DeserializeEmpty);
        }

        log.LogTrace(SessionServiceMessages.Trace.GetSessionOk, sessionValue);
        return Result.Ok(sessionValue);
    }

    public async Task<Result> EndSession(EndSessionCommand command)
    {
        var validationResult = endSessionCommandValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            log.LogError(SessionServiceMessages.Error.ValidationFail + " {@Errors}", validationResult.Errors);
            return Result.Fail(SessionServiceMessages.Error.ValidationFail);
        }

        var result = await redis.DeleteValueAsync(command.SessionId);

        if (!result)
        {
            log.LogError(SessionServiceMessages.Error.SessionNotFound + " {SessionId}", command.SessionId);
            return Result.Fail(SessionServiceMessages.Error.SessionNotFound);
        }

        log.LogTrace(SessionServiceMessages.Trace.EndSessionOk, command.SessionId);
        return Result.Ok();
    }

    public async Task<Result<string>> AddSimpleItem(AddSimpleItemCommand command)
    {
        var validationResult = addSimpleItemCommandValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            log.LogError(SessionServiceMessages.Error.ValidationFail + " {@Errors}", validationResult.Errors);
            return Result.Fail<string>(SessionServiceMessages.Error.ValidationFail);
        }
        
        var newItem = new SharedItem { Id = Guid.NewGuid(), Value = command.ItemValue };
        (command.Session.Items ??= []).Add(newItem);

        var result = await redis.SaveValueAsync(command.Session.Id, command.Session.ToString());

        if (!result)
        {
            log.LogError(SessionServiceMessages.Error.SimpleItemAddFail + " {SessionId} {Item}", command.Session.Id, command.ItemValue);
            return Result.Fail<string>(SessionServiceMessages.Error.SimpleItemAddFail);
        }

        log.LogTrace(SessionServiceMessages.Trace.AddSimpleItemOk, command.Session.Id, newItem.Id);
        return Result.Ok(newItem.Id.ToString());
    }

    public async Task<Result<string>> AddBinaryItem(AddBinaryItemCommand command)
    {
        var validationResult = addBinaryItemCommandValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            log.LogError(SessionServiceMessages.Error.ValidationFail + " {@Errors}", validationResult.Errors);
            return Result.Fail<string>(SessionServiceMessages.Error.ValidationFail);
        }

        string basePath = string.IsNullOrWhiteSpace(storageOptions.Value.UploadFileStorage) 
            ? Directory.GetCurrentDirectory()
            : storageOptions.Value.UploadFileStorage;
        string uploadPath = $"{basePath}/uploads/{command.Session.Id}";
        string fileExtension = Path.GetExtension(command.FormFile.FileName);
        var newItem = new SharedItemBinary { Id = Guid.NewGuid(), Value = command.FormFile.FileName, FileExtension = fileExtension };
        
        string filePath = $"{uploadPath}/{newItem.Id}{fileExtension}";

        //upload file to path
        try
        {
            //create directory path
            Directory.CreateDirectory(uploadPath);
            log.LogTrace(SessionServiceMessages.Trace.CreatedUploadPath, uploadPath);

            using var fileStream = File.Create(filePath);
            command.FormFile.CopyTo(fileStream);
            fileStream.Close();
            log.LogTrace(SessionServiceMessages.Trace.BinaryItemUploaded, filePath);
        } catch(Exception ex)
        {
            log.LogError(ex, SessionServiceMessages.Error.BinaryItemServerCopyFail + " {SessionId} {@Exception}", command.Session.Id, ex);
            return Result.Fail<string>(SessionServiceMessages.Error.BinaryItemServerCopyFail);
        }
        
        // save data into redis
        (command.Session.Items ??= []).Add(newItem);
        var result = await redis.SaveValueAsync(command.Session.Id, command.Session.ToString());

        if (!result)
        {
            //delete file?
            log.LogError(SessionServiceMessages.Error.BinaryItemAddFail + " {SessionId}", command.Session.Id);
            return Result.Fail<string>(SessionServiceMessages.Error.BinaryItemAddFail);
        }

        log.LogTrace(SessionServiceMessages.Trace.AddBinaryItemOk, command.Session.Id, newItem.Id);
        return Result.Ok(newItem.Id.ToString());
    }

    public async Task<Result> DeleteItem(DeleteItemCommand command)
    {
        var validationResult = deleteItemCommandValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            log.LogError(SessionServiceMessages.Error.ValidationFail + " {@Errors}", validationResult.Errors);
            return Result.Fail(SessionServiceMessages.Error.ValidationFail);
        }

        var item = command.Session.Items?.Find(item => item.ToSharedItem()?.Id == command.ItemId);

        if (item is null)
        {
            log.LogError(SessionServiceMessages.Error.SessionItemNotFound + " {SessionId} {ItemId}", command.Session.Id, command.ItemId);
            return Result.Fail(SessionServiceMessages.Error.SessionItemNotFound);
        }
        
        command.Session.Items?.Remove(item);

        var itemBinary = item?.ToSharedItemBinary();

        if (!string.IsNullOrWhiteSpace(itemBinary?.FileExtension))
        {
            //delete file
            string basePath = string.IsNullOrWhiteSpace(storageOptions.Value.UploadFileStorage) 
                ? Directory.GetCurrentDirectory()
                : storageOptions.Value.UploadFileStorage;
            string uploadPath = $"{basePath}/uploads/{command.Session.Id}";
            string fileExtension = Path.GetExtension(itemBinary.Value);
            string filePath = $"{uploadPath}/{itemBinary.Id}{fileExtension}";

            try
            {
                File.Delete(filePath);
                log.LogTrace(SessionServiceMessages.Trace.BinaryItemDeleted, filePath);
            }
            catch(Exception ex)
            {
                log.LogError(ex, SessionServiceMessages.Error.BinaryItemServerDeleteFail + " {SessionId} {ItemId} {@Exception}", command.Session.Id, command.ItemId, ex);
                return Result.Fail(SessionServiceMessages.Error.BinaryItemServerDeleteFail);
            }
        }

        var result = await redis.SaveValueAsync(command.Session.Id, command.Session.ToString());

        if (!result)
        {
            log.LogError(SessionServiceMessages.Error.SessionItemDeleteFail + " {SessionId} {ItemId}", command.Session.Id, command.ItemId);
            return Result.Fail(SessionServiceMessages.Error.SessionItemDeleteFail);
        }

        log.LogTrace(SessionServiceMessages.Trace.DeleteItemOk, command.Session.Id, command.ItemId);
        return Result.Ok();
    }

    public Result<SharedItemBinaryResult> GetBinaryItem(GetBinaryItemCommand command)
    {
        var validationResult = getBinaryItemCommandValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            log.LogError(SessionServiceMessages.Error.ValidationFail + " {@Errors}", validationResult.Errors);
            return Result.Fail<SharedItemBinaryResult>(SessionServiceMessages.Error.ValidationFail);
        }

        var item = command.Session.Items?.Find(item => item.ToSharedItem()?.Id == command.ItemId);

        if (item is null)
        {
            log.LogError(SessionServiceMessages.Error.SessionItemNotFound + " {SessionId} {ItemId}", command.Session.Id, command.ItemId);
            return Result.Fail(SessionServiceMessages.Error.SessionItemNotFound);
        }
        
        var itemBinary = item?.ToSharedItemBinary();

        if (string.IsNullOrWhiteSpace(itemBinary?.FileExtension))
        {
            log.LogError(SessionServiceMessages.Error.SessionBinaryItemNotFound + " {SessionId} {@Item}", command.Session.Id, itemBinary);
            return Result.Fail(SessionServiceMessages.Error.SessionBinaryItemNotFound);
        }
        
        //return file
        string basePath = string.IsNullOrWhiteSpace(storageOptions.Value.UploadFileStorage) 
            ? Directory.GetCurrentDirectory()
            : storageOptions.Value.UploadFileStorage;
        string uploadPath = $"{basePath}/uploads/{command.Session.Id}";
        string fileExtension = Path.GetExtension(itemBinary.Value);
        string filePath = $"{uploadPath}/{itemBinary.Id}{fileExtension}";

        try
        {
            var result = new SharedItemBinaryResult() {
                Filename = itemBinary.Value,
                Data = File.OpenRead(filePath)
            };

            log.LogTrace(SessionServiceMessages.Trace.GetBinaryItemOk, command.Session.Id, command.ItemId);
            return Result.Ok(result);
        }
        catch(Exception ex)
        {
            log.LogError(ex, SessionServiceMessages.Error.BinaryItemGetFail + " {SessionId} {ItemId} {@Exception}", command.Session.Id, command.ItemId, ex);
            return Result.Fail(SessionServiceMessages.Error.BinaryItemGetFail);
        }
    }
}