using LoreSoft.Blazor.Controls.Utilities;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A strongly-typed dictionary for building modal component parameters with a fluent API.
/// </summary>
/// <remarks>
/// This class extends <see cref="Dictionary{TKey,TValue}"/> and provides convenient fluent methods
/// for setting common modal parameters such as message, title, variant, and action button text.
/// It allows for easy parameter construction when showing modal dialogs.
/// </remarks>
/// <example>
/// <code>
/// var parameters = ModalParameters.Create()
///     .Title("Confirm Action")
///     .Message("Are you sure?")
///     .Variant(ModalVariant.Warning)
///     .PrimaryAction("Yes")
///     .SecondaryAction("No");
/// </code>
/// </example>
public class ModalParameters : Dictionary<string, object>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModalParameters"/> class that is empty.
    /// </summary>
    public ModalParameters()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModalParameters"/> class that contains elements copied from the specified dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary whose elements are copied to the new <see cref="ModalParameters"/>.</param>
    public ModalParameters(IDictionary<string, object> dictionary) : base(dictionary)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModalParameters"/> class that contains elements copied from the specified dictionary
    /// and uses the specified comparer.
    /// </summary>
    /// <param name="dictionary">The dictionary whose elements are copied to the new <see cref="ModalParameters"/>.</param>
    /// <param name="comparer">The comparer to use when comparing keys, or <c>null</c> to use the default comparer.</param>
    public ModalParameters(IDictionary<string, object> dictionary, IEqualityComparer<string>? comparer) : base(dictionary, comparer)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModalParameters"/> class that contains elements copied from the specified collection.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new <see cref="ModalParameters"/>.</param>
    public ModalParameters(IEnumerable<KeyValuePair<string, object>> collection) : base(collection)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModalParameters"/> class that contains elements copied from the specified collection
    /// and uses the specified comparer.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new <see cref="ModalParameters"/>.</param>
    /// <param name="comparer">The comparer to use when comparing keys, or <c>null</c> to use the default comparer.</param>
    public ModalParameters(IEnumerable<KeyValuePair<string, object>> collection, IEqualityComparer<string>? comparer) : base(collection, comparer)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModalParameters"/> class that is empty and uses the specified comparer.
    /// </summary>
    /// <param name="comparer">The comparer to use when comparing keys, or <c>null</c> to use the default comparer.</param>
    public ModalParameters(IEqualityComparer<string>? comparer) : base(comparer)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModalParameters"/> class that is empty and has the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the <see cref="ModalParameters"/> can contain.</param>
    public ModalParameters(int capacity) : base(capacity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModalParameters"/> class that is empty, has the specified initial capacity,
    /// and uses the specified comparer.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the <see cref="ModalParameters"/> can contain.</param>
    /// <param name="comparer">The comparer to use when comparing keys, or <c>null</c> to use the default comparer.</param>
    public ModalParameters(int capacity, IEqualityComparer<string>? comparer) : base(capacity, comparer)
    {
    }


    /// <summary>
    /// Sets the message text for the modal dialog.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    /// <returns>The current <see cref="ModalParameters"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
    /// <remarks>
    /// This sets the <see cref="ModalComponentBase.Message"/> parameter.
    /// </remarks>
    public ModalParameters Message(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        this[nameof(ModalComponentBase.Message)] = message;

        return this;
    }

    /// <summary>
    /// Sets the title for the modal dialog.
    /// </summary>
    /// <param name="title">The title text to display.</param>
    /// <returns>The current <see cref="ModalParameters"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="title"/> is null.</exception>
    /// <remarks>
    /// This sets the <see cref="ModalComponentBase.Title"/> parameter.
    /// </remarks>
    public ModalParameters Title(string title)
    {
        ArgumentNullException.ThrowIfNull(title);

        this[nameof(ModalComponentBase.Title)] = title;

        return this;
    }

    /// <summary>
    /// Sets the visual variant for the modal dialog.
    /// </summary>
    /// <param name="variant">The modal variant to apply.</param>
    /// <returns>The current <see cref="ModalParameters"/> instance for method chaining.</returns>
    /// <remarks>
    /// This sets the <see cref="ModalComponentBase.Variant"/> parameter, which determines the visual styling of the modal.
    /// </remarks>
    public ModalParameters Variant(ModalVariant variant)
    {
        this[nameof(ModalComponentBase.Variant)] = variant;
        return this;
    }

    /// <summary>
    /// Sets the text for the primary action button. The primary action is typically used for confirming or proceeding with an action.
    /// </summary>
    /// <param name="primaryAction">The text to display on the primary action button.</param>
    /// <returns>The current <see cref="ModalParameters"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="primaryAction"/> is null or empty.</exception>
    /// <remarks>
    /// This sets the <see cref="ModalComponentBase.PrimaryAction"/> parameter.
    /// </remarks>
    public ModalParameters PrimaryAction(string primaryAction)
    {
        ArgumentException.ThrowIfNullOrEmpty(primaryAction);

        this[nameof(ModalComponentBase.PrimaryAction)] = primaryAction;

        return this;
    }

    /// <summary>
    /// Sets the text for the secondary action button. The secondary action is typically used for canceling or dismissing the modal.
    /// </summary>
    /// <param name="secondaryAction">The text to display on the secondary action button.</param>
    /// <returns>The current <see cref="ModalParameters"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="secondaryAction"/> is null or empty.</exception>
    /// <remarks>
    /// This sets the <see cref="ModalComponentBase.SecondaryAction"/> parameter.
    /// </remarks>
    public ModalParameters SecondaryAction(string secondaryAction)
    {
        ArgumentException.ThrowIfNullOrEmpty(secondaryAction);

        this[nameof(ModalComponentBase.SecondaryAction)] = secondaryAction;

        return this;
    }


    /// <summary>
    /// Sets the CSS class name(s) for the modal dialog container.
    /// </summary>
    /// <param name="className">The CSS class name(s) to apply.</param>
    /// <returns>The current <see cref="ModalParameters"/> instance for method chaining.</returns>
    /// <remarks>
    /// This sets the "class" parameter that will be applied to the modal dialog container element.
    /// </remarks>
    public ModalParameters ClassName(string className)
    {
        this["class"] = className;

        return this;
    }

    /// <summary>
    /// Sets the CSS class name(s) for the modal dialog container using a <see cref="CssBuilder"/>.
    /// </summary>
    /// <param name="action">An action that configures the <see cref="CssBuilder"/> to build the CSS classes.</param>
    /// <returns>The current <see cref="ModalParameters"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method provides a fluent API for building conditional and dynamic CSS classes.
    /// The resulting class string is set as the "class" parameter.
    /// </remarks>
    /// <example>
    /// <code>
    /// parameters.ClassName(css => css
    ///     .AddClass("custom-modal")
    ///     .AddClass("large", isLarge));
    /// </code>
    /// </example>
    public ModalParameters ClassName(Action<CssBuilder> action)
    {
        this["class"] = CssBuilder.Pool.Use(builder =>
        {
            action(builder);
            return builder.ToString() ?? string.Empty;
        });

        return this;
    }


    /// <summary>
    /// Sets the inline CSS styles for the modal dialog container.
    /// </summary>
    /// <param name="rawStyle">The raw CSS style string to apply.</param>
    /// <returns>The current <see cref="ModalParameters"/> instance for method chaining.</returns>
    /// <remarks>
    /// This sets the "style" parameter that will be applied to the modal dialog container element.
    /// The style string should be in the format "property: value; property: value;".
    /// </remarks>
    public ModalParameters Style(string rawStyle)
    {
        this["style"] = rawStyle;

        return this;
    }

    /// <summary>
    /// Sets the inline CSS styles for the modal dialog container using a <see cref="StyleBuilder"/>.
    /// </summary>
    /// <param name="action">An action that configures the <see cref="StyleBuilder"/> to build the CSS styles.</param>
    /// <returns>The current <see cref="ModalParameters"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method provides a fluent API for building conditional and dynamic CSS styles.
    /// The resulting style string is set as the "style" parameter.
    /// </remarks>
    /// <example>
    /// <code>
    /// parameters.Style(style => style
    ///     .AddStyle("width", "500px")
    ///     .AddStyle("z-index", "9999", when: isTopmost));
    /// </code>
    /// </example>
    public ModalParameters Style(Action<StyleBuilder> action)
    {
        this["style"] = StyleBuilder.Pool.Use(builder =>
        {
            action(builder);
            return builder.ToString() ?? string.Empty;
        });

        return this;
    }


    /// <summary>
    /// Sets a custom parameter with the specified name and value.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    /// <returns>The current <see cref="ModalParameters"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null.</exception>
    /// <remarks>
    /// This method allows setting arbitrary parameters that can be used by custom modal components.
    /// </remarks>
    public ModalParameters Parameter(string name, object value)
    {
        ArgumentNullException.ThrowIfNull(name);

        this[name] = value;

        return this;
    }


    /// <summary>
    /// Creates a new empty <see cref="ModalParameters"/> instance.
    /// </summary>
    /// <returns>A new <see cref="ModalParameters"/> instance.</returns>
    /// <remarks>
    /// This is a convenient factory method for creating a new instance using collection expression syntax.
    /// </remarks>
    public static ModalParameters Create() => [];
}
