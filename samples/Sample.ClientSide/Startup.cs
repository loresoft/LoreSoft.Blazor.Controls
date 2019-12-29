using System;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Octokit.Internal;
using Sample.Core;

namespace Sample.ClientSide
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IGitHubClient>(s =>
            {
                var wasmHttpMessageHandlerType = Assembly.Load("WebAssembly.Net.Http").GetType("WebAssembly.Net.Http.HttpClient.WasmHttpMessageHandler");
                var wasmHttpMessageHandler = (HttpMessageHandler)Activator.CreateInstance(wasmHttpMessageHandlerType);

                var connection = new Connection(
                    new ProductHeaderValue("BlazorControls"),
                    new HttpClientAdapter(() => wasmHttpMessageHandler)
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
