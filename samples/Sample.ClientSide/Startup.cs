using Microsoft.AspNetCore.Blazor.Http;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Octokit.Internal;

namespace Sample.ClientSide
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<WebAssemblyHttpMessageHandler>();
            services.AddScoped<IGitHubClient>(s =>
            {
                var connection = new Connection(
                    new ProductHeaderValue("BlazorControls"),
                    new HttpClientAdapter(s.GetRequiredService<WebAssemblyHttpMessageHandler>)
                );

                var client = new GitHubClient(connection);
                return client;
            });

        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
