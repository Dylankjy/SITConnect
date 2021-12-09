using IdGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SITConnect.Models;
using SITConnect.Services;

namespace SITConnect.Pages
{
    public class Login : PageModel
    {
        private readonly UserService _svc;

        public Login(UserService service)
        {
            _svc = service;
        }

        [BindProperty] public string Email { get; set; }
        [BindProperty] public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            var selectedUser = _svc.GetUserByEmail(Email);

            // Handle when email is invalid
            if (selectedUser == null)
            {
                // This generates a random string of bytes as the password to prevent timing attacks
                // Treats an invalid email address as valid and passes it through the Hashing Algorithm to produce
                // a timing similar to valid email addresses.
                // Uses snowflake id generator for random bytes
                selectedUser = new User();
                selectedUser.SetPassword(new IdGenerator(0).CreateId().ToString());
            }

            // Compare passwords
            if (!selectedUser.ComparePassword(Password))
            {
                ErrorMessage = "Login details does not match our records. Please check your email and password.";
                return Page();
            }


            return Page();
        }
    }
}