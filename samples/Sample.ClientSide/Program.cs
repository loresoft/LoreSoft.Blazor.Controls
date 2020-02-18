using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Hosting;
using Sample.Core;

namespace Sample.ClientSide
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.Services.ConfigureServices();
            builder.RootComponents.Add<App>("app");

            await builder.Build().RunAsync();
        }
    }
}
