using quick_share.api.Business.Models;
using quick_share.api.Business.Contracts;
using quick_share.api.Business.Utils;
using quick_share.api.Business.Extensions;
using quick_share.api.Data;
using System.Text.Json;

namespace quick_share.api.Business;

public class SessionService(RedisDataContext redis) : ISessionService
{
    public async Task<string> Start()
    {
        var session = new Session { Id = Generator.NewId() };
        await redis.SaveValueAsync(session.Id,session.ToString());
        return session.Id;
    }

    public async Task<Session?> GetSession(string sessionId)
    {
        //implememtar FluentResult y que regrese errors de validaciones que hay en los metodos en lugar de exceptions/null
        // if (string.IsNullOrWhiteSpace(sessionId))
        // {
        //     return new { Error = "Session Id is empty" };
        // }

        var value = await redis.GetValueAsync(sessionId);

        if (string.IsNullOrWhiteSpace(value))
        {
            //throw new KeyNotFoundException();
            return null;
        }

        return JsonSerializer.Deserialize<Session>(value);
    }

    public async Task<bool> End(string sessionId)
    {
        return await redis.DeleteValueAsync(sessionId);
    }

    public async Task<string?> AddSimpleItem(Session session, string itemValue)
    {
        // ArgumentNullException.ThrowIfNullOrWhiteSpace(itemValue); //para las validaciones ver mejor una clase de validaciones
        
        var newItem = new SharedItem { Id = Guid.NewGuid(), Value = itemValue };
        (session.Items ??= []).Add(newItem);

        await redis.SaveValueAsync(session.Id,session.ToString());

        return newItem.Id.ToString();
    }

    public async Task<string?> AddBinaryItem(Session session, IFormFile formFile)
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
            return null;
        }
        
        // save data into redis
        (session.Items ??= []).Add(newItem);
        await redis.SaveValueAsync(session.Id,session.ToString());

        return newItem.Id.ToString();
    }

    public async Task<bool> DeleteItem(Session session, Guid itemId)
    {
        var item = session.Items?.Find(item => item.ToSharedItem()?.Id == itemId);

        if (item is null)
            return false;
        
        session.Items?.Remove(item);

        var itemBinary = item?.ToSharedItemBinary();

        if (!string.IsNullOrWhiteSpace(itemBinary?.FileExtension))
        {
            //delete file
            string basePath = Directory.GetCurrentDirectory();
            string uploadPath = $"{basePath}/uploads/{session.Id}";
            string fileExtension = Path.GetExtension(itemBinary.Value);
            string filePath = $"{uploadPath}/{itemBinary.Id}{fileExtension}";

            File.Delete(filePath);
        }

        await redis.SaveValueAsync(session.Id,session.ToString());
        return true;
    }

    public async Task<SharedItemBinaryResult?> GetBinaryItem(Session session, Guid itemId)
    {
        var item = session.Items?.Find(item => item.ToSharedItem()?.Id == itemId);

        if (item is null)
            return null;
        
        var itemBinary = item?.ToSharedItemBinary();

        if (string.IsNullOrWhiteSpace(itemBinary?.FileExtension))
            return null;
        
        //return file
        string basePath = Directory.GetCurrentDirectory();
        string uploadPath = $"{basePath}/uploads/{session.Id}";
        string fileExtension = Path.GetExtension(itemBinary.Value);
        string filePath = $"{uploadPath}/{itemBinary.Id}{fileExtension}";

        var result = new SharedItemBinaryResult() {
            Filename = itemBinary.Value,
            Data = File.OpenRead(filePath)
        };

        return result;
    }
}