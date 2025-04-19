using Microsoft.AspNetCore.Mvc;
using quick_share.api.Business.Contracts;
using Serilog;

namespace quick_share.api.Endpoints;

public static class MaintenanceEndpoints
{
    public static void MapMaintenanceEndpoints(this IEndpointRouteBuilder app)
    {
        var groupItems = app.MapGroup("/maintenance");
        groupItems.MapPost("/files/cleanup", PostFileCleanup);
    }

    static async Task<IResult> PostFileCleanup([FromServices]IMaintenanceService service)
    {
        try
        {
            var result = await service.FileCleanup();

            if (result.IsFailed)
            {
                return TypedResults.StatusCode(500);
            }
            return TypedResults.NoContent();
        }
        catch(Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception {@ex}", ex);
            return TypedResults.StatusCode(500);
        }
    }
}