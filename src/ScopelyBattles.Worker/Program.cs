using Dapper;
using ScopelyBattles.Shared.DataAccess;
using ScopelyBattles.Worker;

DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.UseShared(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.RunAsync();
