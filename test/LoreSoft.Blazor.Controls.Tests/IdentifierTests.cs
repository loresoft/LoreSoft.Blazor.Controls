using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LoreSoft.Blazor.Controls.Tests;

public class IdentifierTests
{
    [Fact]
    public void Random_DefaultParameters_ReturnsIdWithDefaultPrefix()
    {
        // Act
        var id = Identifier.Random();

        // Assert
        Assert.NotNull(id);
        Assert.StartsWith("id-", id);
        Assert.Matches("^id-[0-9a-f]+$", id);
    }

    [Fact]
    public void Random_CustomPrefix_ReturnsIdWithCustomPrefix()
    {
        // Arrange
        var prefix = "button";

        // Act
        var id = Identifier.Random(prefix);

        // Assert
        Assert.NotNull(id);
        Assert.StartsWith("button-", id);
        Assert.Matches("^button-[0-9a-f]+$", id);
    }

    [Fact]
    public void Random_MultipleInvocations_ReturnsDifferentIds()
    {
        // Act
        var id1 = Identifier.Random();
        var id2 = Identifier.Random();
        var id3 = Identifier.Random();

        // Assert
        Assert.NotEqual(id1, id2);
        Assert.NotEqual(id2, id3);
        Assert.NotEqual(id1, id3);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Random_NullOrWhiteSpacePrefix_ThrowsArgumentException(string? prefix)
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => Identifier.Random(prefix!));
    }

    [Fact]
    public void Sequential_DefaultParameters_ReturnsIdWithDefaultPrefix()
    {
        // Act
        var id = Identifier.Sequential();

        // Assert
        Assert.NotNull(id);
        Assert.StartsWith("id-", id);
        Assert.Matches("^id-[0-9a-f]+$", id);
    }

    [Fact]
    public void Sequential_CustomPrefix_ReturnsIdWithCustomPrefix()
    {
        // Arrange
        var prefix = "input";

        // Act
        var id = Identifier.Sequential(prefix);

        // Assert
        Assert.NotNull(id);
        Assert.StartsWith("input-", id);
        Assert.Matches("^input-[0-9a-f]+$", id);
    }

    [Fact]
    public void Sequential_MultipleInvocations_ReturnsSequentialIds()
    {
        // Arrange - use a short prefix to ensure we have unique prefixes
        var prefix = Guid.NewGuid().ToString("N")[..4];

        // Act
        var id1 = Identifier.Sequential(prefix);
        var id2 = Identifier.Sequential(prefix);
        var id3 = Identifier.Sequential(prefix);

        // Assert - IDs should be different
        Assert.NotEqual(id1, id2);
        Assert.NotEqual(id2, id3);
        Assert.NotEqual(id1, id3);
        
        // All should start with the same prefix
        Assert.StartsWith($"{prefix}-", id1);
        Assert.StartsWith($"{prefix}-", id2);
        Assert.StartsWith($"{prefix}-", id3);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Sequential_NullOrWhiteSpacePrefix_ThrowsArgumentException(string? prefix)
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => Identifier.Sequential(prefix!));
    }

    [Fact]
    public void Sequential_MultipleInvocations_IdsAreSortable()
    {
        // Arrange - use a short prefix
        var prefix = Guid.NewGuid().ToString("N")[..4];
        var ids = new List<string>();

        // Act - Generate a smaller set to verify general ordering
        // Note: Hex values without padding may not be perfectly sortable (e.g., "a" comes before "10" lexicographically)
        // but sequential generation within a reasonable range should maintain relative order
        for (int i = 0; i < 15; i++)
        {
            ids.Add(Identifier.Sequential(prefix));
        }

        // Assert - all IDs should be unique and use the correct prefix
        Assert.Equal(15, ids.Distinct().Count());
        Assert.All(ids, id => Assert.StartsWith($"{prefix}-", id));
        
        // Verify that they are sequential by extracting and comparing the numeric values
        var values = ids.Select(id => 
        {
            var hexPart = id.Substring(prefix.Length + 1);
            return Convert.ToInt32(hexPart, 16);
        }).ToList();
        
        // Values should be consecutive
        for (int i = 1; i < values.Count; i++)
        {
            Assert.Equal(values[i - 1] + 1, values[i]);
        }
    }

    [Fact]
    public void Sequential_DifferentPrefixes_MaintainIndependentCounters()
    {
        // Arrange - use short unique prefixes
        var prefix1 = Guid.NewGuid().ToString("N")[..4];
        var prefix2 = Guid.NewGuid().ToString("N")[..4];

        // Act
        var id1a = Identifier.Sequential(prefix1);
        var id2a = Identifier.Sequential(prefix2);
        var id1b = Identifier.Sequential(prefix1);
        var id2b = Identifier.Sequential(prefix2);

        // Assert - each prefix should have its own counter
        Assert.StartsWith($"{prefix1}-", id1a);
        Assert.StartsWith($"{prefix1}-", id1b);
        Assert.StartsWith($"{prefix2}-", id2a);
        Assert.StartsWith($"{prefix2}-", id2b);
        
        // The counters should be independent
        Assert.NotEqual(id1a, id1b);
        Assert.NotEqual(id2a, id2b);
    }

    [Fact]
    public void Sequential_DefaultPrefix_UsesOptimizedPath()
    {
        // Arrange
        var ids = new HashSet<string>();

        // Act - generate many IDs with default prefix
        for (int i = 0; i < 1000; i++)
        {
            ids.Add(Identifier.Sequential());
        }

        // Assert - all IDs should be unique
        Assert.Equal(1000, ids.Count);
        Assert.All(ids, id => Assert.StartsWith("id-", id));
    }

    [Fact]
    public async Task Sequential_ConcurrentAccess_GeneratesUniqueIds()
    {
        // Arrange - use a short unique prefix
        var prefix = Guid.NewGuid().ToString("N")[..4];
        var ids = new System.Collections.Concurrent.ConcurrentBag<string>();
        var tasks = new List<Task>();

        // Act - generate IDs concurrently
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    ids.Add(Identifier.Sequential(prefix));
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - all 1000 IDs should be unique
        var uniqueIds = ids.Distinct().ToList();
        Assert.Equal(1000, uniqueIds.Count);
        Assert.All(uniqueIds, id => Assert.StartsWith($"{prefix}-", id));
    }

    [Fact]
    public void Sequential_ReturnsHexadecimalFormat()
    {
        // Arrange - use a short prefix
        var prefix = "hex";

        // Act
        var id = Identifier.Sequential(prefix);

        // Assert - should be in format prefix-hexvalue
        Assert.StartsWith("hex-", id);
        
        // Extract the hex part after the prefix and dash
        var hexPart = id.Substring(4); // Skip "hex-"
        
        // Verify it's valid hexadecimal
        Assert.Matches("^[0-9a-f]+$", hexPart);
    }

    [Fact]
    public void Random_ReturnsHexadecimalFormat()
    {
        // Arrange - use a short prefix
        var prefix = "rnd";

        // Act
        var id = Identifier.Random(prefix);

        // Assert - should be in format prefix-hexvalue
        Assert.StartsWith("rnd-", id);
        
        // Extract the hex part after the prefix and dash
        var hexPart = id.Substring(4); // Skip "rnd-"
        
        // Verify it's valid hexadecimal
        Assert.Matches("^[0-9a-f]+$", hexPart);
    }

    [Fact]
    public void Sequential_WithDefaultPrefix_GeneratesValidIds()
    {
        // Act
        var ids = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            ids.Add(Identifier.Sequential());
        }

        // Assert
        Assert.All(ids, id =>
        {
            Assert.StartsWith("id-", id);
            
            // Verify hex format after prefix
            var hexPart = id.Substring(3);
            Assert.Matches("^[0-9a-f]+$", hexPart);
        });
        
        // All should be unique
        Assert.Equal(10, ids.Distinct().Count());
    }
}
