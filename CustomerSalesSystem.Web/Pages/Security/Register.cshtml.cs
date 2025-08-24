using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Domain;
using CustomerSalesSystem.Web.Helper;
using CustomerSalesSystem.Web.Services.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerSalesSystem.Web.Pages.Security
{
    public class RegisterModel(AuthService authService) : PageModel
    {
        private readonly AuthService _authService = authService;
        [BindProperty]
        public RegisterRequestDTO RegisterRequest { get; set; } = new();

        public string? Message { get; set; }
        public void OnGet()
        {
            RegisterRequest = new RegisterRequestDTO();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _authService.RegisterAsync(RegisterRequest);
            if (result == null)
            {
                ModelState.AddModelError("", "Invalid Data.Registration failed");
                return Page();
            }

            Toast.Show("Registration has been successfully done!", ToastType.Success, "Success");
            return RedirectToPage(PageNavigation.Login);
        }
    }
}
