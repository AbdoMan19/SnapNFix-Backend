using SnapNFix.Application.Common.Interfaces.ServiceLifetime;

namespace SnapNFix.Application.Interfaces;

public interface IBackgroundTaskQueue : ISingleton
{
    void Enqueue(Func<IServiceProvider, CancellationToken, Task> workItem);
    Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}