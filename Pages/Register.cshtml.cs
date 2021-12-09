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

        private readonly UserService _svc;

        public Register(UserService service)
        {
            _svc = service;
        }

        [BindProperty] public User NewUser { get; set; }
        [BindProperty] public string IncomingPasswordText { get; set; }
        [BindProperty] public string BillingCardNo { get; set; }

        public string CustPasswordStatus { get; set; }

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
            NewUser.SetCardNo(BillingCardNo);

            Console.WriteLine(NewUser.Id);
            Console.WriteLine(NewUser.Email);
            Console.WriteLine(NewUser.FirstName);
            Console.WriteLine(NewUser.LastName);
            Console.WriteLine(NewUser.DateOfBirth);
            Console.WriteLine(NewUser.GetCardNo());

            // Create user account
            _svc.AddUser(NewUser);

            return RedirectToPage('/');
        }
    }
}