using CustomerSalesSystem.Application.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerSalesSystem.Web.Pages.Customers
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<CustomerDto>? Customers { get; set; }
        private HttpClient ApiClient => _httpClientFactory.CreateClient("API");
        public async Task OnGetAsync()
        {
            Customers = await ApiClient.GetFromJsonAsync<List<CustomerDto>>("customers");
        }
    }
}
