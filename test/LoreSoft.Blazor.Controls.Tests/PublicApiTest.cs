using System.Reflection;

using LoreSoft.Blazor.Controls.Utilities;

using PublicApiGenerator;

namespace LoreSoft.Blazor.Controls.Tests;

public class PublicApiTest
{
    [Fact]
    public Task PublicApiHasNotChanged()
    {
        // Get the assembly for the library we want to document
        Assembly assembly = typeof(StyleBuilder).Assembly;

        // Retrieve the public API for all types in the assembly
        string publicApi = assembly.GeneratePublicApi();

        // Run a snapshot test on the returned string
        return Verify(publicApi).UseDirectory("Snapshots");
    }
}
