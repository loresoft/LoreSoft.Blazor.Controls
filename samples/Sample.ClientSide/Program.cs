using LoreSoft.Blazor.Controls;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Sample.Core;
using Sample.Core.Services;

namespace Sample.ClientSide;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<Routes>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        var services = builder.Services;

        services
            .AddHttpClient("default", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
            .AddHttpMessageHandler<ProgressBarHandler>();

        services
            .AddHttpClient<GitHubClient>()
            .AddHttpMessageHandler<ProgressBarHandler>();

        services.AddScoped(sp => sp
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient("default")
        );

        services.AddBlazorControls();

        await builder
            .Build()
            .RunAsync();
    }
}
