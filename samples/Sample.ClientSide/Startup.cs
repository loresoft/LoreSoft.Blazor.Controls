using System;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Octokit.Internal;

namespace Sample.ClientSide
{
    public static class Startup
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
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

            return services;
        }
    }
}
