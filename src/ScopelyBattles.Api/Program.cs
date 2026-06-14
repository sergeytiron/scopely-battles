using Dapper;
using FastEndpoints;
using FastEndpoints.Swagger;
using ScopelyBattles.Shared.DataAccess;

DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.UseShared(builder.Configuration);
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "Scopely Battles API";
        s.Version = "v1";
    };
});

var app = builder.Build();
app.UseFastEndpoints();
app.UseSwaggerGen();

await app.RunAsync();
