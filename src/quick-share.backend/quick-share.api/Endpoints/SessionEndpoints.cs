using Microsoft.AspNetCore.Mvc;
using quick_share.api.Logic.Contracts;

namespace quick_share.api.Endpoints;

public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var groupItems = app.MapGroup("/session");

        groupItems.MapGet("/start", GetStart);
        groupItems.MapGet("/{sessionId}", GetSession);
    }

    static async Task<IResult> GetStart([FromServices]ISessionService service)
    {
        return TypedResults.Ok(await service.Start());
    }

    static async Task<IResult> GetSession(string sessionId, [FromServices]ISessionService service)
    {
        return TypedResults.Ok(await service.GetSession(sessionId));
    }
}