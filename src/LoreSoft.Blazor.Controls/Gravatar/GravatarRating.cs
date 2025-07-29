namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Specifies the content rating for Gravatar images, restricting which images are shown based on their appropriateness.
/// </summary>
public enum GravatarRating
{
    /// <summary>
    /// Suitable for display on all websites with any audience type ("General Audience").
    /// </summary>
    g,

    /// <summary>
    /// May contain rude gestures, provocatively dressed individuals, the lesser swear words, or mild violence ("Parental Guidance Suggested").
    /// </summary>
    pg,

    /// <summary>
    /// May contain harsh profanity, intense violence, nudity, or hard drug use ("Restricted").
    /// </summary>
    r,

    /// <summary>
    /// May contain hardcore sexual imagery or extremely disturbing violence ("Explicit").
    /// </summary>
    x
}
