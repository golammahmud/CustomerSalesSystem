using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Web.Services.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace CustomerSalesSystem.Web.Pages.Security
{
    public class LoginModel(AuthService authService) : PageModel
    {
        private readonly AuthService _authService = authService;
        [BindProperty]
        public LoginRequest LoginRequest { get; set; } = new();

        public string? Message { get; set; }


        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostLogOffAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToPage(PageNavigation.Login);
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return Page();

            var authResult = await _authService.LoginAsync(LoginRequest);
            if (authResult == null)
            {
                ModelState.AddModelError("", "Invalid credentials");
                return Page();
            }

            // Sign in the user for Razor Pages authorization
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, LoginRequest.Username)
        };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirect to return URL if provided
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToPage("/Customers/Index");
        }

    }
}
