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

namespace quick_share.api.Business.Services;

public class SessionService(
    RedisDataContext redis, 
    ILogger<SessionService> log,
    IValidator<GetSessionCommand> getSessionCommandValidator,
    IValidator<EndSessionCommand> endSessionCommandValidator,
    IValidator<AddSimpleItemCommand> addSimpleItemCommandValidator,
    IValidator<AddBinaryItemCommand> addBinaryItemCommandValidator,
    IValidator<DeleteItemCommand> deleteItemCommandValidator,
    IValidator<GetBinaryItemCommand> getBinaryItemCommandValidator
    ) : ISessionService
{
    public async Task<Result<string>> StartSession()
    {
        var session = new Session { Id = Generator.NewId() };
        var result = await redis.SaveValueAsync(session.Id,session.ToString());
        if (!result)
        {
            log.LogError(SessionServiceErrors.SessionStartFail);
            return Result.Fail(SessionServiceErrors.SessionStartFail);
        }

        log.LogTrace("Start session Ok {SessionId}", session.Id);
        return Result.Ok(session.Id);
    }

    public async Task<Result<Session>> GetSession(GetSessionCommand command)
    {
        var validationResult = getSessionCommandValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            log.LogError(SessionServiceErrors.ValidationFail + " {@Errors}", validationResult.Errors);
            return Result.Fail<Session>(SessionServiceErrors.ValidationFail);
        }

        var value = await redis.GetValueAsync(command.SessionId);

        if (string.IsNullOrWhiteSpace(value))
        {
            log.LogError(SessionServiceErrors.SessionNotFound + " {SessionId}", command.SessionId);
            return Result.Fail<Session>(SessionServiceErrors.SessionNotFound);
        }

        var sessionValue = JsonSerializer.Deserialize<Session>(value);
        if (sessionValue is null)
        {
            log.LogError(SessionServiceErrors.DeserializeEmpty + " {SessionId} {RedisValue}", command.SessionId, value);
            return Result.Fail<Session>(SessionServiceErrors.DeserializeEmpty);
        }

        log.LogTrace("Get session Ok {Session}", sessionValue);
        return Result.Ok(sessionValue);
    }

    public async Task<Result> EndSession(EndSessionCommand command)
    {
        var validationResult = endSessionCommandValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            log.LogError(SessionServiceErrors.ValidationFail + " {@Errors}", validationResult.Errors);
            return Result.Fail(SessionServiceErrors.ValidationFail);
        }

        var result = await redis.DeleteValueAsync(command.SessionId);

        if (!result)
        {
            log.LogError(SessionServiceErrors.SessionNotFound + " {SessionId}", command.SessionId);
            return Result.Fail(SessionServiceErrors.SessionNotFound);
        }

        log.LogTrace("End session Ok {SessionId}", command.SessionId);
        return Result.Ok();
    }

    public async Task<Result<string>> AddSimpleItem(AddSimpleItemCommand command)
    {
        var validationResult = addSimpleItemCommandValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            log.LogError(SessionServiceErrors.ValidationFail + " {@Errors}", validationResult.Errors);
            return Result.Fail<string>(SessionServiceErrors.ValidationFail);
        }
        
        var newItem = new SharedItem { Id = Guid.NewGuid(), Value = command.ItemValue };
        (command.Session.Items ??= []).Add(newItem);

        var result = await redis.SaveValueAsync(command.Session.Id, command.Session.ToString());

        if (!result)
        {
            log.LogError(SessionServiceErrors.SimpleItemAddFail + " {SessionId} {Item}", command.Session.Id, command.ItemValue);
            return Result.Fail<string>(SessionServiceErrors.SimpleItemAddFail);
        }

        log.LogTrace("Add simple item into session Ok {SessionId} {ItemId}", command.Session.Id, newItem.Id);
        return Result.Ok(newItem.Id.ToString());
    }

    public async Task<Result<string>> AddBinaryItem(AddBinaryItemCommand command)
    {
        var validationResult = addBinaryItemCommandValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            log.LogError(SessionServiceErrors.ValidationFail + " {@Errors}", validationResult.Errors);
            return Result.Fail<string>(SessionServiceErrors.ValidationFail);
        }

        string basePath = Directory.GetCurrentDirectory();
        string uploadPath = $"{basePath}/uploads/{command.Session.Id}";
        string fileExtension = Path.GetExtension(command.FormFile.FileName);
        var newItem = new SharedItemBinary { Id = Guid.NewGuid(), Value = command.FormFile.FileName, FileExtension = fileExtension };
        
        string filePath = $"{uploadPath}/{newItem.Id}{fileExtension}";

        //upload file to path
        try
        {
            //create directory path
            Directory.CreateDirectory(uploadPath);
            log.LogTrace("Create server upload path completed {UploadPath}", uploadPath);

            using var fileStream = File.Create(filePath);
            command.FormFile.CopyTo(fileStream);
            fileStream.Close();
            log.LogTrace("Binary uploaded to server completed {FilePath}", filePath);
        } catch(Exception ex)
        {
            log.LogError(ex, SessionServiceErrors.BinaryItemServerCopyFail + " {SessionId} {@Exception}", command.Session.Id, ex);
            return Result.Fail<string>(SessionServiceErrors.BinaryItemServerCopyFail);
        }
        
        // save data into redis
        (command.Session.Items ??= []).Add(newItem);
        var result = await redis.SaveValueAsync(command.Session.Id, command.Session.ToString());

        if (!result)
        {
            //delete file?
            log.LogError(SessionServiceErrors.BinaryItemAddFail + " {SessionId}", command.Session.Id);
            return Result.Fail<string>(SessionServiceErrors.BinaryItemAddFail);
        }

        log.LogTrace("Add binary item into session Ok {SessionId} {ItemId}", command.Session.Id, newItem.Id);
        return Result.Ok(newItem.Id.ToString());
    }

    public async Task<Result> DeleteItem(DeleteItemCommand command)
    {
        var validationResult = deleteItemCommandValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            log.LogError(SessionServiceErrors.ValidationFail + " {@Errors}", validationResult.Errors);
            return Result.Fail(SessionServiceErrors.ValidationFail);
        }

        var item = command.Session.Items?.Find(item => item.ToSharedItem()?.Id == command.ItemId);

        if (item is null)
        {
            log.LogError(SessionServiceErrors.SessionItemNotFound + " {SessionId} {ItemId}", command.Session.Id, command.ItemId);
            return Result.Fail(SessionServiceErrors.SessionItemNotFound);
        }
        
        command.Session.Items?.Remove(item);

        var itemBinary = item?.ToSharedItemBinary();

        if (!string.IsNullOrWhiteSpace(itemBinary?.FileExtension))
        {
            //delete file
            string basePath = Directory.GetCurrentDirectory();
            string uploadPath = $"{basePath}/uploads/{command.Session.Id}";
            string fileExtension = Path.GetExtension(itemBinary.Value);
            string filePath = $"{uploadPath}/{itemBinary.Id}{fileExtension}";

            try
            {
                File.Delete(filePath);
                log.LogTrace("Binary deleted from server completed {FilePath}", filePath);
            }
            catch(Exception ex)
            {
                log.LogError(ex, SessionServiceErrors.BinaryItemServerDeleteFail + " {SessionId} {ItemId} {@Exception}", command.Session.Id, command.ItemId, ex);
                return Result.Fail(SessionServiceErrors.BinaryItemServerDeleteFail);
            }
        }

        var result = await redis.SaveValueAsync(command.Session.Id, command.Session.ToString());

        if (!result)
        {
            log.LogError(SessionServiceErrors.SessionItemDeleteFail + " {SessionId} {ItemId}", command.Session.Id, command.ItemId);
            return Result.Fail(SessionServiceErrors.SessionItemDeleteFail);
        }

        log.LogTrace("Delete item from session Ok {SessionId} {ItemId}", command.Session.Id, command.ItemId);
        return Result.Ok();
    }

    public Result<SharedItemBinaryResult> GetBinaryItem(GetBinaryItemCommand command)
    {
        var validationResult = getBinaryItemCommandValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            log.LogError(SessionServiceErrors.ValidationFail + " {@Errors}", validationResult.Errors);
            return Result.Fail<SharedItemBinaryResult>(SessionServiceErrors.ValidationFail);
        }

        var item = command.Session.Items?.Find(item => item.ToSharedItem()?.Id == command.ItemId);

        if (item is null)
        {
            log.LogError(SessionServiceErrors.SessionItemNotFound + " {SessionId} {ItemId}", command.Session.Id, command.ItemId);
            return Result.Fail(SessionServiceErrors.SessionItemNotFound);
        }
        
        var itemBinary = item?.ToSharedItemBinary();

        if (string.IsNullOrWhiteSpace(itemBinary?.FileExtension))
        {
            log.LogError(SessionServiceErrors.SessionBinaryItemNotFound + " {SessionId} {@Item}", command.Session.Id, itemBinary);
            return Result.Fail(SessionServiceErrors.SessionBinaryItemNotFound);
        }
        
        //return file
        string basePath = Directory.GetCurrentDirectory();
        string uploadPath = $"{basePath}/uploads/{command.Session.Id}";
        string fileExtension = Path.GetExtension(itemBinary.Value);
        string filePath = $"{uploadPath}/{itemBinary.Id}{fileExtension}";

        try
        {
            var result = new SharedItemBinaryResult() {
                Filename = itemBinary.Value,
                Data = File.OpenRead(filePath)
            };

            log.LogTrace("Get binary item from session Ok {SessionId} {ItemId}", command.Session.Id, command.ItemId);
            return Result.Ok(result);
        }
        catch(Exception ex)
        {
            log.LogError(ex, SessionServiceErrors.BinaryItemGetFail + " {SessionId} {ItemId} {@Exception}", command.Session.Id, command.ItemId, ex);
            return Result.Fail(SessionServiceErrors.BinaryItemGetFail);
        }
    }
}