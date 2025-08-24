using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Domain;
using CustomerSalesSystem.Web.Helper;
using CustomerSalesSystem.Web.Services.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerSalesSystem.Web.Pages.Sales
{
    public class CreateModel : BasePageModel
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

            Toast.Show("Sale created successfully!", ToastType.Success, "Success");
            return RedirectToPage(PageNavigation.SalesList);
        }
    }
}
