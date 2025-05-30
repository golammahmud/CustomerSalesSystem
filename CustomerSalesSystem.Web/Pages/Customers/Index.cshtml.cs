using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerSalesSystem.Web.Pages.Customers
{
    public class IndexModel(IHttpClientFactory httpClientFactory, IFilterQueryFromAIService filterQueryService) : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IFilterQueryFromAIService _filterQueryService = filterQueryService;

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }= string.Empty;
        public List<object> Results { get; set; } = new();


        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public List<CustomerDto>? Customers { get; set; }
        private HttpClient ApiClient => _httpClientFactory.CreateClient("API");


        public async Task OnGetAsync(int pageNumber = 1)
        {
            PageNumber = pageNumber;

            var response = await ApiClient.GetFromJsonAsync<PagedResult<CustomerDto>>(
                $"customers?pageNumber={PageNumber}&pageSize={PageSize}");

            if (response is not null)
            {
                Customers = response.Items;
                TotalCount = response.TotalCount;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return Page();

            var aiResult = await _filterQueryService.GetFilterQueryFromOpenAPI(SearchQuery);

            if (aiResult == null || aiResult.Filters.Count == 0)
            {
                ModelState.AddModelError("", "Could not understand your query.");
                return Page();
            }

            var searchRequest = new
            {
                Filters = aiResult.Filters,
                PageNumber = 1,
                PageSize = PageSize
            };

            var response = await ApiClient.PostAsJsonAsync("customers/search", searchRequest);

            if (response.IsSuccessStatusCode)
            {
                var pagedResult = await response.Content.ReadFromJsonAsync<PagedResult<CustomerDto>>();
                if (pagedResult is not null)
                {
                    Customers = pagedResult.Items;
                    TotalCount = pagedResult.TotalCount;
                }
            }
            else
            {
                ModelState.AddModelError("", "Search failed.");
            }

            return Page();
        }


    }
}
