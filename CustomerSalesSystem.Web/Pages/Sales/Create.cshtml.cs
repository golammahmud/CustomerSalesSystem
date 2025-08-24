using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Web.Services.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerSalesSystem.Web.Pages.Sales
{
    public class CreateModel : PageModel
    {
        private readonly SalesService _saleService;

        public CreateModel( SalesService saleService)
        {
            _saleService = saleService;
        }

        [BindProperty]
        public CreateSaleDto Sale { get; set; } = new();

        public void  OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            if (Sale.ProductId == 0)
            {
                ModelState.AddModelError(string.Empty, "Selected product not found.");
                return Page();
            }


            await _saleService.CreateAsync(Sale);

            return RedirectToPage(PageNavigation.SalesList);
        }
    }
}
