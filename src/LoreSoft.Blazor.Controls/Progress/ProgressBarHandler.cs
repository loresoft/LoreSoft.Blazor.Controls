namespace LoreSoft.Blazor.Controls;

/// <summary>
/// An HTTP message handler that manages the progress bar state for outgoing requests.
/// Starts the progress bar when a request begins and completes it when the response is received.
/// </summary>
public class ProgressBarHandler : DelegatingHandler
{
    private readonly ProgressBarState _progressState;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressBarHandler"/> class.
    /// </summary>
    /// <param name="progressState">The <see cref="ProgressBarState"/> used to control the progress bar.</param>
    public ProgressBarHandler(ProgressBarState progressState)
    {
        _progressState = progressState;
    }

    /// <summary>
    /// Sends an HTTP request asynchronously and updates the progress bar state.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The HTTP response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _progressState.Start();

        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            _progressState.Complete();
        }
    }
}
