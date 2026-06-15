using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScopelyBattles.Shared.Battles;
using ScopelyBattles.Shared.Battles.Processing;
using ScopelyBattles.Shared.Leaderboard;
using ScopelyBattles.Shared.Players;

namespace ScopelyBattles.Shared.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseShared(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<ConnectionStrings>()
            .Bind(configuration.GetSection("ConnectionStrings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<PostgresConnectionFactory>();
        services.AddSingleton<PlayerStore>();
        services.AddSingleton<LeaderboardStore>();
        services.AddSingleton<BattleStore>();
        services.AddSingleton<IRandomProvider, RandomProvider>();
        services.AddSingleton<BattleProcessor>();

        return services;
    }
}
