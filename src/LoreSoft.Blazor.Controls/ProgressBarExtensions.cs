using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LoreSoft.Blazor.Controls;

public static class ProgressBarExtensions
{
    public static IServiceCollection AddProgressBar(this IServiceCollection services)
    {
        services.TryAddSingleton<ProgressBarState>();
        services.TryAddTransient<ProgressBarHandler>();

        return services;
    }
}
