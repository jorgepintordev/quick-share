using Microsoft.AspNetCore.Mvc;
using quick_share.api.Logic.Contracts;

namespace quick_share.api.Endpoints;

public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var groupItems = app.MapGroup("/session");

        groupItems.MapGet("/start", GetStart);
    }

    static async Task<IResult> GetStart([FromServices]ISessionService service)
    {
        return TypedResults.Ok(await service.Start());
    }
}