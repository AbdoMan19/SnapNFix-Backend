using Microsoft.Extensions.DependencyInjection;
using SnapNFix.Application.Interfaces;

namespace SnapNFix.Application.Common.Extensions;

public static class BackgroundTaskQueueExtensions
{
    public static void EnqueueScoped<TService>(this IBackgroundTaskQueue queue, Func<TService, Task> action)
        where TService : notnull
    {
        queue.Enqueue(async (sp, ct) =>
        {
            var service = sp.GetRequiredService<TService>();
            await action(service);
        });
    }
}