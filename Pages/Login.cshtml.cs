using System;
using System.Linq;
using IdGen;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SITConnect.Models;
using SITConnect.Services;

namespace SITConnect.Pages
{
    public class Login : PageModel
    {
        private readonly AuditLogService _auditDb;
        private readonly UserService _userDb;

        public Login(UserService userService, AuditLogService auditService)
        {
            _userDb = userService;
            _auditDb = auditService;
        }

        [BindProperty] public string Email { get; set; }
        [BindProperty] public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("user") != null)
            {
                // Get user in session
                var currentUser = new User().FromJson(HttpContext.Session.GetString("user"));

                return RedirectToPage("/MyAccount");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            // Declare variable
            var selectedUser = _userDb.GetUserByEmail(Email);
            var auditObject = new AuditLog();
            
            // If selectedUser is invalid, initialise with empty object
            selectedUser ??= new User();
            
            // Check whether account or IP is blocked
            // This block of code blocks the account or IP for 15 minutes after 3 attempts
            var auditLogsByUserId = _auditDb.GetLogsByUserId(selectedUser.Id)
                .Where(o => o.Timestamp >= DateTime.Now.AddMinutes(-15) && o.LogType == "login_failed");
            
            var auditLogsByIp = _auditDb.GetLogsByIp(HttpContext.Connection.RemoteIpAddress.ToString())
                .Where(o => o.Timestamp >= DateTime.Now.AddMinutes(-15) && o.LogType == "login_failed");

            if (auditLogsByUserId.Count() > 2 || auditLogsByIp.Count() > 2)
            {
                ErrorMessage = "Too many login attempts. Try again later.";
                return Page();
            }


            // Handle when email is invalid
            if (selectedUser.Email == null)
            {
                // This generates a random string of bytes as the password to prevent timing attacks
                // Treats an invalid email address as valid and passes it through the Hashing Algorithm to produce
                // a timing similar to valid email addresses.
                // Uses snowflake id generator for random bytes
                selectedUser.SetPassword(new IdGenerator(0).CreateId().ToString());
            }


            // Compare passwords
            if (!selectedUser.ComparePassword(Password))
            {
                ErrorMessage = "Login details does not match our records. Please check your email and password.";

                // Add to audit log only if account is associated to an account
                if (selectedUser.Email != null)
                {
                    auditObject.ActorId = selectedUser.Id;
                    auditObject.Timestamp = DateTime.Now;
                    auditObject.LogType = "login_failed";
                    auditObject.IpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
                    _auditDb.AddLog(auditObject);
                } else {
                    auditObject.ActorId = 0;
                    auditObject.Timestamp = DateTime.Now;
                    auditObject.LogType = "login_failed";
                    auditObject.IpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
                    _auditDb.AddLog(auditObject);
                }

                return Page();
            }

            // If all is well, set the session
            HttpContext.Session.SetString("user", selectedUser.ToJson());

            // And then add a new audit log accounting for the successful login
            auditObject.ActorId = selectedUser.Id;
            auditObject.Timestamp = DateTime.Now;
            auditObject.LogType = "login_success";
            auditObject.IpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            _auditDb.AddLog(auditObject);

            return RedirectToPage("/MyAccount");
        }
    }
}