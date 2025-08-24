using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Domain;
using CustomerSalesSystem.Web.Helper;
using CustomerSalesSystem.Web.Services.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerSalesSystem.Web.Pages.Customers
{
    public class CreateModel : PageModel
    {
        private readonly CustomerService _customerService;

        public CreateModel(CustomerService customerService)
        {
            _customerService = customerService;
        }

        [BindProperty]
        public CustomerDto Customer { get; set; } = new();

        // GET handler
        public void OnGet()
        {
            // You can initialize any default values for Customer here if needed
        }

        // POST handler
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); // Validation failed
            }

            try
            {
                await _customerService.CreateAsync(Customer);
                Toast.Show("Customer create successful!", ToastType.Success, "Success");
                return RedirectToPage(PageNavigation.CustomerList); // Redirect to customers list after creation
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating customer: {ex.Message}");
                return Page();
            }
        }
    }
}
