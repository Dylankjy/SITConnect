using System.Text.RegularExpressions;

namespace SITConnect.Pages
{
    public class PasswordFunc
    {
        public string GetPasswordStrength(string password)
        {
            // Ensure that password is not null
            if (password == null)
                // If it is, return error message to view
                return "Please enter a password";

            var score = 0;

            if (password.Length >= 8) score++;

            if (Regex.IsMatch(password, "[a-z]")) score++;

            if (Regex.IsMatch(password, "[A-Z]")) score++;

            if (Regex.IsMatch(password, "[0-9]")) score++;

            if (Regex.IsMatch(password, "[^a-zA-Z0-9]+")) score++;

            var status = score switch
            {
                1 => "Very Weak",
                2 => "Weak",
                3 => "Medium",
                4 => "Strong",
                5 => "Excellent",
                _ => null
            };

            // Exit point
            return status;
        }
    }
}