using LoreSoft.Blazor.Controls;

using Sample.Core;
using Sample.Core.Services;
using Sample.ServerSide.Components;

namespace Sample.ServerSide;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddHttpClient<GitHubClient>()
            .AddHttpMessageHandler<ProgressBarHandler>();

        builder.Services
            .AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddBlazorControls();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseAntiforgery();
        app.MapStaticAssets();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(typeof(Routes).Assembly);

        app.Run();
    }
}
