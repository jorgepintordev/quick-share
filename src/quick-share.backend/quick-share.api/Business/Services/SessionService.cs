using quick_share.api.Business.Models;
using quick_share.api.Business.Contracts;
using quick_share.api.Business.Utils;
using quick_share.api.Business.Extensions;
using quick_share.api.Data;
using System.Text.Json;
using FluentResults;
using quick_share.api.Business.Consts;

namespace quick_share.api.Business.Services;

public class SessionService(RedisDataContext redis, ILogger<SessionService> log) : ISessionService
{
    public async Task<Result<string>> Start()
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

    public async Task<Result<Session>> GetSession(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            log.LogError(SessionServiceErrors.SessionIdEmpty);
            return Result.Fail<Session>(SessionServiceErrors.SessionIdEmpty);
        }

        var value = await redis.GetValueAsync(sessionId);

        if (string.IsNullOrWhiteSpace(value))
        {
            log.LogError(SessionServiceErrors.SessionNotFound + " {SessionId}", sessionId);
            return Result.Fail<Session>(SessionServiceErrors.SessionNotFound);
        }

        var sessionValue = JsonSerializer.Deserialize<Session>(value);
        if (sessionValue is null)
        {
            log.LogError(SessionServiceErrors.DeserializeEmpty + " {SessionId} {RedisValue}", sessionId, value);
            return Result.Fail<Session>(SessionServiceErrors.DeserializeEmpty);
        }

        log.LogTrace("Get session Ok {Session}", sessionValue);
        return Result.Ok(sessionValue);
    }

    public async Task<Result> End(string sessionId)
    {
        var result = await redis.DeleteValueAsync(sessionId);

        if (!result)
        {
            log.LogError(SessionServiceErrors.SessionNotFound + " {SessionId}", sessionId);
            return Result.Fail(SessionServiceErrors.SessionNotFound);
        }

        log.LogTrace("End session Ok {SessionId}", sessionId);
        return Result.Ok();
    }

    public async Task<Result<string>> AddSimpleItem(Session session, string itemValue)
    {
        // ArgumentNullException.ThrowIfNullOrWhiteSpace(itemValue); //para las validaciones ver mejor una clase de validaciones
        
        var newItem = new SharedItem { Id = Guid.NewGuid(), Value = itemValue };
        (session.Items ??= []).Add(newItem);

        var result = await redis.SaveValueAsync(session.Id,session.ToString());

        if (!result)
        {
            log.LogError(SessionServiceErrors.SimpleItemAddFail + " {SessionId} {Item}", session.Id, itemValue);
            return Result.Fail<string>(SessionServiceErrors.SimpleItemAddFail);
        }

        log.LogTrace("Add simple item into session Ok {SessionId} {ItemId}", session.Id, newItem.Id);
        return Result.Ok(newItem.Id.ToString());
    }

    public async Task<Result<string>> AddBinaryItem(Session session, IFormFile formFile)
    {
        string basePath = Directory.GetCurrentDirectory();
        string uploadPath = $"{basePath}/uploads/{session.Id}";
        string fileExtension = Path.GetExtension(formFile.FileName);
        var newItem = new SharedItemBinary { Id = Guid.NewGuid(), Value = formFile.FileName, FileExtension = fileExtension };
        
        string filePath = $"{uploadPath}/{newItem.Id}{fileExtension}";

        //upload file to path
        try
        {
            //create directory path
            Directory.CreateDirectory(uploadPath);
            log.LogTrace("Create server upload path completed {UploadPath}", uploadPath);

            using var fileStream = File.Create(filePath);
            formFile.CopyTo(fileStream);
            fileStream.Close();
            log.LogTrace("Binary uploaded to server completed {FilePath}", filePath);
        } catch(Exception ex)
        {
            log.LogError(ex, SessionServiceErrors.BinaryItemServerCopyFail + " {SessionId} {@Exception}", session.Id, ex);
            return Result.Fail<string>(SessionServiceErrors.BinaryItemServerCopyFail);
        }
        
        // save data into redis
        (session.Items ??= []).Add(newItem);
        var result = await redis.SaveValueAsync(session.Id,session.ToString());

        if (!result)
        {
            //delete file?
            log.LogError(SessionServiceErrors.BinaryItemAddFail + " {SessionId}", session.Id);
            return Result.Fail<string>(SessionServiceErrors.BinaryItemAddFail);
        }

        log.LogTrace("Add binary item into session Ok {SessionId} {ItemId}", session.Id, newItem.Id);
        return Result.Ok(newItem.Id.ToString());
    }

    public async Task<Result> DeleteItem(Session session, Guid itemId)
    {
        var item = session.Items?.Find(item => item.ToSharedItem()?.Id == itemId);

        if (item is null)
        {
            log.LogError(SessionServiceErrors.SessionItemNotFound + " {SessionId} {ItemId}", session.Id, itemId);
            return Result.Fail(SessionServiceErrors.SessionItemNotFound);
        }
        
        session.Items?.Remove(item);

        var itemBinary = item?.ToSharedItemBinary();

        if (!string.IsNullOrWhiteSpace(itemBinary?.FileExtension))
        {
            //delete file
            string basePath = Directory.GetCurrentDirectory();
            string uploadPath = $"{basePath}/uploads/{session.Id}";
            string fileExtension = Path.GetExtension(itemBinary.Value);
            string filePath = $"{uploadPath}/{itemBinary.Id}{fileExtension}";

            try
            {
                File.Delete(filePath);
                log.LogTrace("Binary deleted from server completed {FilePath}", filePath);
            }
            catch(Exception ex)
            {
                log.LogError(ex, SessionServiceErrors.BinaryItemServerDeleteFail + " {SessionId} {ItemId} {@Exception}", session.Id, itemId, ex);
                return Result.Fail(SessionServiceErrors.BinaryItemServerDeleteFail);
            }
        }

        var result = await redis.SaveValueAsync(session.Id,session.ToString());

        if (!result)
        {
            log.LogError(SessionServiceErrors.SessionItemDeleteFail + " {SessionId} {ItemId}", session.Id, itemId);
            return Result.Fail(SessionServiceErrors.SessionItemDeleteFail);
        }

        log.LogTrace("Delete item from session Ok {SessionId} {ItemId}", session.Id, itemId);
        return Result.Ok();
    }

    public Result<SharedItemBinaryResult> GetBinaryItem(Session session, Guid itemId)
    {
        var item = session.Items?.Find(item => item.ToSharedItem()?.Id == itemId);

        if (item is null)
        {
            log.LogError(SessionServiceErrors.SessionItemNotFound + " {SessionId} {ItemId}", session.Id, itemId);
            return Result.Fail(SessionServiceErrors.SessionItemNotFound);
        }
        
        var itemBinary = item?.ToSharedItemBinary();

        if (string.IsNullOrWhiteSpace(itemBinary?.FileExtension))
        {
            log.LogError(SessionServiceErrors.SessionBinaryItemNotFound + " {SessionId} {@Item}", session.Id, itemBinary);
            return Result.Fail(SessionServiceErrors.SessionBinaryItemNotFound);
        }
        
        //return file
        string basePath = Directory.GetCurrentDirectory();
        string uploadPath = $"{basePath}/uploads/{session.Id}";
        string fileExtension = Path.GetExtension(itemBinary.Value);
        string filePath = $"{uploadPath}/{itemBinary.Id}{fileExtension}";

        try
        {
            var result = new SharedItemBinaryResult() {
                Filename = itemBinary.Value,
                Data = File.OpenRead(filePath)
            };

            log.LogTrace("Get binary item from session Ok {SessionId} {ItemId}", session.Id, itemId);
            return Result.Ok(result);
        }
        catch(Exception ex)
        {
            log.LogError(ex, SessionServiceErrors.BinaryItemGetFail + " {SessionId} {ItemId} {@Exception}", session.Id, itemId, ex);
            return Result.Fail(SessionServiceErrors.BinaryItemGetFail);
        }
    }
}