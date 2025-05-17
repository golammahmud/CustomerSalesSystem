using CustomerSalesSystem.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerSalesSystem.Web.Pages.Customers
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public EditModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private HttpClient ApiClient => _clientFactory.CreateClient("API");

        [BindProperty]
        public CustomerDto Customer { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var response = await ApiClient.GetFromJsonAsync<CustomerDto>($"customers/{id}");

            if (response == null)
                return NotFound();

            Customer = response;
            return Page();
        }
    }
}
