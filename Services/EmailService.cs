using DebtManagerApp.API.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DebtManagerApp.API.Services
{
	public class EmailService
	{
		private readonly SmtpSettings _smtpSettings;

		public EmailService(IOptions<SmtpSettings> smtpSettings)
		{
			_smtpSettings = smtpSettings.Value;
		}

		public async Task SendEmailAsync(string to, string subject, string body)
		{
			var message = new MailMessage
			{
				From = new MailAddress(_smtpSettings.Username, _smtpSettings.SenderName),
				Subject = subject,
				Body = body,
				IsBodyHtml = true,
			};
			message.To.Add(new MailAddress(to));

			using (var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port))
			{
				client.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
				client.EnableSsl = _smtpSettings.EnableSsl;
				await client.SendMailAsync(message);
			}
		}
	}
}
