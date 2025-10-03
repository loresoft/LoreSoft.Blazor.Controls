// Ignore Spelling: Debounce Keydown Multiselect

using System.Linq.Expressions;
using System.Timers;

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
public partial class Typeahead<TItem, TValue> : ComponentBase, IDisposable
{
    private readonly System.Timers.Timer _debounceTimer;
    private readonly Queue<Func<Task>> _pending;

    /// <summary>
    /// Initializes a new instance of the <see cref="Typeahead{TItem, TValue}"/> component.
    /// </summary>
    public Typeahead()
    {
        Items = [];
        AllowClear = true;
        Loading = false;
        SearchMode = false;
        SelectedIndex = 0;
        SearchResults = [];
        SearchPlaceholder = "Search ...";

        _searchText = string.Empty;

        _debounceTimer = new System.Timers.Timer();
        _debounceTimer.AutoReset = false;
        _debounceTimer.Elapsed += (s, e) => InvokeAsync(() => Search(s, e));

        _pending = new Queue<Func<Task>>();
    }

    /// <summary>
    /// Gets or sets the cascading <see cref="EditContext"/> for form validation.
    /// </summary>
    [CascadingParameter]
    protected EditContext? EditContext { get; set; }

    /// <summary>
    /// Gets or sets additional attributes to be applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

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

    /// <summary>
    /// Gets or sets the placeholder text for the input control.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text for the search input.
    /// </summary>
    [Parameter]
    public string? SearchPlaceholder { get; set; }

    /// <summary>
    /// Gets or sets the list of items to display in the dropdown.
    /// </summary>
    [Parameter]
    public IReadOnlyCollection<TItem>? Items { get; set; }

    /// <summary>
    /// Gets or sets the asynchronous method to load items.
    /// </summary>
    [Parameter]
    public Func<Task<IEnumerable<TItem>>>? ItemLoader { get; set; }

    /// <summary>
    /// Gets or sets the asynchronous search method to filter items based on input text.
    /// </summary>
    [Parameter]
    public Func<string, Task<IEnumerable<TItem>>> SearchMethod { get; set; } = null!;

    /// <summary>
    /// Gets or sets the method to convert an item to its value.
    /// </summary>
    [Parameter]
    public Func<TItem, TValue?> ConvertMethod { get; set; } = null!;

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
    public RenderFragment<TItem> ResultTemplate { get; set; } = null!;

    /// <summary>
    /// Gets or sets the template for displaying the selected value.
    /// </summary>
    [Parameter]
    public RenderFragment<TValue> SelectedTemplate { get; set; } = null!;

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
    public bool AllowClear { get; set; }

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
    protected bool Loading { get; set; }

    /// <summary>
    /// Gets a value indicating whether the search menu is currently open.
    /// </summary>
    protected bool SearchMode { get; set; }

    /// <summary>
    /// Gets the list of search results.
    /// </summary>
    protected IList<TItem> SearchResults { get; set; }

    /// <summary>
    /// Gets the reference to the search input element.
    /// </summary>
    protected ElementReference SearchInput { get; set; }

    private string _searchText;

    /// <summary>
    /// Gets or sets the current search text.
    /// Triggers search or clears results based on input length.
    /// </summary>
    protected string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;

            if (value.Length == 0)
            {
                _debounceTimer.Stop();
                SearchResults.Clear();
                SelectedIndex = -1;

                ShowItems();
            }
            else if (value.Length >= MinimumLength)
            {
                _debounceTimer.Stop();
                _debounceTimer.Start();
            }
        }
    }

    /// <summary>
    /// Gets or sets the index of the currently selected search result.
    /// </summary>
    protected int SelectedIndex { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to prevent key events (e.g., form submit on Enter).
    /// </summary>
    protected bool PreventKey { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (SearchMethod == null)
        {
            if (typeof(TItem) != typeof(string) || Items == null)
                throw new InvalidOperationException($"{GetType()} requires a {nameof(SearchMethod)} parameter.");

            SearchMethod = (text) => Task.FromResult(Items.Where(x => x is string s && s.Contains(text, StringComparison.InvariantCultureIgnoreCase)));
        }

        if (ConvertMethod == null)
        {
            if (typeof(TItem) != typeof(TValue))
                throw new InvalidOperationException($"{GetType()} requires a {nameof(ConvertMethod)} parameter.");

            ConvertMethod = item => item is TValue value ? value : default;
        }

        SelectedTemplate ??= item => builder => builder.AddContent(0, item?.ToString());
        ResultTemplate ??= item => builder => builder.AddContent(0, item?.ToString());
        NoRecordsTemplate ??= builder => builder.AddContent(0, "No Records Found");
        LoadingTemplate ??= builder => builder.AddContent(0, "Loading ...");

        if (FieldIdentifier.Equals(default))
        {
            if (ValuesExpression != null)
                FieldIdentifier = FieldIdentifier.Create(ValuesExpression);
            else if (ValueExpression != null)
                FieldIdentifier = FieldIdentifier.Create(ValueExpression);
        }

        _debounceTimer.Interval = Debounce;
    }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (ItemLoader != null)
            _pending.Enqueue(LoadItems);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        while (_pending.Count > 0)
        {
            var action = _pending.Dequeue();
            await action();
        }
    }

    /// <summary>
    /// Performs a search using the current search text and updates the search results.
    /// </summary>
    /// <param name="source">The event source.</param>
    /// <param name="e">The elapsed event arguments.</param>
    public async void Search(object? source, ElapsedEventArgs e)
    {
        Loading = true;
        StateHasChanged();

        var result = await SearchMethod(_searchText);

        SearchResults = result?.ToList() ?? [];

        Loading = false;
        StateHasChanged();
    }

    /// <summary>
    /// Selects a search result item and updates the selected value(s).
    /// </summary>
    /// <param name="item">The item to select.</param>
    public async Task SelectResult(TItem item)
    {
        var value = ConvertMethod(item);
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

        SearchText = "";
        SearchMode = true;

        // need to wait for search input to render
        _pending.Enqueue(() => SearchInput.FocusAsync().AsTask());
    }

    /// <summary>
    /// Closes the search menu and stops loading.
    /// </summary>
    public void CloseMenu()
    {
        SearchMode = false;
        Loading = false;
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
    public async Task HandleKeydown(KeyboardEventArgs args)
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
    public bool IsMultiselect()
    {
        return ValuesExpression != null;
    }

    /// <summary>
    /// Determines whether there are search results to display.
    /// </summary>
    /// <returns><c>true</c> if there are search results; otherwise, <c>false</c>.</returns>
    public bool HasSearchResult()
    {
        return SearchMode
            && !Loading
            && SearchResults.Any();
    }

    /// <summary>
    /// Determines whether there is a selected value or values.
    /// </summary>
    /// <returns><c>true</c> if there is a value; otherwise, <c>false</c>.</returns>
    public bool HasValue()
    {
        return Value != null || Values?.Count > 0;
    }

    /// <summary>
    /// Gets the CSS class for a search result item based on selection state.
    /// </summary>
    /// <param name="item">The item to evaluate.</param>
    /// <param name="index">The index of the item.</param>
    /// <returns>The CSS class string.</returns>
    public string ResultClass(TItem item, int index)
    {
        const string resultClass = "typeahead-option-selected";

        if (index == SelectedIndex)
            return resultClass;

        var value = ConvertMethod(item);
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
    public string ControlClass()
    {
        var validationClass = EditContext != null
            ? EditContext.FieldCssClass(FieldIdentifier)
            : string.Empty;

        return CssBuilder
            .Default("typeahead-control")
            .AddClass("typeahead-active", SearchMode)
            .AddClass(validationClass, validationClass.HasValue())
            .ToString();
    }

    /// <summary>
    /// Loads items asynchronously using the <see cref="ItemLoader"/> method.
    /// </summary>
    public async Task LoadItems()
    {
        if (ItemLoader == null)
            return;

        Loading = true;
        StateHasChanged();

        var result = await ItemLoader();

        Items = result?.ToList() ?? [];

        Loading = false;
        StateHasChanged();
    }

    /// <summary>
    /// Disposes resources used by the component.
    /// </summary>
    public void Dispose()
    {
        _debounceTimer?.Dispose();
        GC.SuppressFinalize(this);
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
    /// Shows all items in the search results if not already present.
    /// </summary>
    private void ShowItems()
    {
        if (Items == null || Items.Count == 0 || SearchResults.Count != 0)
            return;

        SearchResults = [.. Items];
    }
}
