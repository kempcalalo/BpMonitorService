using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace BpMonitorService
{
    public class Mailer
    {
        private string apiKey;
        public Mailer()
        {
            apiKey = ConfigurationManager.AppSettings["SendGridApiKey"];
        }

        public bool SendEmail(string fromEmail, string fromEmailName,string subject, string toEmail, string toEmailName,string plainTextContent,
            string htmlContent = null)
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromEmailName);
            var to = new EmailAddress(toEmail, toEmailName);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = client.SendEmailAsync(msg).Result;

            return response.StatusCode == HttpStatusCode.Accepted;
        }
    }
}
