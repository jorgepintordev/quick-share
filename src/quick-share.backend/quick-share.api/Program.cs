using quick_share.api.Data;
using quick_share.api.Endpoints;
using quick_share.api.Business;
using quick_share.api.Business.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "quick-share API";
    config.Title = "quick-share API v1";
    config.Version = "v1";
});

builder.Services.AddScoped<RedisDataContext>();
builder.Services.AddScoped<ISessionService, SessionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "quick-share API";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

app.MapSessionEndpoints();

app.Run();
