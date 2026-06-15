using ScopelyBattles.Shared.Battles.Processing;

namespace ScopelyBattles.Worker;

public sealed class Worker(BattleProcessor processor) : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan ErrorDelay = TimeSpan.FromSeconds(1);

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.WhenAll(Enumerable.Range(0, Environment.ProcessorCount).Select(_ => ProcessLoopAsync(stoppingToken)));

    private async Task ProcessLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processedBattle = await processor.ProcessNextAsync(stoppingToken);

                if (!processedBattle)
                {
                    await Task.Delay(IdleDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception)
            {
                await Task.Delay(ErrorDelay, stoppingToken);
            }
        }
    }
}
