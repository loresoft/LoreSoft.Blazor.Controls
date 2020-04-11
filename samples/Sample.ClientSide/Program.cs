using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Sample.Core;

namespace Sample.ClientSide
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.ConfigureServices();
            builder.Services.AddBaseAddressHttpClient();		
			
            await builder.Build().RunAsync();
        }
    }
}
