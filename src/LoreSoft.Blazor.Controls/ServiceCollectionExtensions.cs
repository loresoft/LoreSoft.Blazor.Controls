using LoreSoft.Blazor.Controls.Events;

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
        services.TryAddSingleton<EventBus>();
        services.TryAddScoped<DownloadService>();
        services.TryAddScoped<StorageService>();
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

    /// <summary>
    /// Adds modal dialog services to the specified <see cref="IServiceCollection"/>.
    /// Registers <see cref="ModalService"/> as singleton.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    /// <remarks>
    /// This method registers the modal service which is used to display and manage modal dialogs.
    /// The <see cref="ModalService"/> requires a <see cref="EventBus"/> service to be registered,
    /// which is automatically handled by <see cref="AddBlazorControls"/>.
    /// </remarks>
    public static IServiceCollection AddModals(this IServiceCollection services)
    {
        services.TryAddSingleton<ModalService>();

        return services;
    }
}
