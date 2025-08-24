using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Web.Helper;
using CustomerSalesSystem.Web.Services;
using CustomerSalesSystem.Web.Services.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerSalesSystem.Web.Pages.Customers
{
    public class IndexModel(IHttpClientFactory httpClientFactory, IFilterQueryFromAIService filterQueryService, CustomerService customerService) : BasePageModel
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IFilterQueryFromAIService _filterQueryService = filterQueryService;
        private readonly CustomerService _customerService = customerService;

        [BindProperty(SupportsGet = true)]
        public string CsSearchQuery { get; set; }= string.Empty;
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
            if (string.IsNullOrWhiteSpace(CsSearchQuery))
                return Page();

            AIQueryResult? aiResult = null;

            try
            {
                aiResult = await _filterQueryService.GetFilterQueryFromOpenAPI(CsSearchQuery);
            }
            catch (Exception ex)
            {
                // Optional: log the exception
                // _logger.LogError(ex, "OpenAPI query failed. Falling back to local parser.");
                aiResult = CustomerSearchFallbackParser.Parse(CsSearchQuery);
            }

            // Fallback if result is null or has no filters
            if (aiResult == null || !aiResult.Filters.Any())
            {
                aiResult = CustomerSearchFallbackParser.Parse(CsSearchQuery);
            }

            // Final check
            if (aiResult == null || !aiResult.Filters.Any())
            {
                ModelState.AddModelError("", "Could not understand your query.");
                Speak("Could not understand your query.");
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

                    if (pagedResult?.Items.Any() == false && !string.IsNullOrWhiteSpace(CsSearchQuery))
                    {
                        Speak("Sorry no search data found..try again with valid data");
                    }
                    else
                    {
                        Speak("sarch result found ..here you go");
                    }
                   
                }
                
            }
            else
            {
                ModelState.AddModelError("", "Search failed.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (id <= 0) return BadRequest();

            var result = await _customerService.GetByIdAsync(id);
            if (result is null) return NotFound();

            await _customerService.DeleteAsync(id);

            return RedirectToPage( PageNavigation.CustomerList);
        }
    }
}
