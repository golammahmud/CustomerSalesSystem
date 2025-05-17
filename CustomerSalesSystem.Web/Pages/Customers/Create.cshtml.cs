using CustomerSalesSystem.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerSalesSystem.Web.Pages.Customers
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public CustomerDto Customer { get; set; } = new();
    }
}
