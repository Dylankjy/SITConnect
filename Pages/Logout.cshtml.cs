using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SITConnect.Pages
{
    public class Logout : PageModel
    {
        public void OnGet()
        {
            // Empty
        }

        public IActionResult OnPost()
        {
            // Clears session and logs user out.
            HttpContext.Session.Clear();
            return RedirectToPage("Index");
        }
    }
}