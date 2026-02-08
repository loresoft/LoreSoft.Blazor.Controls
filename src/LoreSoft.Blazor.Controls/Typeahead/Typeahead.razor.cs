// Ignore Spelling: Debounce Keydown Multiselect Typeahead

using System.Linq.Expressions;

using LoreSoft.Blazor.Controls.Abstracts;
using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A component that provides typeahead (autocomplete) functionality for selecting items.
/// Supports single and multi-select, custom search, templates, and debounced search input.
/// </summary>
/// <typeparam name="TItem">The type of the item displayed in the dropdown.</typeparam>
/// <typeparam name="TValue">The type of the value selected by the user.</typeparam>
[CascadingTypeParameter(nameof(TItem))]
[CascadingTypeParameter(nameof(TValue))]
public partial class Typeahead<TItem, TValue> : StandardComponent
{
    private readonly DebounceAction<string?> _debouncer = new();
    private readonly LoadingState _loadingState = new();

    /// <summary>
    /// Gets or sets the cascading <see cref="EditContext"/> for form validation.
    /// </summary>
    [CascadingParameter]
    protected EditContext? EditContext { get; set; }

    #region Value Binding
    /// <summary>
    /// Gets or sets the selected value for single-select mode.
    /// </summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected value changes.
    /// </summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the value expression for form validation.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue>>? ValueExpression { get; set; }
    #endregion

    #region Values Binding
    /// <summary>
    /// Gets or sets the selected values for multi-select mode.
    /// </summary>
    [Parameter]
    public IList<TValue>? Values { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected values change.
    /// </summary>
    [Parameter]
    public EventCallback<IList<TValue>> ValuesChanged { get; set; }

    /// <summary>
    /// Gets or sets the values expression for form validation in multi-select mode.
    /// </summary>
    [Parameter]
    public Expression<Func<IList<TValue>>>? ValuesExpression { get; set; }

    #endregion

    /// <summary>
    /// Gets or sets the placeholder text for the input control.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text for the search input.
    /// </summary>
    [Parameter]
    public string? SearchPlaceholder { get; set; } = "Search ...";


    /// <summary>
    /// Gets or sets the list of items to display in the dropdown.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>
    /// Gets or sets the asynchronous method to load items.
    /// </summary>
    [Parameter]
    public Func<Task<IEnumerable<TItem>>>? ItemLoader { get; set; }


    /// <summary>
    /// Gets or sets the asynchronous search method to filter items based on input text.
    /// </summary>
    [Parameter]
    public Func<string, Task<IEnumerable<TItem>>>? SearchMethod { get; set; }

    /// <summary>
    /// Gets or sets the method to convert an item to its value.
    /// </summary>
    [Parameter]
    public Func<TItem?, TValue?>? ConvertMethod { get; set; }

    /// <summary>
    /// Gets or sets the asynchronous method to look up an item by its value.
    /// Used to convert a value back to its corresponding item.
    /// </summary>
    [Parameter]
    public Func<TValue?, Task<TItem?>>? LookupMethod { get; set; }


    /// <summary>
    /// Gets or sets the template to display when no records are found.
    /// </summary>
    [Parameter]
    public RenderFragment? NoRecordsTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template to display while loading search results.
    /// </summary>
    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for displaying each search result item.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? ResultTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for displaying the selected value.
    /// </summary>
    [Parameter]
    public RenderFragment<TValue>? SelectedTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for the dropdown footer.
    /// </summary>
    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }


    /// <summary>
    /// Gets or sets the minimum number of characters required to trigger a search.
    /// </summary>
    [Parameter]
    public int MinimumLength { get; set; } = 1;

    /// <summary>
    /// Gets or sets the debounce interval (in milliseconds) for search input.
    /// </summary>
    [Parameter]
    public int Debounce { get; set; } = 800;

    /// <summary>
    /// Gets or sets a value indicating whether the clear button is allowed.
    /// </summary>
    [Parameter]
    public bool AllowClear { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the control is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the field identifier for form validation.
    /// </summary>
    [Parameter]
    public FieldIdentifier FieldIdentifier { get; set; }


    /// <summary>
    /// Gets a value indicating whether the control is currently loading search results.
    /// </summary>
    protected bool Loading => _loadingState.IsLoading;

    /// <summary>
    /// Gets a value indicating whether the search menu is currently open.
    /// </summary>
    protected bool SearchMode { get; set; }

    /// <summary>
    /// Gets the list of search results.
    /// </summary>
    protected List<TItem> SearchResults { get; set; } = [];

    /// <summary>
    /// Gets or sets the current search text.
    /// Triggers search or clears results based on input length.
    /// </summary>
    protected string SearchText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the index of the currently selected search result.
    /// </summary>
    protected int SelectedIndex { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to prevent key events (e.g., form submit on Enter).
    /// </summary>
    protected bool PreventKey { get; set; }

    protected int TabIndex => Disabled ? -1 : 0;

    /// <summary>
    /// Gets the list of current items. Either from Items parameter or loaded via ItemLoader.
    /// </summary>
    protected List<TItem> CurrentItems { get; set; } = [];

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // copy Items to CurrentItems if not set
        if (Items != null && ItemLoader == null && CurrentItems.Count == 0)
            CurrentItems.AddRange(Items);

        // set up field identifier for validation
        if (FieldIdentifier.Equals(default))
        {
            if (ValuesExpression != null)
                FieldIdentifier = FieldIdentifier.Create(ValuesExpression);
            else if (ValueExpression != null)
                FieldIdentifier = FieldIdentifier.Create(ValueExpression);
        }
    }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // load items if an item loader is provided
        // will overwrite Items parameter if both are set
        if (!Disabled && ItemLoader != null)
            ExecuteAfterRender(LoadItems);
    }


    /// <summary>
    /// Performs a search using the current search text and updates the search results.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    public async Task Search(string? searchText)
    {
        // do not search if disabled or not in search mode
        if (Disabled || !SearchMode)
            return;

        // set loading state
        using var loading = _loadingState.Start();
        StateHasChanged();

        IEnumerable<TItem>? result = null;

        if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < MinimumLength)
        {
            // if search text is null or below minimum length, return all items
            result = CurrentItems;
        }
        else if (SearchMethod != null)
        {
            // use custom search method
            result = await SearchMethod(searchText);
        }
        else if (typeof(TItem) == typeof(string) && CurrentItems.Count > 0)
        {
            // default search: filter items containing the search text (case-insensitive)
            result = CurrentItems.Where(x => x is string s && s.Contains(searchText, StringComparison.InvariantCultureIgnoreCase));
        }
        else
        {
            // no search method available
            throw new InvalidOperationException($"Typeahead component requires a {nameof(SearchMethod)} parameter.");
        }

        // apply search results
        SearchResults.Clear();
        if (result != null)
            SearchResults.AddRange(result);

        // reset loading state
        loading.Dispose();
        StateHasChanged();
    }

    /// <summary>
    /// Handles search input changes with debouncing to prevent excessive searches.
    /// </summary>
    private async Task HandleSearchInput()
        => await _debouncer.Debounce(SearchText, Search, TimeSpan.FromMilliseconds(Debounce));


    /// <summary>
    /// Selects a search result item and updates the selected value(s).
    /// </summary>
    /// <param name="item">The item to select.</param>
    public async Task SelectResult(TItem item)
    {
        var value = ConvertItem(item);
        if (value == null)
            return;

        if (IsMultiselect())
        {
            var valueList = Values ?? [];
            if (!valueList.Remove(value))
                valueList.Add(value);

            await ValuesChanged.InvokeAsync(valueList);
        }
        else
        {
            await ValueChanged.InvokeAsync(value);
        }

        EditContext?.NotifyFieldChanged(FieldIdentifier);
        CloseMenu();
    }

    /// <summary>
    /// Removes a value from the selected values in multi-select mode.
    /// </summary>
    /// <param name="item">The value to remove.</param>
    public async Task RemoveValue(TValue item)
    {
        var valueList = Values ?? [];
        valueList.Remove(item);

        await ValuesChanged.InvokeAsync(valueList);
        EditContext?.NotifyFieldChanged(FieldIdentifier);
    }

    /// <summary>
    /// Clears the selected value(s).
    /// </summary>
    public async Task Clear()
    {
        if (IsMultiselect())
            await ValuesChanged.InvokeAsync([]);
        else
            await ValueChanged.InvokeAsync(default);

        EditContext?.NotifyFieldChanged(FieldIdentifier);
        CloseMenu();
    }


    /// <summary>
    /// Shows the search menu and focuses the search input.
    /// </summary>
    public void ShowMenu()
    {
        if (Disabled || SearchMode)
            return;

        SearchText = string.Empty;
        SearchMode = true;
        SelectedIndex = -1;
        SearchResults.Clear();

        // need to wait for search input to render
        ExecuteAfterRender(async () =>
        {
            // set focus to search input
            if (Element != null)
                await Element.Value.FocusAsync();

            // perform initial search
            await Search(SearchText);
        });
    }

    /// <summary>
    /// Closes the search menu and stops loading.
    /// </summary>
    public void CloseMenu()
    {
        SearchMode = false;
        SelectedIndex = -1;
        SearchResults.Clear();
    }

    /// <summary>
    /// Toggles the search menu open or closed.
    /// </summary>
    public void ToggleMenu()
    {
        if (SearchMode)
            CloseMenu();
        else
            ShowMenu();
    }


    /// <summary>
    /// Handles keydown events for navigation and selection in the search menu.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    private async Task HandleKeydown(KeyboardEventArgs args)
    {
        // prevent form submit on enter
        PreventKey = (args.Key == "Enter");

        if (args.Key == "ArrowDown")
            MoveSelection(1);
        else if (args.Key == "ArrowUp")
            MoveSelection(-1);
        else if ((args.Key == "Enter" || args.Key == "Tab") && SelectedIndex >= 0 && SelectedIndex < SearchResults.Count)
            await SelectResult(SearchResults[SelectedIndex]);
        else if (args.Key == "Escape")
            CloseMenu();

        // close menu when tabbing out
        if (args.Key == "Tab")
            CloseMenu();
    }

    /// <summary>
    /// Determines whether the control is in multi-select mode.
    /// </summary>
    /// <returns><c>true</c> if multi-select; otherwise, <c>false</c>.</returns>
    private bool IsMultiselect()
    {
        return ValuesExpression != null;
    }

    /// <summary>
    /// Determines whether there are search results to display.
    /// </summary>
    /// <returns><c>true</c> if there are search results; otherwise, <c>false</c>.</returns>
    private bool HasSearchResult()
    {
        return SearchMode
            && !Loading
            && SearchResults.Any();
    }

    /// <summary>
    /// Determines whether there is a selected value or values.
    /// </summary>
    /// <returns><c>true</c> if there is a value; otherwise, <c>false</c>.</returns>
    private bool HasValue()
    {
        return Value != null || Values?.Count > 0;
    }

    /// <summary>
    /// Gets the CSS class for a search result item based on selection state.
    /// </summary>
    /// <param name="item">The item to evaluate.</param>
    /// <param name="index">The index of the item.</param>
    /// <returns>The CSS class string.</returns>
    private string ResultClass(TItem item, int index)
    {
        const string resultClass = "typeahead-option-selected";

        if (index == SelectedIndex)
            return resultClass;

        var value = ConvertItem(item);
        if (value == null)
            return string.Empty;

        if (!IsMultiselect())
        {
            return Equals(value, Value)
                ? resultClass
                : string.Empty;
        }

        if (Values == null || Values.Count == 0)
            return string.Empty;

        return Values.Contains(value)
            ? resultClass
            : string.Empty;
    }

    /// <summary>
    /// Gets the CSS class for the typeahead control based on validation and active state.
    /// </summary>
    /// <returns>The CSS class string.</returns>
    private string? ControlClass()
    {
        var validationClass = EditContext != null
            ? EditContext.FieldCssClass(FieldIdentifier)
            : string.Empty;

        return CssBuilder.Pool.Use(builder =>
        {
            return builder
                .AddClass("typeahead-control")
                .AddClass("typeahead-active", SearchMode)
                .AddClass("typeahead-disabled", Disabled)
                .AddClass(validationClass, validationClass.HasValue())
                .ToString();
        });
    }

    /// <summary>
    /// Loads items asynchronously using the <see cref="ItemLoader"/> method.
    /// </summary>
    private async ValueTask LoadItems()
    {
        if (ItemLoader == null)
            return;

        using var loading = _loadingState.Start();
        StateHasChanged();

        var result = await ItemLoader();

        CurrentItems.Clear();
        if (result != null)
            CurrentItems.AddRange(result);

        // trigger result update if search menu active
        if (SearchMode)
            await HandleSearchInput();

        loading.Dispose();
        StateHasChanged();
    }

    /// <summary>
    /// Moves the selection index by the specified count, wrapping around the search results.
    /// </summary>
    /// <param name="count">The amount to move the selection index.</param>
    private void MoveSelection(int count)
    {
        var index = SelectedIndex + count;

        if (index >= SearchResults.Count)
            index = 0;

        if (index < 0)
            index = SearchResults.Count - 1;

        SelectedIndex = index;
    }

    /// <summary>
    /// Converts an item to its corresponding value using <see cref="ConvertMethod"/> or direct casting.
    /// </summary>
    /// <param name="item">The item to convert.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when conversion is not possible and no <see cref="ConvertMethod"/> is provided.</exception>
    private TValue? ConvertItem(TItem item)
    {
        if (ConvertMethod is not null)
            return ConvertMethod(item);

        if (item is TValue value)
            return value;

        throw new InvalidOperationException($"Typeahead component requires a {nameof(ConvertMethod)} parameter.");
    }
}
