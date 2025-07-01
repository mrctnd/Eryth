using System.Security.Claims;
using Eryth.ViewModels;

namespace Eryth.Services
{
    public interface ISearchService
    {
        Task<SearchResultsViewModel> SearchAsync(ClaimsPrincipal user, string query);
        Task<List<string>> GetSearchSuggestionsAsync(string query, int maxResults = 5);
    }
}
