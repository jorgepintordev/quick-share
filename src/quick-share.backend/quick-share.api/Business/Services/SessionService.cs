using quick_share.api.Business.Models;
using quick_share.api.Business.Contracts;
using quick_share.api.Business.Utils;
using quick_share.api.Business.Extensions;
using quick_share.api.Data;
using System.Text.Json;
using FluentResults;
using quick_share.api.Business.Consts;

namespace quick_share.api.Business.Services;

public class SessionService(RedisDataContext redis) : ISessionService
{
    public async Task<Result<string>> Start()
    {
        var session = new Session { Id = Generator.NewId() };
        var result = await redis.SaveValueAsync(session.Id,session.ToString());
        if (!result)
        {
            return Result.Fail(SessionServiceErrors.SessionStartFail);
        }

        return Result.Ok(session.Id);
    }

    public async Task<Result<Session>> GetSession(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return Result.Fail<Session>(SessionServiceErrors.SessionIdEmpty);
        }

        var value = await redis.GetValueAsync(sessionId);

        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Fail<Session>(SessionServiceErrors.SessionNotFound);
        }

        var sessionValue = JsonSerializer.Deserialize<Session>(value);
        if (sessionValue is null)
        {
            return Result.Fail<Session>(SessionServiceErrors.DeserializeEmpty);
        }

        return Result.Ok(sessionValue);
    }

    public async Task<Result> End(string sessionId)
    {
        var result = await redis.DeleteValueAsync(sessionId);

        return Result.OkIf(result, SessionServiceErrors.SessionNotFound);
    }

    public async Task<Result<string>> AddSimpleItem(Session session, string itemValue)
    {
        // ArgumentNullException.ThrowIfNullOrWhiteSpace(itemValue); //para las validaciones ver mejor una clase de validaciones
        
        var newItem = new SharedItem { Id = Guid.NewGuid(), Value = itemValue };
        (session.Items ??= []).Add(newItem);

        var result = await redis.SaveValueAsync(session.Id,session.ToString());

        if (!result)
        {
            return Result.Fail<string>(SessionServiceErrors.SimpleItemAddFail);
        }

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
            using var fileStream = File.Create(filePath);
            formFile.CopyTo(fileStream);
            fileStream.Close();
        } catch(Exception)
        {
            //log ex
            return Result.Fail<string>(SessionServiceErrors.BinaryItemServerCopyFail);
        }
        
        // save data into redis
        (session.Items ??= []).Add(newItem);
        var result = await redis.SaveValueAsync(session.Id,session.ToString());

        if (!result)
        {
            //delete file?
            return Result.Fail<string>(SessionServiceErrors.BinaryItemAddFail);
        }

        return Result.Ok(newItem.Id.ToString());
    }

    public async Task<Result> DeleteItem(Session session, Guid itemId)
    {
        var item = session.Items?.Find(item => item.ToSharedItem()?.Id == itemId);

        if (item is null)
            return Result.Fail(SessionServiceErrors.SessionItemNotFound);
        
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
            }
            catch(Exception)
            {
                //log ex
                return Result.Fail(SessionServiceErrors.BinaryItemServerDeleteFail);
            }
        }

        var result = await redis.SaveValueAsync(session.Id,session.ToString());

        return Result.OkIf(result, SessionServiceErrors.SessionItemDeleteFail);
    }

    public Result<SharedItemBinaryResult> GetBinaryItem(Session session, Guid itemId)
    {
        var item = session.Items?.Find(item => item.ToSharedItem()?.Id == itemId);

        if (item is null)
            return Result.Fail(SessionServiceErrors.SessionItemNotFound);
        
        var itemBinary = item?.ToSharedItemBinary();

        if (string.IsNullOrWhiteSpace(itemBinary?.FileExtension))
            return Result.Fail(SessionServiceErrors.SessionBinaryItemNotFound);
        
        //return file
        string basePath = Directory.GetCurrentDirectory();
        string uploadPath = $"{basePath}/uploads/{session.Id}";
        string fileExtension = Path.GetExtension(itemBinary.Value);
        string filePath = $"{uploadPath}/{itemBinary.Id}{fileExtension}";

        var result = new SharedItemBinaryResult() {
            Filename = itemBinary.Value,
            Data = File.OpenRead(filePath)
        };

        return Result.Ok(result);
    }
}