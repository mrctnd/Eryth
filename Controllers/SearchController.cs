using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Eryth.Services;
using Eryth.ViewModels;

namespace Eryth.Controllers
{
    [AllowAnonymous]
    [Route("Search")]
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string q)
        {
            try
            {
                var viewModel = new SearchPageViewModel
                {
                    Query = q ?? string.Empty,
                    Results = new SearchResultsViewModel()
                };

                if (!string.IsNullOrWhiteSpace(q))
                {
                    viewModel.Results = await _searchService.SearchAsync(User, q.Trim());
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Development ortamında hata detaylarını göster
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    return Content($"Search Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
                }
                
                // Production'da generic error
                var errorViewModel = new SearchPageViewModel
                {
                    Query = q ?? string.Empty,
                    Results = new SearchResultsViewModel()
                };
                
                return View(errorViewModel);
            }
        }
    }
}
