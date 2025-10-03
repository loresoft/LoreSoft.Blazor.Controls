using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A Blazor component that renders different content based on the current breakpoint from a <see cref="BreakpointProvider"/>.
/// This component follows a mobile-first approach where content cascades down from larger breakpoints to smaller ones.
/// If content is not specified for the current breakpoint, it will automatically fall back to the next smaller breakpoint
/// until it finds available content or reaches the <see cref="ExtraSmall"/> fallback.
/// </summary>
/// <remarks>
/// This component must be used within a <see cref="BreakpointProvider"/> component to function properly.
/// The breakpoint hierarchy is: xxl → xl → lg → md → sm → xs (fallback).
/// </remarks>
/// <example>
/// <code>
/// &lt;BreakpointProvider&gt;
///     &lt;BreakpointView&gt;
///         &lt;ExtraSmall&gt;
///             &lt;p&gt;Mobile content&lt;/p&gt;
///         &lt;/ExtraSmall&gt;
///         &lt;Medium&gt;
///             &lt;div class="desktop-layout"&gt;
///                 &lt;p&gt;Desktop content&lt;/p&gt;
///             &lt;/div&gt;
///         &lt;/Medium&gt;
///     &lt;/BreakpointView&gt;
/// &lt;/BreakpointProvider&gt;
/// </code>
/// </example>
public partial class BreakpointView : ComponentBase, IDisposable
{
    private RenderFragment? _currentFragment;
    private BreakpointProvider? _provider;

    /// <summary>
    /// Gets or sets the <see cref="BreakpointProvider"/> instance from the cascading value.
    /// This provider is used to monitor breakpoint changes and determine which content to render.
    /// </summary>
    /// <value>
    /// The <see cref="BreakpointProvider"/> instance that provides breakpoint change notifications.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown during component initialization if no <see cref="BreakpointProvider"/> is found in the cascading values.
    /// </exception>
    [CascadingParameter]
    public BreakpointProvider? Provider { get; set; }

    /// <summary>
    /// Gets or sets the content to display for extra small breakpoints (xs) and up.
    /// This serves as the ultimate fallback content if no other breakpoint-specific content is available.
    /// Typically used for mobile-first responsive design.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> containing the content to render for extra small viewports (typically &lt; 576px).
    /// </value>
    /// <remarks>
    /// This is the fallback content that will be used if no content is specified for larger breakpoints.
    /// In a mobile-first design approach, this should contain your base mobile layout.
    /// </remarks>
    [Parameter]
    public RenderFragment? ExtraSmall { get; set; }

    /// <summary>
    /// Gets or sets the content to display for small breakpoints (sm) and up.
    /// This content will be used for small viewports and will cascade down to extra small if not specified.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> containing the content to render for small viewports (typically ≥ 576px).
    /// </value>
    /// <remarks>
    /// If this content is not specified, the component will fall back to <see cref="ExtraSmall"/> content.
    /// </remarks>
    [Parameter]
    public RenderFragment? Small { get; set; }

    /// <summary>
    /// Gets or sets the content to display for medium breakpoints (md) and up.
    /// This content will be used for medium viewports and will cascade down to smaller breakpoints if not specified.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> containing the content to render for medium viewports (typically ≥ 768px).
    /// </value>
    /// <remarks>
    /// If this content is not specified, the component will fall back to <see cref="Small"/> or <see cref="ExtraSmall"/> content.
    /// </remarks>
    [Parameter]
    public RenderFragment? Medium { get; set; }

    /// <summary>
    /// Gets or sets the content to display for large breakpoints (lg) and up.
    /// This content will be used for large viewports and will cascade down to smaller breakpoints if not specified.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> containing the content to render for large viewports (typically ≥ 992px).
    /// </value>
    /// <remarks>
    /// If this content is not specified, the component will fall back to smaller breakpoint content in this order:
    /// <see cref="Medium"/> → <see cref="Small"/> → <see cref="ExtraSmall"/>.
    /// </remarks>
    [Parameter]
    public RenderFragment? Large { get; set; }

    /// <summary>
    /// Gets or sets the content to display for extra large breakpoints (xl) and up.
    /// This content will be used for extra large viewports and will cascade down to smaller breakpoints if not specified.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> containing the content to render for extra large viewports (typically ≥ 1200px).
    /// </value>
    /// <remarks>
    /// If this content is not specified, the component will fall back to smaller breakpoint content in this order:
    /// <see cref="Large"/> → <see cref="Medium"/> → <see cref="Small"/> → <see cref="ExtraSmall"/>.
    /// </remarks>
    [Parameter]
    public RenderFragment? ExtraLarge { get; set; }

    /// <summary>
    /// Gets or sets the content to display for extra extra large breakpoints (xxl) and up.
    /// This content will be used for the largest viewports and will cascade down to smaller breakpoints if not specified.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> containing the content to render for extra extra large viewports (typically ≥ 1400px).
    /// </value>
    /// <remarks>
    /// If this content is not specified, the component will fall back to smaller breakpoint content in this order:
    /// <see cref="ExtraLarge"/> → <see cref="Large"/> → <see cref="Medium"/> → <see cref="Small"/> → <see cref="ExtraSmall"/>.
    /// </remarks>
    [Parameter]
    public RenderFragment? ExtraExtraLarge { get; set; }

    /// <summary>
    /// Builds the render tree for the component by adding the current fragment based on the active breakpoint.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to construct the render tree.</param>
    /// <remarks>
    /// This method is called by the Blazor framework during the rendering process.
    /// It outputs the content fragment that corresponds to the current viewport breakpoint.
    /// </remarks>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (_currentFragment != null)
            builder.AddContent(0, _currentFragment);
    }

    /// <summary>
    /// Initializes the component and subscribes to breakpoint change notifications from the provider.
    /// This method also sets the initial content fragment based on the current breakpoint.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the component is not used within a <see cref="BreakpointProvider"/>.
    /// The component requires a <see cref="BreakpointProvider"/> to be present in the cascading parameter chain.
    /// </exception>
    /// <remarks>
    /// This method is called once during the component lifecycle, after the component parameters have been set
    /// but before the first render. It establishes the connection to the breakpoint provider and sets up
    /// the initial display state.
    /// </remarks>
    protected override void OnInitialized()
    {
        if (Provider == null)
            throw new InvalidOperationException($"{nameof(BreakpointView)} must be used within a {nameof(BreakpointProvider)}.");

        _provider = Provider;
        _provider.Subscribe(OnBreakpointChanged);

        // Set initial fragment based on current breakpoint
        UpdateCurrentFragment(_provider.Current);
    }

    /// <summary>
    /// Handles breakpoint change notifications from the <see cref="BreakpointProvider"/> and updates the current fragment.
    /// This method is called automatically when the viewport size changes and crosses a breakpoint threshold.
    /// </summary>
    /// <param name="breakpointChanged">
    /// The <see cref="BreakpointChanged"/> data containing information about the current breakpoint,
    /// previous breakpoint, and viewport width.
    /// </param>
    /// <remarks>
    /// This method updates the component's display content and triggers a re-render to reflect the new breakpoint.
    /// The re-render is scheduled asynchronously to ensure proper component lifecycle management.
    /// </remarks>
    private void OnBreakpointChanged(BreakpointChanged breakpointChanged)
    {
        UpdateCurrentFragment(breakpointChanged.Current);
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Updates the current fragment based on the breakpoint hierarchy using a mobile-first cascade approach.
    /// The method selects the most appropriate content fragment for the current breakpoint, falling back
    /// to smaller breakpoints if content is not available for the current breakpoint.
    /// </summary>
    /// <param name="currentBreakpoint">
    /// The current breakpoint name (e.g., "xs", "sm", "md", "lg", "xl", "xxl").
    /// If the breakpoint name cannot be parsed, defaults to "xs".
    /// </param>
    /// <remarks>
    /// <para>
    /// The fallback hierarchy ensures that content is always available by cascading from larger to smaller breakpoints:
    /// </para>
    /// <list type="bullet">
    /// <item><description>xxl → xl → lg → md → sm → xs</description></item>
    /// <item><description>xl → lg → md → sm → xs</description></item>
    /// <item><description>lg → md → sm → xs</description></item>
    /// <item><description>md → sm → xs</description></item>
    /// <item><description>sm → xs</description></item>
    /// <item><description>xs (ultimate fallback)</description></item>
    /// </list>
    /// <para>
    /// This approach follows responsive design best practices where you define content for specific breakpoints
    /// and let it cascade down to smaller viewports when specific content isn't provided.
    /// </para>
    /// </remarks>
    private void UpdateCurrentFragment(string currentBreakpoint)
    {
        if (string.IsNullOrWhiteSpace(currentBreakpoint))
        {
            _currentFragment = null;
            return;
        }

        // Convert string breakpoint to enum for comparison
        if (!Enum.TryParse<Breakpoints>(currentBreakpoint, true, out var current))
        {
            current = Breakpoints.xs;
        }

        // Find the most appropriate fragment based on breakpoint hierarchy
        // Start from the current breakpoint and work down to find the first available fragment
        _currentFragment = current switch
        {
            Breakpoints.xxl => ExtraExtraLarge ?? ExtraLarge ?? Large ?? Medium ?? Small ?? ExtraSmall,
            Breakpoints.xl => ExtraLarge ?? Large ?? Medium ?? Small ?? ExtraSmall,
            Breakpoints.lg => Large ?? Medium ?? Small ?? ExtraSmall,
            Breakpoints.md => Medium ?? Small ?? ExtraSmall,
            Breakpoints.sm => Small ?? ExtraSmall,
            Breakpoints.xs => ExtraSmall,
            _ => ExtraSmall
        };
    }

    /// <summary>
    /// Disposes the component and unsubscribes from breakpoint change notifications to prevent memory leaks.
    /// This method ensures proper cleanup of event subscriptions and suppresses the finalizer for performance.
    /// </summary>
    public void Dispose()
    {
        _provider?.Unsubscribe(OnBreakpointChanged);
        GC.SuppressFinalize(this);
    }
}
