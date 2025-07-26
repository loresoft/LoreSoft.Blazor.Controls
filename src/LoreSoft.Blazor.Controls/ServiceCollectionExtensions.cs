using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LoreSoft.Blazor.Controls;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorControls(this IServiceCollection services)
    {
        services.TryAddScoped<DownloadService>();

        services.AddProgressBar();
        services.AddToaster();

        return services;
    }

    public static IServiceCollection AddProgressBar(this IServiceCollection services)
    {
        services.TryAddSingleton<ProgressBarState>();
        services.TryAddTransient<ProgressBarHandler>();

        return services;
    }

    public static IServiceCollection AddToaster(this IServiceCollection services)
    {
        services.TryAddSingleton<IToaster, Toaster>();

        return services;
    }
}
