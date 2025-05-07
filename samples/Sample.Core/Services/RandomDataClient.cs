using Sample.Core.Models;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;

namespace Sample.Core.Services;

public class RandomDataClient
{
    public HttpClient HttpClient { get; }

    public RandomDataClient(HttpClient httpClient)
    {
        httpClient.BaseAddress = new Uri("https://random-data-api.com/");

        var mimeTypeHeader = new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json);
        httpClient.DefaultRequestHeaders.Accept.Add(mimeTypeHeader);

        HttpClient = httpClient;
    }

    public async Task<List<Bank>> GetBanks(int size = 100)
    {
        var url = $"/api/v2/banks";

        if (size > 0)
            url += $"?size={size}";

        return await HttpClient.GetFromJsonAsync<List<Bank>>(url);
    }
}
