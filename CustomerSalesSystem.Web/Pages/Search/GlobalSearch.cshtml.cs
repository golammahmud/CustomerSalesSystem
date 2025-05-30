using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerSalesSystem.Web.Pages.Search
{
    public class GlobalSearchModel(IHttpClientFactory httpClientFactory, IFilterQueryFromAIService filterQueryService) : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IFilterQueryFromAIService _filterQueryService = filterQueryService;

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; } = string.Empty;
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
            if (string.IsNullOrWhiteSpace(SearchQuery)) return Page();

            // Step 1: Send to DeepSeek API
            var queryInfo = await _filterQueryService.GetFilterQueryFromOpenAPI(SearchQuery);

            // Step 2: Apply filters

            return Page();
        }

    }
}
