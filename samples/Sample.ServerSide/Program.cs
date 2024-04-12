using LoreSoft.Blazor.Controls;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Sample.Core;
using Sample.Core.Services;

namespace Sample.ServerSide
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddHttpClient<GitHubClient>()
                .AddHttpMessageHandler<ProgressBarHandler>();

            builder.Services
                .AddHttpClient<RandomDataClient>()
                .AddHttpMessageHandler<ProgressBarHandler>();

            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor(config => { config.DetailedErrors = true; });
            builder.Services.AddProgressBar();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}
