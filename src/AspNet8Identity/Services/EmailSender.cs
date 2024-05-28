using AspNet8Identity.Data;
using AspNet8Identity.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;

namespace AspNet8Identity.Services
{
    internal sealed class EmailSender(IOptions<EmailSettings> emailSettings) 
        : IEmailSender<ApplicationUser>
    {
        private readonly EmailSettings _emailSettings = emailSettings.Value;
        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            var client = new SendGridClient(_emailSettings.ApiKey);
            var plainTextContent = $"Please confirm your account by clicking this link: {confirmationLink}";
            var htmlContent = $"Please confirm your account by clicking <a href=\"{confirmationLink}\">here</a>.";
            var message = MailHelper.CreateSingleEmail(
                new EmailAddress(_emailSettings.FromAddress, _emailSettings.FromName),
                new EmailAddress(email),
                "Confirm your email",
                plainTextContent,
                htmlContent);
            var response = await client.SendEmailAsync(message);

            Log.Information($"Email sent with status: {response.StatusCode}");
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            var client = new SendGridClient(_emailSettings.ApiKey);
            var plainTextContent = $"Reset your password by clicking this link: {resetLink}";
            var htmlContent = $"Reset your password by clicking <a href=\"{resetLink}\">here</a>.";
            var message = MailHelper.CreateSingleEmail(
                new EmailAddress(_emailSettings.FromAddress, _emailSettings.FromName),
                new EmailAddress(email),
                "Reset your password",
                plainTextContent,
                htmlContent);
            var response = await client.SendEmailAsync(message);

            Log.Information($"Email sent with status: {response.StatusCode}");
        }

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            throw new NotImplementedException();
        }
    }
}
