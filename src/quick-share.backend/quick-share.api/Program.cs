using quick_share.api.Data;
using quick_share.api.Endpoints;
using quick_share.api.Business.Services;
using quick_share.api.Business.Contracts;
using Serilog;
using FluentValidation;
using quick_share.api.Business.Validations;
using quick_share.api.Business.Commands;
using StackExchange.Redis;
using quick_share.api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
    
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "quick-share API";
    config.Title = "quick-share API v1";
    config.Version = "v1";
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConnection = builder.Configuration.GetConnectionString("Redis")
        ?? throw new InvalidOperationException("Missing Redis connection string");

    return ConnectionMultiplexer.Connect(redisConnection);
});

builder.Services.Configure<StorageOptions>(
    builder.Configuration.GetSection(StorageOptions.Storage));


builder.Services.AddScoped<RedisDataContext>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IValidator<GetSessionCommand>, GetSessionCommandValidator>();
builder.Services.AddScoped<IValidator<EndSessionCommand>, EndSessionCommandValidator>();
builder.Services.AddScoped<IValidator<AddSimpleItemCommand>, AddSimpleItemCommandValidator>();
builder.Services.AddScoped<IValidator<AddBinaryItemCommand>, AddBinaryItemCommandValidator>();
builder.Services.AddScoped<IValidator<DeleteItemCommand>, DeleteItemCommandValidator>();
builder.Services.AddScoped<IValidator<GetBinaryItemCommand>, GetBinaryItemCommandValidator>();

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

app.UseSerilogRequestLogging();

app.MapSessionEndpoints();

app.Run();
