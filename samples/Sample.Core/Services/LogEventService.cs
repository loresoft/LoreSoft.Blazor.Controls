using LoreSoft.Blazor.Controls;
using LoreSoft.Blazor.Controls.Extensions;

using Sample.Core.Models;

namespace Sample.Core.Services;

public class LogEventService
{
    private readonly IReadOnlyCollection<LogEvent> _logEvents = Data.GenerateLogEvents();

    public DataResult<LogEvent> Load(DataRequest dataRequest)
    {
        // apply search query, sorting skipped in key-set paging
        var queryable = _logEvents
            .AsQueryable()
            .Filter(dataRequest.Query);

        if (dataRequest.PageSize <= 0)
            return new DataResult<LogEvent>([.. queryable]);

        // apply continuation token for paging
        if (int.TryParse(dataRequest.ContinuationToken, out var lastId) && lastId > 0)
            queryable = queryable.Where(l => l.Id > lastId);

        // retrieve items, adding one extra to determine if there is a next page
        var items = queryable
            .Take(dataRequest.PageSize + 1)
            .OrderBy(l => l.Id)
            .ToList();


        // there is a next page if collected items exceed the page size
        string? nextToken = null;
        if (items.Count > dataRequest.PageSize)
        {
            // remove extra item
            items.RemoveAt(items.Count - 1);

            // get the last item's id for the continuation token
            var lastItem = items[^1];

            nextToken = lastItem.Id.ToString();
        }

        return new DataResult<LogEvent>(items, continuationToken: nextToken);
    }
}
