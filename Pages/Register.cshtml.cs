using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SITConnect.Models;
using SITConnect.Services;

namespace SITConnect.Pages
{
    public class Register : PageModel
    {
        // Import PasswordFunc
        private readonly PasswordFunc _pwdFunc = new PasswordFunc();

        private readonly UserService _userDb;
        private readonly AuditLogService _auditDb;

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

        public void OnGet()
        {
            // Empty
        }

        public IActionResult OnPost()
        {
            Console.WriteLine("New user account");
            // CustPasswordStatus = _pwdFunc.GetPasswordStrength(IncomingPasswordText);

            // if (CustPasswordStatus == "Weak" || CustPasswordStatus == "Very Weak")
            // {
            //     return Page();
            // }

            NewUser.SetPassword(IncomingPasswordText);
            bool isCardValid = NewUser.SetCardNo(BillingCardNo);

            if (!isCardValid)
            {
                CardStatus = "The card number provided is not valid.";
                return Page();
            }

            Console.WriteLine(NewUser.Id);
            Console.WriteLine(NewUser.Email);
            Console.WriteLine(NewUser.FirstName);
            Console.WriteLine(NewUser.LastName);
            Console.WriteLine(NewUser.DateOfBirth);
            Console.WriteLine(NewUser.GetCardNo());
            // Check for existing user accounts with the same email address
            if (_userDb.GetUserByEmail(NewUser.Email) != null)
            {
                EmailStatus = "This email is already in use.";
                return Page();
            }

            // Create user account
            _userDb.AddUser(NewUser);

            return RedirectToPage('/');
        }
    }
}