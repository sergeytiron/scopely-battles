using Dapper;
using FastEndpoints;
using ScopelyBattles.Shared.DataAccess;

DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.UseShared(builder.Configuration);
builder.Services.AddFastEndpoints();

var app = builder.Build();
app.UseFastEndpoints();

await app.RunAsync();
