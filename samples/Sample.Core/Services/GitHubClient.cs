using Sample.Core.Models.GitHub;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Sample.Core.Services
{
    public class GitHubClient
    {
        public HttpClient HttpClient { get; }

        public GitHubClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("https://api.github.com/");

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.mercy-preview+json"));

            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GitHubClient", "1.0.0"));

            HttpClient = httpClient;
        }

        public async Task<SearchResult<Repository>> SearchRepositories(string text, int? page = null, int? pageSize = null)
        {
            var encoded = Uri.EscapeDataString(text);
            var url = $"search/repositories?q={encoded}";

            if (page > 0)
                url += $"&page={page}";

            if (pageSize > 0)
                url += $"&per_page={pageSize}";

            var result = await HttpClient.GetFromJsonAsync<SearchResult<Repository>>(url);

            return result;
        }
    }
}
