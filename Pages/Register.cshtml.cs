using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SITConnect.Models;
using SITConnect.Services;

namespace SITConnect.Pages
{
    public class Register : PageModel
    {
        private readonly AuditLogService _auditDb;

        // Import PasswordFunc
        private readonly PasswordFunc _pwdFunc = new PasswordFunc();

        private readonly UserService _userDb;

        public Register(UserService userService, AuditLogService auditService)
        {
            _userDb = userService;
            _auditDb = auditService;
        }

        [BindProperty] public User NewUser { get; set; }
        [BindProperty] public string IncomingPasswordText { get; set; }
        [BindProperty] public string BillingCardNo { get; set; }

        public string CustPasswordStatus { get; set; }
        public string EmailStatus { get; set; }
        public string CardStatus { get; set; }
        public string Message { get; set; }

        public IActionResult OnGet()
        {
            // Prevent loggedin from viewing this page
            if (ModelState.IsValid && HttpContext.Session.GetString("user") != null)
            {
                // Get user in session
                var currentUser = new User().FromJson(HttpContext.Session.GetString("user"));

                return RedirectToPage("/MyAccount");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            // Check whether password is strong enough
            if (_pwdFunc.GetPasswordStrength(IncomingPasswordText) != "Excellent")
            {
                CustPasswordStatus = "Your password is too weak. Please enter a stronger one";
                return Page();
            }

            // Special fields setters
            NewUser.SetPassword(IncomingPasswordText);
            var isCardValid = NewUser.SetCardNo(BillingCardNo);

            if (!isCardValid)
            {
                CardStatus = "The card number provided is not valid.";
                return Page();
            }

            // Check for existing user accounts with the same email address
            if (_userDb.GetUserByEmail(NewUser.Email) != null)
            {
                EmailStatus = "This email is already in use.";
                return Page();
            }

            // Create user account
            _userDb.AddUser(NewUser);

            // Add to audit log
            var auditObject = new AuditLog();
            auditObject.ActorId = NewUser.Id;
            auditObject.Timestamp = DateTime.Now;
            auditObject.LogType = "create_account";
            auditObject.IpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            _auditDb.AddLog(auditObject);

            Message = "Your account has successfully been created. You may login now.";

            return Page();
        }
    }
}

// Console.WriteLine(NewUser.Id);
// Console.WriteLine(NewUser.Email);
// Console.WriteLine(NewUser.FirstName);
// Console.WriteLine(NewUser.LastName);
// Console.WriteLine(NewUser.DateOfBirth);
// Console.WriteLine(NewUser.GetCardNo());