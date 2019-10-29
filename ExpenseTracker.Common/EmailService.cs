using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ExpenseTracker.Common
{
    /// <summary>
    /// Helps to send out required email messages
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string sendGridKey;
        private readonly string senderAddress;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            sendGridKey = _configuration["appSettings:SendGridKey"];
            senderAddress = _configuration["appSettings:FromEmailAddress"];
        }
        
        public async Task<bool> ConfirmEmail(string recipient,string cTokenLink)
        {
            string subject = "Confirm your email";
            string message = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(cTokenLink)}'>clicking here</a>.";

           return await SendMessage(subject, message, recipient);
        }

        public async Task<bool> ResetPassword(string recipient, string resetPasswordLink)
        {
            string subject = "Reset Password";
            string message = $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(resetPasswordLink)}'>clicking here</a>.";
            
            return await SendMessage(subject, message, recipient);
        }

        public async Task<bool> BudgetExceeded(string recipient,string message,string category)
        {
            string subject = $"Budget for {category} Exceeded";
            //string message = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(cTokenLink)}'>clicking here</a>.";

            return await SendMessage(subject, message, recipient);
        }

        public async Task<bool> SendReminderEmail(string recipient, string message)
        {
            string subject = "Remember to Record your Expenses";

            return await SendMessage(subject, message, recipient);
        }

        /// <summary>
        /// Sends an email message
        /// </summary>
        /// <param name="subject">The subject of the email</param>
        /// <param name="message">The content of the email message</param>
        /// <param name="recipient">The recipient of the email message</param>
        /// <returns>bool whether the message was successfully sent or not</returns>
        public async Task<bool> SendMessage(string subject, string message, string recipient)
        {
            try
            {
                string sub = subject;
                //string to = recipient;
                string msgTxt = message;
                var client = new SendGridClient(sendGridKey);
                var sender = new EmailAddress(senderAddress, "Expense Tracker");
                var to = new EmailAddress(recipient,"ET");

                var plainTextContent = "";

                var htmlContent = message;

                var msg = MailHelper.CreateSingleEmail(sender, to, sub, plainTextContent, htmlContent);

                //var response = await client.SendEmailAsync(msg);

                var response = await client.RequestAsync(method: SendGridClient.Method.POST,
                                                 requestBody: msg.Serialize(),
                                                 urlPath: "mail/send");
                var statusCode = response.StatusCode;
                var headers = response.Headers.ToString();
                var body = await response.Body.ReadAsStringAsync();
                return response.StatusCode == HttpStatusCode.Accepted;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Email not sent successfully!");
                Debug.WriteLine($"{ex.Message}");
                return false;
            }
        }
    }
}
