## Overview

The LoreSoft Blazor Controls project contains a collection of Blazor user controls.

* Demo: [https://blazor.loresoft.com/](https://blazor.loresoft.com/ "LoreSoft Blazor Controls")
* NuGet: [https://nuget.org/packages/LoreSoft.Blazor.Controls](https://nuget.org/packages/LoreSoft.Blazor.Controls "NuGet Package")
* Source: [https://github.com/loresoft/LoreSoft.Blazor.Controls](https://github.com/loresoft/LoreSoft.Blazor.Controls "Project Source")

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

Then add the JS script at the bottom of the page using the following script tag.

```html
<script src="_content/LoreSoft.Blazor.Controls/BlazorControls.js"></script>
```

## Typeahead Features

* Searching data by supplying a search function
* Template for search result, selected value, and footer
* Debounce support for smoother search
* Character limit before searching
* Multiselect support
* Built in form validation support

## Typeahead Properties

### Required

* **Value** - Bind to `Value` in single selection mode.  Mutually exclusive to `Values` property.
* **Values** - Bind to `Values` in multiple selection mode.  Mutually exclusive to `Value` property.
* **SearchMethod** - The method used to return search result

### Optional

* **AllowClear** - Allow the selected value to be cleared
* **ConvertMethod** - The method used to convert search result type to the value type
* **Debounce** - Time to wait, in milliseconds, after last key press before starting a search
* **Items** - The initial list of items to show when there isn't any search text
* **MinimumLength** - Minimum number of characters before starting a search
* **Placeholder** - The placeholder text to show when nothing is selected

### Templates

* **ResultTemplate** - User defined template for displaying a result in the results list
* **SelectedTemplate** - User defined template for displaying the selected item(s)
* **NoRecordsTemplate** - Template for when no items are found
* **FooterTemplate** - Template displayed at the end of the results list
* **LoadingTemplate** - Template displayed while searching

## Typeahead Examples

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
