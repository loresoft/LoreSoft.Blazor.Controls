# LoreSoft Blazor Controls

A comprehensive collection of high-quality Blazor components designed to enhance your web applications with rich user interface elements. This library provides everything from data visualization components to form controls, loading indicators, and utility components.

## Overview

The LoreSoft Blazor Controls project contains a collection of production-ready Blazor user controls that are easy to use, highly customizable, and built with performance in mind. Whether you need data grids, form inputs, loading indicators, or notification systems, this library has you covered.

### Quick Links

* **Demo**: [https://blazor.loresoft.com/](https://blazor.loresoft.com/ "LoreSoft Blazor Controls")
* **NuGet**: [https://nuget.org/packages/LoreSoft.Blazor.Controls](https://nuget.org/packages/LoreSoft.Blazor.Controls "NuGet Package")
* **Source**: [https://github.com/loresoft/LoreSoft.Blazor.Controls](https://github.com/loresoft/LoreSoft.Blazor.Controls "Project Source")

## Installing

The LoreSoft.Blazor.Controls library is available on nuget.org via package name `LoreSoft.Blazor.Controls`.

To install LoreSoft.Blazor.Controls, run the following command in the Package Manager Console

```shell
Install-Package LoreSoft.Blazor.Controls
```

## Setup

To use, you need to include the following CSS and JS files in your `index.html` (Blazor WebAssembly) or `_Host.cshtml` (Blazor Server).

In the head tag add the following CSS.

```html
<link rel="stylesheet" href="_content/LoreSoft.Blazor.Controls/BlazorControls.css" />
```

## Components

This library provides a comprehensive set of Blazor components organized into the following categories:

### Data Components

#### DataGrid

A powerful data grid component for displaying tabular data with advanced features:

* Column sorting (single and multi-column)
* Filtering and searching
* Row selection and grouping
* Detail views and expandable rows
* CSV export functionality
* Customizable column templates
* Built-in pagination support

#### DataList

A flexible list component for displaying data items using custom templates:

* Query-based filtering
* Field-based sorting
* CSV export capabilities
* Customizable row templates
* Simple and lightweight alternative to DataGrid

### Form Components

#### DateTimePicker

A comprehensive date and time input component supporting multiple data types:

* Supports `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, and `TimeSpan`
* Multiple picker modes (date, time, datetime)
* Built-in validation support
* Keyboard navigation
* Customizable format strings

#### InputImage

An image upload component with preview functionality:

* File upload with drag-and-drop support
* Image preview with automatic resizing
* Data URL conversion
* File size validation
* Configurable maximum file size

#### ToggleSwitch

A modern toggle switch component for boolean values:

* Smooth animations
* Two-way data binding
* Form validation support
* Customizable styling
* Supports both `bool` and `bool?` types

#### Typeahead

An advanced autocomplete/search component with rich features:

* Asynchronous search functionality
* Debounced search input
* Single and multi-select modes
* Customizable templates for results and selections
* Minimum character length configuration
* Built-in form validation support

### UI Components

#### BusyButton

A button component that shows loading state during operations:

* Automatic disable during busy state
* Customizable busy indicator
* Built-in loading text
* Smooth state transitions

#### Conditional

A utility component for conditional rendering:

* Simple boolean-based content switching
* Separate templates for true/false states
* Clean alternative to `@if` statements in templates

#### LoadingBlock

A loading overlay component for indicating progress:

* Overlay or replacement loading modes
* Customizable loading template
* Smooth fade transitions
* Flexible positioning

#### ProgressBar

An animated progress bar with service-based state management:

* Smooth animations with customizable duration
* Service-based progress updates
* Auto-hide on completion
* Multiple progress operations support

#### Skeleton

A skeleton placeholder component for loading states:

* Multiple shape types (text, rectangle, circle)
* Customizable dimensions
* Smooth loading animations
* Responsive design support

### Notification Components

#### Toaster

A comprehensive toast notification system:

* Multiple notification levels (Info, Success, Warning, Error)
* Configurable positioning
* Auto-dismiss with custom timeouts
* Custom templates and styling
* Queue management for multiple toasts

### Query Components

#### QueryBuilder

A visual query builder for complex filtering:

* Dynamic field configuration
* Nested group support
* Multiple operators per field type
* Real-time query updates
* Export to various query formats

### Utility Components

#### Gravatar

A component for displaying Gravatar images:

* Automatic MD5 hash generation
* Fallback image support
* Multiple Gravatar modes
* Configurable ratings and sizes

#### LazyValue

A component for asynchronously loading and displaying values:

* Key-based value loading
* Custom loading templates
* Error handling support
* Caching capabilities

#### LocalTime

A component for displaying dates and times in the user's local timezone:

* Automatic timezone conversion
* Supports `DateTime` and `DateTimeOffset`
* Customizable display formats
* Semantic HTML time elements

#### Repeater

A simple repeater component for rendering collections:

* Custom item templates
* Empty state templates
* Clean alternative to `@foreach` loops
* Strongly typed item context

## Examples

### Typeahead Component

The Typeahead component provides powerful search and selection capabilities:

### Basic Example

State selection dropdown.  Bind to `Value` property for single selection mode.

```html
<Typeahead SearchMethod="@SearchState"
           Items="Data.StateList"
           @bind-Value="@SelectedState"
           Placeholder="State">
    <SelectedTemplate Context="state">
        @state.Name
    </SelectedTemplate>
    <ResultTemplate Context="state">
        @state.Name
    </ResultTemplate>
</Typeahead>
```

```csharp
@code {
    public StateLocation SelectedState { get; set; }

    public Task<IEnumerable<StateLocation>> SearchState(string searchText)
    {
        var result = Data.StateList
            .Where(x => x.Name.ToLower().Contains(searchText.ToLower()))
            .ToList();

        return Task.FromResult<IEnumerable<StateLocation>>(result);
    }
}
```

### Multiselect Example

When you want to be able to select multiple results.  Bind to the `Values` property.  The target property must be type `IList<T>`.

```html
<Typeahead SearchMethod="@SearchPeople"
           Items="Data.PersonList"
           @bind-Values="@SelectedPeople"
           Placeholder="Owners">
    <SelectedTemplate Context="person">
        @person.FullName
    </SelectedTemplate>
    <ResultTemplate Context="person">
        @person.FullName
    </ResultTemplate>
</Typeahead>
```

```csharp
@code {
    public IList<Person> SelectedPeople;

    public Task<IEnumerable<Person>> SearchPeople(string searchText)
    {
        var result = Data.PersonList
            .Where(x => x.FullName.ToLower().Contains(searchText.ToLower()))
            .ToList();

        return Task.FromResult<IEnumerable<Person>>(result);
    }
 }
 ```

### GitHub Repository Search

Use Octokit to search for a GitHub repository.

```html
<Typeahead SearchMethod="@SearchGithub"
           @bind-Value="@SelectedRepository"
           Placeholder="Repository"
           MinimumLength="3">
    <SelectedTemplate Context="repo">
        @repo.FullName
    </SelectedTemplate>
    <ResultTemplate Context="repo">
        <div class="github-repository clearfix">
            <div class="github-avatar"><img src="@repo.Owner.AvatarUrl"></div>
            <div class="github-meta">
                <div class="github-title">@repo.FullName</div>
                <div class="github-description">@repo.Description</div>
                <div class="github-statistics">
                    <div class="github-forks"><i class="fa fa-flash"></i> @repo.ForksCount Forks</div>
                    <div class="github-stargazers"><i class="fa fa-star"></i> @repo.StargazersCount Stars</div>
                    <div class="github-watchers"><i class="fa fa-eye"></i> @repo.SubscribersCount Watchers</div>
                </div>
            </div>
        </div>
    </ResultTemplate>
</Typeahead>
```

```csharp
@inject IGitHubClient GitHubClient;

@code {
    public Repository SelectedRepository { get; set; }

    public async Task<IEnumerable<Repository>> SearchGithub(string searchText)
    {
        var request = new SearchRepositoriesRequest(searchText);
        var result = await GitHubClient.Search.SearchRepo(request);

        return result.Items;
    }
}
```
