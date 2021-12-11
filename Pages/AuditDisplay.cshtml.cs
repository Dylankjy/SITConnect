using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SITConnect.Models;
using SITConnect.Services;

namespace SITConnect.Pages
{
    public class AuditDisplay : PageModel
    {
        private readonly AuditLogService _auditDb;
        private readonly UserService _userDb;

        public AuditDisplay(UserService userService, AuditLogService auditService)
        {
            _userDb = userService;
            _auditDb = auditService;
        }

        public List<AuditLog> AuditLogsToDisplay { get; set; }
        public User CurrentUser { get; set; }
        
        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("passwordReset") != null) return RedirectToPage("ChangePassword");
            if (!ModelState.IsValid && HttpContext.Session.GetString("user") == null)
            {
                return RedirectToPage("/Error403");
            }
            
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("user"));
            
            AuditLogsToDisplay = _auditDb.GetLogsByUserId(CurrentUser.Id).AsEnumerable().Reverse().ToList();
            return Page();
        }
    }
}