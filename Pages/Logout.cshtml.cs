using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SITConnect.Models;
using SITConnect.Services;

namespace SITConnect.Pages
{
    public class Logout : PageModel
    {
        private readonly AuditLogService _auditDb;
        private readonly UserService _userDb;

        public Logout(UserService userService, AuditLogService auditService)
        {
            _userDb = userService;
            _auditDb = auditService;
        }

        public IActionResult OnGet()
        {
            return RedirectToPage("Login");
        }

        public IActionResult OnPost()
        {
            // Check whether user is active in session
            if (HttpContext.Session.GetString("user") == null) return RedirectToPage("Login");

            // Add to audit
            _auditDb.AddLog(new AuditLog
            {
                ActorId = new User().FromJson(HttpContext.Session.GetString("user")).Id,
                IpAddress = HttpContext.Connection.RemoteIpAddress.ToString(),
                LogType = "logout"
            });

            // Clears session and logs user out.
            HttpContext.Session.Clear();
            Response.Cookies.Delete(".AspNetCore.Session");

            return RedirectToPage("Index");
        }
    }
}