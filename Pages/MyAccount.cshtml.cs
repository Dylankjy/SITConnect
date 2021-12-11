using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SITConnect.Models;

namespace SITConnect.Pages
{
    public class MyAccount : PageModel
    {
        public User CurrentUser;
        public string CurrentUserCardNumber;

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("user") == null) return RedirectToPage("Error403");
            if (HttpContext.Session.GetString("passwordReset") != null) return RedirectToPage("ChangePassword");

            // Get user in session
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("user"));
            string cardNo = CurrentUser.GetCardNo();
            CurrentUserCardNumber = "**** **** **** " + cardNo.Substring(11, 4);

            return Page();
        }
    }
}