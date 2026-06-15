using Dapper;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication;
using NSwag;
using ScopelyBattles.Api.Authentication;
using ScopelyBattles.Api.Middleware;
using ScopelyBattles.Shared.DataAccess;

DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.UseShared(builder.Configuration);
builder.Services.AddFastEndpoints();
builder
    .Services.AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName,
        options => { }
    );
builder.Services.AddAuthorization();
builder.Services.SwaggerDocument(o =>
{
    o.EnableJWTBearerAuth = false;
    o.DocumentSettings = s =>
    {
        s.Title = "Scopely Battles API";
        s.Version = "v1";
        s.AddAuth(
            ApiKeyAuthenticationHandler.SchemeName,
            new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = ApiKeyAuthenticationHandler.HeaderName,
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "API key required for protected endpoints.",
            }
        );
    };
});

var app = builder.Build();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen();

await app.RunAsync();
