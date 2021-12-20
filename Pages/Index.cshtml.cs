using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SITConnect.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("passwordReset") != null) return RedirectToPage("ChangePassword");

            // Get whether user is authorised after OTP
            if (HttpContext.Session.GetString("otpAuthorisation") == "0") return RedirectToPage("Login");

            return Page();
        }
    }
}