namespace LoreSoft.Blazor.Controls;

public class ProgressBarHandler : DelegatingHandler
{
    private readonly ProgressBarState _progressState;

    public ProgressBarHandler(ProgressBarState progressState)
    {
        _progressState = progressState;
    }

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
