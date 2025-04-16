using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using quick_share.api.Business.Contracts;
using quick_share.api.Business.Models;

namespace quick_share.api.Endpoints;

public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        //Added .DisableAntiforgery() to be able to upload files, auth should be added to the api
        var groupItems = app.MapGroup("/session").DisableAntiforgery();

        groupItems.MapGet("/start", GetStart);
        groupItems.MapGet("/{sessionId}", GetSession);
        groupItems.MapGet("/{sessionId}/end", GetEnd);
        groupItems.MapPost("/{sessionId}/simpleItem", PostSimpleItem);
        groupItems.MapPost("/{sessionId}/binaryItem", PostBinaryItem);
        groupItems.MapDelete("/{sessionId}/{itemId}", DeleteItem);
        groupItems.MapGet("/{sessionId}/{itemId}", GetBinaryItem);
    }

    static async Task<IResult> GetStart([FromServices]ISessionService service)
    {
        var result = await service.Start();

        if (result.IsFailed)
        {
            //add fail messages to log
            return TypedResults.StatusCode(500);
        }
        return TypedResults.Created(result.Value);
    }

    static async Task<IResult> GetSession(string sessionId, [FromServices]ISessionService service)
    {
        var result = await service.GetSession(sessionId);

        if (result.IsFailed)
        {
            //add fail messages to log
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(result.Value);
    }

    static async Task<IResult> GetEnd(string sessionId, [FromServices]ISessionService service)
    {
        var result = await service.End(sessionId);

        if (result.IsFailed)
        {
            //add fail messages to log
            return TypedResults.NotFound();
        }
        return TypedResults.NoContent();
    }

    static async Task<IResult> PostSimpleItem(string sessionId, string value, [FromServices]ISessionService service)
    {
        var session = await service.GetSession(sessionId);
        if (session.IsFailed) { return TypedResults.NotFound(); }

        var result = await service.AddSimpleItem(session.Value, value);

        if (result.IsFailed)
        {
            //add fail messages to log
            return TypedResults.StatusCode(500);
        }
        return TypedResults.Created(result.Value);
    }

    static async Task<IResult> PostBinaryItem(string sessionId, IFormFile formFile, [FromServices]ISessionService service)
    {
        var session = await service.GetSession(sessionId);
        if (session.IsFailed) { return TypedResults.NotFound(); }

        var result = await service.AddBinaryItem(session.Value, formFile);
        
        if (result.IsFailed)
        {
            //add fail messages to log
            return TypedResults.StatusCode(500);
        }
        return TypedResults.Created(result.Value);
    }

    static async Task<IResult> DeleteItem(string sessionId, Guid itemId, [FromServices]ISessionService service)
    {
        var session = await service.GetSession(sessionId);
        if (session.IsFailed) { return TypedResults.NotFound(); }

        var result = await service.DeleteItem(session.Value, itemId);

        if (result.IsFailed)
        {
            //add fail messages to log
            return TypedResults.NotFound();
        }
        return TypedResults.NoContent();
    }

    static async Task<IResult> GetBinaryItem(string sessionId, Guid itemId, [FromServices]ISessionService service)
    {
        var session = await service.GetSession(sessionId);
        if (session.IsFailed) { return TypedResults.NotFound(); }

        var result = service.GetBinaryItem(session.Value, itemId);

        if (result.IsFailed)
        {
            //add fail messages to log
            return TypedResults.NotFound();
        }
        return TypedResults.File(result.Value.Data!, MediaTypeNames.Application.Octet, result.Value.Filename);
    }
}