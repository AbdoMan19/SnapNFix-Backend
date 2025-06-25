using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Interfaces;

namespace SnapNFix.Infrastructure.Services.BackgroundTasksService;

public class BackgroundTaskExecutor : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundTaskExecutor> _logger;

    public BackgroundTaskExecutor(IBackgroundTaskQueue taskQueue, IServiceProvider serviceProvider, ILogger<BackgroundTaskExecutor> logger)
    {
        _taskQueue = taskQueue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Task Executor starting");
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            await workItem(scope.ServiceProvider, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error occurred executing background task.");
                        }
                    }, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected during shutdown, no need to throw
                    _logger.LogInformation("Background task dequeue operation was canceled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred dequeueing background task");
                    
                    // Add a small delay before retrying to prevent tight loops in error cases
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in Background Task Executor");
            throw;
        }
        finally
        {
            _logger.LogInformation("Background Task Executor stopping");
        }
    }  
}