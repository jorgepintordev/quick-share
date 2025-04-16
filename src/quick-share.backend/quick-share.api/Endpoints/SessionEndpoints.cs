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
        return TypedResults.Created(await service.Start());
    }

    static async Task<IResult> GetSession(string sessionId, [FromServices]ISessionService service)
    {
        return await service.GetSession(sessionId)
            is Session session
                ? TypedResults.Ok(session)
                : TypedResults.NotFound();
    }

    static async Task<IResult> GetEnd(string sessionId, [FromServices]ISessionService service)
    {
        return await service.End(sessionId)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    static async Task<IResult> PostSimpleItem(string sessionId, string value, [FromServices]ISessionService service)
    {
        var session = await service.GetSession(sessionId);
        //validations should be moved to validation class
        if (session is null) { return TypedResults.NotFound(); }

        return TypedResults.Created(await service.AddSimpleItem(session, value));
    }

    static async Task<IResult> PostBinaryItem(string sessionId, IFormFile formFile, [FromServices]ISessionService service)
    {
        var session = await service.GetSession(sessionId);
        //validations should be moved to validation class
        if (session is null) { return TypedResults.NotFound(); }

        return await service.AddBinaryItem(session, formFile)
            is string result
                ? TypedResults.Created(result)
                : TypedResults.StatusCode(500);
    }

    static async Task<IResult> DeleteItem(string sessionId, Guid itemId, [FromServices]ISessionService service)
    {
        var session = await service.GetSession(sessionId);
        //validations should be moved to validation class
        if (session is null) { return TypedResults.NotFound(); }

        return await service.DeleteItem(session, itemId)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    static async Task<IResult> GetBinaryItem(string sessionId, Guid itemId, [FromServices]ISessionService service)
    {
        var session = await service.GetSession(sessionId);
        //validations should be moved to validation class
        if (session is null) { return TypedResults.NotFound(); }

        return await service.GetBinaryItem(session, itemId)
            is SharedItemBinaryResult result
            ? TypedResults.File(result.Data!, MediaTypeNames.Application.Octet, result.Filename)
            : TypedResults.NotFound();
    }
}