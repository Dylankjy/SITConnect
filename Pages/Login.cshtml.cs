using System;
using System.Linq;
using AspNetCore.ReCaptcha;
using IdGen;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SITConnect.Models;
using SITConnect.Services;

#pragma warning disable 4014

namespace SITConnect.Pages
{
    public class Login : PageModel
    {
        private readonly AuditLogService _auditDb;
        private readonly OtpCodeService _otpDb;
        private readonly UserService _userDb;

        public Login(UserService userService, AuditLogService auditService, OtpCodeService otpCodeService)
        {
            _userDb = userService;
            _auditDb = auditService;
            _otpDb = otpCodeService;
        }

        [BindProperty] public string Email { get; set; }
        [BindProperty] public string Password { get; set; }

        public string ErrorMessage { get; set; }

        [BindProperty] public string OtpAuthorisation { get; set; }
        [BindProperty] public int OtpCode { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("user") != null)
            {
                // Get user in session
                var currentUser = new User().FromJson(HttpContext.Session.GetString("user"));

                // Get whether user is authorised after OTP
                var isOtpAuthorised = HttpContext.Session.GetString("otpAuthorisation");
                OtpAuthorisation = isOtpAuthorised;
                if (isOtpAuthorised == "1") return RedirectToPage("/MyAccount");
            }

            return Page();
        }

        [ValidateReCaptcha]
        public IActionResult OnPost()
        {
            if (HttpContext.Session.GetString("otpAuthorisation") == null)
            {
                // Declare variable
                var selectedUser = _userDb.GetUserByEmail(Email);
                var auditObject = new AuditLog();

                // If selectedUser is invalid, initialise with empty object
                selectedUser ??= new User();

                // Check whether account or IP is blocked
                // This block of code blocks the account or IP for 15 minutes after 3 attempts

                // Get records for the past 15min
                var auditLogsByUserId = _auditDb.GetLogsByUserId(selectedUser.Id)
                    .Where(o => o.Timestamp >= DateTime.Now.AddMinutes(-15) && o.LogType == "login_failed")
                    .OrderBy(o => o.Timestamp);

                var auditLogsByIp = _auditDb.GetLogsByIp(HttpContext.Connection.RemoteIpAddress.ToString())
                    .Where(o => o.Timestamp >= DateTime.Now.AddMinutes(-15) && o.LogType == "login_failed")
                    .OrderBy(o => o.Timestamp);

                if (auditLogsByUserId.Count() > 2 || auditLogsByIp.Count() > 2)
                {
                    ErrorMessage = "Too many login attempts. Try again later.";
                    return Page();
                }


                // Handle when email is invalid
                if (selectedUser.Email == null)
                    // This generates a random string of bytes as the password to prevent timing attacks
                    // Treats an invalid email address as valid and passes it through the Hashing Algorithm to produce
                    // a timing similar to valid email addresses.
                    // Uses snowflake id generator for random bytes
                    selectedUser.SetPassword(new IdGenerator(0).CreateId().ToString());


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
                    }
                    else
                    {
                        auditObject.ActorId = 0;
                        auditObject.Timestamp = DateTime.Now;
                        auditObject.LogType = "login_failed";
                        auditObject.IpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
                        _auditDb.AddLog(auditObject);
                    }

                    return Page();
                }

                // Delete any existing codes that exist
                // Delete all existing codes
                _otpDb.DestroyAllCodesByUserId(selectedUser.Id);

                // Send OTP and set in DB
                var otpObject = new OtpCode
                {
                    UserId = selectedUser.Id
                };
                otpObject.SendEmail(selectedUser);
                _otpDb.AddCode(otpObject);

                // And then add a new audit log accounting for the successful login
                auditObject.ActorId = selectedUser.Id;
                auditObject.Timestamp = DateTime.Now;
                auditObject.LogType = "login_success_password";
                auditObject.IpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
                _auditDb.AddLog(auditObject);

                // If all is well, set the session
                HttpContext.Session.SetString("user", selectedUser.ToJson());
                HttpContext.Session.SetString("otpAuthorisation", "0");

                return Page();
            }

            // From here on out, this does the OTP verification
            var otpResult = _otpDb.GetCode(OtpCode);
            var auditOtpObject = new AuditLog();

            // Handle when invalid
            if (otpResult == null)
            {
                ErrorMessage = "This code is invalid. Please re-login for a new one.";

                // Add to audit
                auditOtpObject.ActorId = new User().FromJson(HttpContext.Session.GetString("user")).Id;
                auditOtpObject.Timestamp = DateTime.Now;
                auditOtpObject.LogType = "login_failed_otp";
                auditOtpObject.IpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
                _auditDb.AddLog(auditOtpObject);

                return Page();
            }

            // Check expiry
            if (otpResult.Timestamp >= DateTime.Now.AddMinutes(-5))
                ErrorMessage = "This code has expired. Please re-login for a new one.";

            // If all is well, set the otpAuthorisation to 1
            HttpContext.Session.SetString("otpAuthorisation", "1");

            // Add to audit
            auditOtpObject.ActorId = otpResult.UserId;
            auditOtpObject.Timestamp = DateTime.Now;
            auditOtpObject.LogType = "login_success_otp";
            auditOtpObject.IpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            _auditDb.AddLog(auditOtpObject);

            // Delete all existing codes
            _otpDb.DestroyAllCodesByUserId(otpResult.UserId);

            // Check whether the user needs a password update
            var auditLogsForPasswordChanges = _auditDb.GetLogsByUserId(otpResult.UserId);
            if (!auditLogsForPasswordChanges.Any(o =>
                o.Timestamp >= DateTime.Now.AddMinutes(-20) && o.LogType == "password_changed"))
            {
                HttpContext.Session.SetString("passwordReset", "mmm yes you need password update, you insecure child");
                return RedirectToPage("/ChangePassword");
            }

            return RedirectToPage("/MyAccount");
        }
    }
}