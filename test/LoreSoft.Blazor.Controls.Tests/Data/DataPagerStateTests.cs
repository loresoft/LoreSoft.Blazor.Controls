using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoreSoft.Blazor.Controls;
using Xunit;

namespace LoreSoft.Blazor.Controls.Tests.Data;

public class DataPagerStateTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var pagerState = new DataPagerState();

        // Assert
        Assert.Equal(0, pagerState.Page);
        Assert.Equal(0, pagerState.PageSize);
        Assert.Equal(0, pagerState.Total);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Page_WhenSet_ShouldUpdateValue(int expectedPage)
    {
        // Arrange
        var pagerState = new DataPagerState();

        // Act
        pagerState.Page = expectedPage;

        // Assert
        Assert.Equal(expectedPage, pagerState.Page);
    }

    [Fact]
    public void Page_WhenChanged_ShouldRaisePropertyChanged()
    {
        // Arrange
        var pagerState = new DataPagerState();
        var propertyChangedEvents = new List<string>();
        pagerState.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName!);

        // Act
        pagerState.Page = 5;

        // Assert
        Assert.Contains("Page", propertyChangedEvents);
    }

    [Fact]
    public void Page_WhenSetToSameValue_ShouldNotRaisePropertyChanged()
    {
        // Arrange
        var pagerState = new DataPagerState();
        pagerState.Page = 5;
        var propertyChangedEvents = new List<string>();
        pagerState.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName!);

        // Act
        pagerState.Page = 5; // Same value

        // Assert
        Assert.Empty(propertyChangedEvents);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    public void PageSize_WhenSet_ShouldUpdateValue(int expectedPageSize)
    {
        // Arrange
        var pagerState = new DataPagerState();

        // Act
        pagerState.PageSize = expectedPageSize;

        // Assert
        Assert.Equal(expectedPageSize, pagerState.PageSize);
    }

    [Fact]
    public void PageSize_WhenChanged_ShouldResetPageToOne()
    {
        // Arrange
        var pagerState = new DataPagerState();
        pagerState.Page = 5;

        // Act
        pagerState.PageSize = 10;

        // Assert
        Assert.Equal(1, pagerState.Page);
    }

    [Fact]
    public void PageSize_WhenChanged_ShouldRaisePropertyChanged()
    {
        // Arrange
        var pagerState = new DataPagerState();
        var propertyChangedEvents = new List<string>();
        pagerState.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName!);

        // Act
        pagerState.PageSize = 10;

        // Assert
        Assert.Contains("PageSize", propertyChangedEvents);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(250)]
    [InlineData(1000)]
    public void Total_WhenSet_ShouldUpdateValue(int expectedTotal)
    {
        // Arrange
        var pagerState = new DataPagerState();

        // Act
        pagerState.Total = expectedTotal;

        // Assert
        Assert.Equal(expectedTotal, pagerState.Total);
    }

    [Fact]
    public void Total_WhenChanged_ShouldRaisePropertyChanged()
    {
        // Arrange
        var pagerState = new DataPagerState();
        var propertyChangedEvents = new List<string>();
        pagerState.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName!);

        // Act
        pagerState.Total = 100;

        // Assert
        Assert.Contains("Total", propertyChangedEvents);
    }

    [Theory]
    [InlineData(1, 10, 100, 1)]    // First page
    [InlineData(2, 10, 100, 11)]   // Second page
    [InlineData(5, 10, 100, 41)]   // Fifth page
    [InlineData(10, 10, 100, 91)]  // Last page
    public void StartItem_ShouldCalculateCorrectly(int page, int pageSize, int total, int expectedStartItem)
    {
        // Arrange - Set properties in correct order to avoid Reset() issues
        var pagerState = new DataPagerState();
        pagerState.Total = total;
        pagerState.PageSize = pageSize; // This resets page to 1
        pagerState.Page = page; // Then set the desired page

        // Act & Assert
        Assert.Equal(expectedStartItem, pagerState.StartItem);
    }

    [Fact]
    public void StartItem_WhenPageSizeIsZero_ShouldReturnOne()
    {
        // Arrange
        var pagerState = new DataPagerState
        {
            Total = 100,
            PageSize = 0,
            Page = 5
        };

        // Act & Assert
        Assert.Equal(1, pagerState.StartItem);
    }

    [Theory]
    [InlineData(1, 10, 100, 10)]   // First page
    [InlineData(2, 10, 100, 20)]   // Second page
    [InlineData(5, 10, 100, 50)]   // Fifth page
    [InlineData(10, 10, 100, 100)] // Last page
    [InlineData(10, 10, 95, 95)]   // Last page with fewer items
    public void EndItem_ShouldCalculateCorrectly(int page, int pageSize, int total, int expectedEndItem)
    {
        // Arrange - Set properties in correct order to avoid Reset() issues
        var pagerState = new DataPagerState();
        pagerState.Total = total;
        pagerState.PageSize = pageSize; // This resets page to 1
        pagerState.Page = page; // Then set the desired page

        // Act & Assert
        Assert.Equal(expectedEndItem, pagerState.EndItem);
    }

    [Fact]
    public void EndItem_WhenPageSizeIsZero_ShouldReturnTotal()
    {
        // Arrange
        var pagerState = new DataPagerState
        {
            Total = 100,
            PageSize = 0,
            Page = 5
        };

        // Act & Assert
        Assert.Equal(100, pagerState.EndItem);
    }

    [Theory]
    [InlineData(100, 10, 10)]  // 100 items, 10 per page = 10 pages
    [InlineData(95, 10, 10)]   // 95 items, 10 per page = 10 pages (rounded up)
    [InlineData(91, 10, 10)]   // 91 items, 10 per page = 10 pages (rounded up)
    [InlineData(90, 10, 9)]    // 90 items, 10 per page = 9 pages
    [InlineData(1, 10, 1)]     // 1 item, 10 per page = 1 page
    [InlineData(0, 10, 0)]     // 0 items = 0 pages
    public void PageCount_ShouldCalculateCorrectly(int total, int pageSize, int expectedPageCount)
    {
        // Arrange
        var pagerState = new DataPagerState
        {
            Total = total,
            PageSize = pageSize
        };

        // Act & Assert
        Assert.Equal(expectedPageCount, pagerState.PageCount);
    }

    [Fact]
    public void PageCount_WhenTotalIsZero_ShouldReturnZero()
    {
        // Arrange
        var pagerState = new DataPagerState
        {
            Total = 0,
            PageSize = 10
        };

        // Act & Assert
        Assert.Equal(0, pagerState.PageCount);
    }

    [Theory]
    [InlineData(1, false)]  // First page has no previous
    [InlineData(2, true)]   // Second page has previous
    [InlineData(5, true)]   // Fifth page has previous
    public void HasPreviousPage_ShouldCalculateCorrectly(int page, bool expectedHasPrevious)
    {
        // Arrange
        var pagerState = new DataPagerState
        {
            Page = page
        };

        // Act & Assert
        Assert.Equal(expectedHasPrevious, pagerState.HasPreviousPage);
    }

    [Theory]
    [InlineData(1, 10, 100, true)]   // Page 1 of 10 has next
    [InlineData(5, 10, 100, true)]   // Page 5 of 10 has next
    [InlineData(10, 10, 100, false)] // Page 10 of 10 has no next
    public void HasNextPage_ShouldCalculateCorrectly(int page, int pageSize, int total, bool expectedHasNext)
    {
        // Arrange - Set properties in correct order to avoid Reset() issues
        var pagerState = new DataPagerState();
        pagerState.Total = total;
        pagerState.PageSize = pageSize; // This resets page to 1
        pagerState.Page = page; // Then set the desired page

        // Act & Assert
        Assert.Equal(expectedHasNext, pagerState.HasNextPage);
    }

    [Theory]
    [InlineData(1, true)]   // Page 1 is first
    [InlineData(0, true)]   // Page 0 is considered first (edge case)
    [InlineData(2, false)]  // Page 2 is not first
    public void IsFirstPage_ShouldCalculateCorrectly(int page, bool expectedIsFirst)
    {
        // Arrange
        var pagerState = new DataPagerState
        {
            Page = page
        };

        // Act & Assert
        Assert.Equal(expectedIsFirst, pagerState.IsFirstPage);
    }

    [Theory]
    [InlineData(1, 10, 100, false)]  // Page 1 of 10 is not last
    [InlineData(5, 10, 100, false)]  // Page 5 of 10 is not last
    [InlineData(10, 10, 100, true)]  // Page 10 of 10 is last
    [InlineData(15, 10, 100, true)]  // Page 15 of 10 is considered last (edge case)
    public void IsLastPage_ShouldCalculateCorrectly(int page, int pageSize, int total, bool expectedIsLast)
    {
        // Arrange - Set properties in correct order to avoid Reset() issues
        var pagerState = new DataPagerState();
        pagerState.Total = total;
        pagerState.PageSize = pageSize; // This resets page to 1
        pagerState.Page = page; // Then set the desired page

        // Act & Assert
        Assert.Equal(expectedIsLast, pagerState.IsLastPage);
    }

    [Fact]
    public void Attach_ShouldSetPageAndPageSizeWithoutTriggeringEvents()
    {
        // Arrange
        var pagerState = new DataPagerState();
        var propertyChangedEvents = new List<string>();
        pagerState.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName!);

        // Use reflection to call the internal Attach method
        var attachMethod = typeof(DataPagerState).GetMethod("Attach", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        attachMethod?.Invoke(pagerState, new object[] { 3, 25 });

        // Assert
        Assert.Equal(3, pagerState.Page);
        Assert.Equal(25, pagerState.PageSize);
        Assert.Empty(propertyChangedEvents);
    }

    [Fact]
    public void Reset_ShouldSetPageToOneWithoutTriggeringEvents()
    {
        // Arrange
        var pagerState = new DataPagerState();
        pagerState.Page = 5;
        var propertyChangedEvents = new List<string>();
        pagerState.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName!);

        // Use reflection to call the internal Reset method
        var resetMethod = typeof(DataPagerState).GetMethod("Reset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        resetMethod?.Invoke(pagerState, null);

        // Assert
        Assert.Equal(1, pagerState.Page);
        Assert.Empty(propertyChangedEvents);
    }

    [Fact]
    public void PropertyChanged_WhenSubscribed_ShouldReceiveCorrectPropertyName()
    {
        // Arrange
        var pagerState = new DataPagerState();
        string? receivedPropertyName = null;
        object? receivedSender = null;

        pagerState.PropertyChanged += (sender, e) =>
        {
            receivedSender = sender;
            receivedPropertyName = e.PropertyName;
        };

        // Act
        pagerState.Total = 100;

        // Assert
        Assert.Same(pagerState, receivedSender);
        Assert.Equal("Total", receivedPropertyName);
    }

    [Fact]
    public void MultiplePropertyChanges_ShouldRaiseEventsForEachProperty()
    {
        // Arrange
        var pagerState = new DataPagerState();
        var propertyChangedEvents = new List<string>();
        pagerState.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName!);

        // Act
        pagerState.Total = 100;
        pagerState.Page = 2;
        pagerState.PageSize = 10;

        // Assert
        Assert.Contains("Total", propertyChangedEvents);
        Assert.Contains("Page", propertyChangedEvents);
        Assert.Contains("PageSize", propertyChangedEvents);
        Assert.Equal(3, propertyChangedEvents.Count);
    }

    [Fact]
    public void EdgeCases_WithZeroValues_ShouldHandleGracefully()
    {
        // Arrange & Act
        var pagerState = new DataPagerState
        {
            Page = 0,
            PageSize = 0,
            Total = 0
        };

        // Assert - Should not throw exceptions
        var startItem = pagerState.StartItem;
        var endItem = pagerState.EndItem;
        var pageCount = pagerState.PageCount;
        var hasNext = pagerState.HasNextPage;
        var hasPrevious = pagerState.HasPreviousPage;
        var isFirst = pagerState.IsFirstPage;
        var isLast = pagerState.IsLastPage;

        // All properties should return values without throwing
        Assert.Equal(0, startItem);
        Assert.Equal(0, endItem);
        Assert.Equal(0, pageCount);
        Assert.False(hasNext);
        Assert.False(hasPrevious);
        Assert.True(isFirst);
        Assert.True(isLast);
    }

    [Fact]
    public void EdgeCases_WithNegativeValues_ShouldHandleGracefully()
    {
        // Arrange & Act
        var pagerState = new DataPagerState
        {
            Page = -1,
            PageSize = -5,
            Total = -10
        };

        // Assert - Should not throw exceptions
        var exception = Record.Exception(() =>
        {
            var startItem = pagerState.StartItem;
            var endItem = pagerState.EndItem;
            var pageCount = pagerState.PageCount;
            var hasNext = pagerState.HasNextPage;
            var hasPrevious = pagerState.HasPreviousPage;
            var isFirst = pagerState.IsFirstPage;
            var isLast = pagerState.IsLastPage;
        });

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(1, 10, 50)]
    [InlineData(2, 15, 30)]
    public void CalculatedProperties_WorkTogether_Correctly(int page, int pageSize, int total)
    {
        // Arrange - Set properties in correct order to avoid Reset() issues
        var pagerState = new DataPagerState();
        pagerState.Total = total;
        pagerState.PageSize = pageSize; // This resets page to 1
        pagerState.Page = page; // Then set the desired page

        // Act & Assert - Verify the relationships between calculated properties
        var expectedPageCount = total > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0;
        Assert.Equal(expectedPageCount, pagerState.PageCount);

        var expectedEndItem = pageSize == 0 ? total : Math.Min(pageSize * page, total);
        Assert.Equal(expectedEndItem, pagerState.EndItem);

        var expectedStartItem = pageSize == 0 ? 1 : Math.Max(expectedEndItem - (pageSize - 1), 1);
        Assert.Equal(expectedStartItem, pagerState.StartItem);

        Assert.Equal(page > 1, pagerState.HasPreviousPage);
        Assert.Equal(page < expectedPageCount, pagerState.HasNextPage);
        Assert.Equal(page <= 1, pagerState.IsFirstPage);
        Assert.Equal(page >= expectedPageCount, pagerState.IsLastPage);
    }

    [Fact]
    public void PropertyChanged_HasEventHandler()
    {
        // Arrange
        var pagerState = new DataPagerState();

        // Assert - Check that the PropertyChanged event exists
        Assert.NotNull(pagerState.GetType().GetEvent("PropertyChanged"));
    }

    [Fact]
    public void SetProperty_WithSameValue_DoesNotRaiseEvent()
    {
        // Arrange
        var pagerState = new DataPagerState();
        pagerState.Total = 100;
        var eventCount = 0;
        pagerState.PropertyChanged += (sender, e) => eventCount++;

        // Act
        pagerState.Total = 100; // Same value

        // Assert
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void SetProperty_WithDifferentValue_RaisesEvent()
    {
        // Arrange
        var pagerState = new DataPagerState();
        pagerState.Total = 100;
        var eventCount = 0;
        pagerState.PropertyChanged += (sender, e) => eventCount++;

        // Act
        pagerState.Total = 200; // Different value

        // Assert
        Assert.Equal(1, eventCount);
    }

    [Fact]
    public void PageCount_WithZeroPageSize_ShouldHandleGracefully()
    {
        // Arrange
        var pagerState = new DataPagerState
        {
            Total = 100,
            PageSize = 0
        };

        // Act & Assert - When PageSize is 0, division by zero occurs in Math.Ceiling
        // The actual behavior may vary, but it should not crash
        var exception = Record.Exception(() =>
        {
            var pageCount = pagerState.PageCount;
        });

        Assert.Null(exception);
    }

    [Fact]
    public void StartItem_AndEndItem_Relationship_ShouldBeConsistent()
    {
        // Arrange - Set properties in correct order to avoid Reset() issues
        var pagerState = new DataPagerState();
        pagerState.Total = 100;
        pagerState.PageSize = 10; // This resets page to 1
        pagerState.Page = 2; // Then set the desired page

        // Act
        var startItem = pagerState.StartItem;
        var endItem = pagerState.EndItem;

        // Assert
        // Based on the implementation:
        // EndItem = Math.Min(PageSize * Page, Total) = Math.Min(10 * 2, 100) = 20
        // StartItem = Math.Max(EndItem - (PageSize - 1), 1) = Math.Max(20 - 9, 1) = 11
        Assert.Equal(20, endItem);
        Assert.Equal(11, startItem);
        Assert.Equal(10, endItem - startItem + 1); // Should have exactly pageSize items
    }

    [Fact]
    public void PageSize_WhenSetMultipleTimes_ShouldResetPageEachTime()
    {
        // Arrange
        var pagerState = new DataPagerState();
        pagerState.Page = 5;

        // Act & Assert
        pagerState.PageSize = 10;
        Assert.Equal(1, pagerState.Page);

        pagerState.Page = 3;
        pagerState.PageSize = 20;
        Assert.Equal(1, pagerState.Page);
    }

    [Fact]
    public void NavigationProperties_WithSinglePage_ShouldBehaveCorrectly()
    {
        // Arrange
        var pagerState = new DataPagerState();
        pagerState.Total = 30;
        pagerState.PageSize = 50; // This resets page to 1
        pagerState.Page = 1; // Less than page size

        // Act & Assert
        Assert.Equal(1, pagerState.PageCount);
        Assert.False(pagerState.HasPreviousPage);
        Assert.False(pagerState.HasNextPage);
        Assert.True(pagerState.IsFirstPage);
        Assert.True(pagerState.IsLastPage);
    }

    [Fact]
    public void StartItem_WhenPageIsZero_ShouldReturnOne()
    {
        // Arrange
        var pagerState = new DataPagerState();
        pagerState.Total = 100;
        pagerState.PageSize = 10; // This resets page to 1
        pagerState.Page = 0; // Set to zero after PageSize

        // Act & Assert
        // EndItem = Math.Min(10 * 0, 100) = 0
        // StartItem = Math.Max(0 - 9, 1) = Math.Max(-9, 1) = 1
        Assert.Equal(0, pagerState.EndItem);
        Assert.Equal(1, pagerState.StartItem);
    }

    [Fact]
    public void CalculatedProperties_WithEmptyData_ShouldBehaveCorrectly()
    {
        // Arrange
        var pagerState = new DataPagerState();
        pagerState.Total = 0;
        pagerState.PageSize = 10; // This resets page to 1
        pagerState.Page = 1;

        // Act & Assert
        Assert.Equal(0, pagerState.PageCount);
        Assert.Equal(0, pagerState.EndItem);
        Assert.Equal(0, pagerState.StartItem);
        Assert.False(pagerState.HasPreviousPage);
        Assert.False(pagerState.HasNextPage);
        Assert.True(pagerState.IsFirstPage);
        Assert.True(pagerState.IsLastPage);
    }

    [Fact]
    public void ObjectInitializer_ResetsBehavior_ShouldBeDocumented()
    {
        // This test documents the behavior that using object initializer syntax
        // can cause unexpected results because PageSize setter calls Reset()

        // Arrange & Act
        var pagerState = new DataPagerState
        {
            Page = 5,     // This gets set first
            PageSize = 10, // This calls Reset(), setting Page back to 1
            Total = 100
        };

        // Assert - The Page is 1, not 5!
        Assert.Equal(1, pagerState.Page);
        Assert.Equal(10, pagerState.PageSize);
        Assert.Equal(100, pagerState.Total);
    }

    [Fact]
    public void ProperInitializationOrder_ShouldMaintainExpectedState()
    {
        // This test shows the correct way to initialize DataPagerState

        // Arrange & Act
        var pagerState = new DataPagerState();
        pagerState.Total = 100;
        pagerState.PageSize = 10; // This resets page to 1
        pagerState.Page = 5; // Now set the desired page

        // Assert
        Assert.Equal(5, pagerState.Page);
        Assert.Equal(10, pagerState.PageSize);
        Assert.Equal(100, pagerState.Total);
    }
}
