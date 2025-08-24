using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Domain;
using CustomerSalesSystem.Web.Helper;
using CustomerSalesSystem.Web.Services;
using CustomerSalesSystem.Web.Services.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerSalesSystem.Web.Pages.Customers
{
    public class EditModel : PageModel
    {
        private readonly CustomerService _customerService;

        public EditModel(CustomerService customerService)
        {
            _customerService = customerService;
        }

        [BindProperty]
        public CustomerDto Customer { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var response = await _customerService.GetByIdAsync(id);

            if (response == null)
                return NotFound();

            Customer = response;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); // Validation failed
            }

            try
            {
                await _customerService.UpdateAsync(Customer);
                Toast.Show("Customer has been updated successfully!", ToastType.Success, "Success");
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
