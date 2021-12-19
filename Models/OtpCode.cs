using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SITConnect.Models
{
    public class OtpCode
    {
        private static readonly Random Rnd = new Random();

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int Code { get; set; } = Rnd.Next(100000, 999999);

        public long UserId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public async Task SendEmail(User userObject)
        {
            var apiKey = ConfigurationManager.AppSetting["SendgridAPIKey"];
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage
            {
                From = new EmailAddress("noreply@tanukihq.com", "TanukiHQ"),
                Subject = "SIT Connect - Your One-time-Password",
                // PlainTextContent = "and easy to do anywhere, even with C#",
                HtmlContent =
                    $"<p>Hello! You may use <code>{Code.ToString()}</code> to login to your account.<br>Thank you.</p>"
            };

            msg.AddTo(new EmailAddress(userObject.Email, $"{userObject.FirstName} {userObject.LastName}"));
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
        }
    }

    internal static class ConfigurationManager
    {
        static ConfigurationManager()
        {
            AppSetting = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }

        public static IConfiguration AppSetting { get; }
    }
}