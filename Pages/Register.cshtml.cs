using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SITConnect.Pages
{
    public class Register : PageModel
    {
        // Import PasswordFunc
        private readonly PasswordFunc _pwdFunc = new PasswordFunc();

        [BindProperty] public string CustFname { get; set; }
        [BindProperty] public string CustLname { get; set; }
        [BindProperty] public string BillingCardno { get; set; }
        [BindProperty] public string CustEmail { get; set; }
        [BindProperty] public string CustPwd { get; set; }
        [BindProperty] public string CustDob { get; set; }
        [BindProperty] public string CustPhoto { get; set; }

        public string CustPasswordStatus { get; set; }

        public void OnGet()
        {
            // Empty
        }

        public IActionResult OnPost()
        {
            CustPasswordStatus = _pwdFunc.GetPasswordStrength(CustPwd);
            return Page();
        }
    }
}