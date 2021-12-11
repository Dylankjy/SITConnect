using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SITConnect.Models;
using SITConnect.Services;

namespace SITConnect.Pages
{
    public class ChangePassword : PageModel
    {
        private readonly AuditLogService _auditDb;
        private readonly UserService _userDb;

        public ChangePassword(UserService userService, AuditLogService auditService)
        {
            _userDb = userService;
            _auditDb = auditService;
        }

        [BindProperty]
        public string OldPassword { get; set; }
        [BindProperty]
        public string NewPassword { get; set; }
        
        public string PasswordStrength { get; set; }
        public string CurrentPasswordStatus { get; set; }
        public string SuccessMessage { get; set; }
        public bool CanChangePassword { get; set; } = true;
        
        public IActionResult OnGet()
        {
            // Ensure that user is already logged in
            if (HttpContext.Session.GetString("user") == null)
            {
                return RedirectToPage("Error403");
            }
            
            // Get user in session
            var currentUser = new User().FromJson(HttpContext.Session.GetString("user"));
            
            var passwordLogsByUserId = _auditDb.GetLogsByUserId(currentUser.Id)
                .Where(o => o.Timestamp >= DateTime.Now.AddMinutes(-5) && o.LogType == "password_changed");

            if (passwordLogsByUserId.Any())
            {
                CanChangePassword = false;
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            var currentUser = new User().FromJson(HttpContext.Session.GetString("user"));

            if (!currentUser.ComparePassword(OldPassword))
            {
                CurrentPasswordStatus = "Password did not match our current records.";
                return Page();
            }
            
            // Check password history
            string[] arrayOfPassword = currentUser.Password.Split(";~;");
            int abortPasswordChange = 0;

            // Check the first password history
            if (BCrypt.Net.BCrypt.Verify(NewPassword, arrayOfPassword[0]))
            {
                abortPasswordChange++;
            }

            // Catch if password no exist
            try
            {
                if (BCrypt.Net.BCrypt.Verify(NewPassword, arrayOfPassword[1]))
                {
                    abortPasswordChange++;
                }
            }
            catch (IndexOutOfRangeException)
            {
                // Empty
            }
            try
            {
                if (BCrypt.Net.BCrypt.Verify(NewPassword, arrayOfPassword[2]))
                {
                    abortPasswordChange++;
                }
            }
            catch (IndexOutOfRangeException)
            {
                // Empty
            }
            
            if (abortPasswordChange != 0)
            {
                PasswordStrength = "This password is too recent to be reused.";
                return Page();
            }

            var auditObject = new AuditLog
            {
                ActorId = currentUser.Id,
                Timestamp = DateTime.Now,
                LogType = "password_changed",
                IpAddress = HttpContext.Connection.RemoteIpAddress.ToString()
            };
            
            _auditDb.AddLog(auditObject);
            
            currentUser.SetPassword(NewPassword);
            _userDb.UpdateUser(currentUser);
            
            SuccessMessage = "Your password has been successfully changed.";
            return Page();
        }
    }
}