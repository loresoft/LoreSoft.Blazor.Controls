using LoreSoft.Blazor.Controls;

using Sample.Core.Services;

namespace Sample.ServerSide;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.SetMinimumLevel(LogLevel.Trace);

        builder.Services
            .AddHttpClient<GitHubClient>()
            .AddHttpMessageHandler<ProgressBarHandler>();

        builder.Services
            .AddHttpClient<RandomDataClient>()
            .AddHttpMessageHandler<ProgressBarHandler>();

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor(config => { config.DetailedErrors = true; });

        builder.Services.AddBlazorControls();

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
}
