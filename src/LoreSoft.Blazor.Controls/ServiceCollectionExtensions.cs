using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Extension methods for registering LoreSoft Blazor Controls services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all LoreSoft Blazor Controls services to the specified <see cref="IServiceCollection"/>.
    /// Registers <see cref="DownloadService"/>, progress bar, and toaster services.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddBlazorControls(this IServiceCollection services)
    {
        services.TryAddSingleton<Messenger>();
        services.TryAddScoped<DownloadService>();
        services.TryAddScoped<BrowserCultureProvider>();

        services.AddProgressBar();
        services.AddToaster();
        services.AddModals();

        return services;
    }

    /// <summary>
    /// Adds progress bar services to the specified <see cref="IServiceCollection"/>.
    /// Registers <see cref="ProgressBarState"/> as singleton and <see cref="ProgressBarHandler"/> as transient.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddProgressBar(this IServiceCollection services)
    {
        services.TryAddSingleton<ProgressBarState>();
        services.TryAddTransient<ProgressBarHandler>();

        return services;
    }

    /// <summary>
    /// Adds toaster notification services to the specified <see cref="IServiceCollection"/>.
    /// Registers <see cref="IToaster"/> as singleton.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddToaster(this IServiceCollection services)
    {
        services.TryAddSingleton<IToaster, Toaster>();

        return services;
    }

    public static IServiceCollection AddModals(this IServiceCollection services)
    {
        services.TryAddSingleton<ModalService>();

        return services;
    }
}
