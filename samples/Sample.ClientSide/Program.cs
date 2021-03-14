using System;
using System.Net.Http;
using System.Threading.Tasks;
using LoreSoft.Blazor.Controls;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Sample.Core;
using Sample.Core.Services;

namespace Sample.ClientSide
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            var services = builder.Services;

            builder.RootComponents.Add<App>("app");
            
            services
                .AddHttpClient("default", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<ProgressBarHandler>();

            services
                .AddHttpClient<GitHubClient>()
                .AddHttpMessageHandler<ProgressBarHandler>();

            services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("default"));

            services.AddProgressBar();

            await builder.Build().RunAsync();
        }

        public static void ConfigureServices(IServiceCollection services)
        {

        }
    }
}
