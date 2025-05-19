using CustomerSalesSystem.Application.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerSalesSystem.Web.Pages.Customers
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

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

        
    }
}
