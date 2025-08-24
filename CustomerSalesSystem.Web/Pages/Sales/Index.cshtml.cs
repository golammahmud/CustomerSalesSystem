using CustomerSalesSystem.Web.Services.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerSalesSystem.Web.Pages.Sales
{
    public class IndexModel : PageModel
    {
        private readonly CustomerService _customerService;
        private readonly SalesService _salesService;

        public IndexModel(CustomerService customerService, SalesService saleService)
        {
            _customerService = customerService;
            _salesService = saleService;
        }

        public void OnGet()
        {
        }

  
        // ✅ AJAX handler to load sales
        public async Task<IActionResult> OnGetSalesAsync()
        {
            var sales = await _salesService.GetAllAsync();
            return new JsonResult(sales);
        }

        // ✅ AJAX handler for filter
        public async Task<IActionResult> OnGetFilterSalesAsync(int? customerId, DateTime? date)
        {
            var sales = await _salesService.FilterAsync(customerId, date);
            return new JsonResult(sales);
        }
    }
}
