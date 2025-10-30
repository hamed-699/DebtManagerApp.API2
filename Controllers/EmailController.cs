using DebtManagerApp.API.Dtos;
using DebtManagerApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // إضافة مساحة الاسم الخاصة بالـ Logger
using System;
using System.Threading.Tasks;

namespace DebtManagerApp.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class EmailController : ControllerBase
	{
		private readonly EmailService _emailService;
		private readonly ILogger<EmailController> _logger; // إضافة حقل لتخزين الـ Logger

		// تحديث المُنشئ ليقوم باستلام خدمة الـ Logger
		public EmailController(EmailService emailService, ILogger<EmailController> logger)
		{
			_emailService = emailService;
			_logger = logger;
		}

		[HttpPost("send")]
		public async Task<IActionResult> SendEmail([FromBody] EmailRequestDto emailRequest)
		{
			// التحقق من أن النموذج صالح قبل محاولة الإرسال
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				await _emailService.SendEmailAsync(emailRequest.ToEmail, emailRequest.Subject, emailRequest.Body);
				return Ok(new { message = "Email sent successfully." });
			}
			catch (Exception ex) // التقاط تفاصيل الخطأ
			{
				// تسجيل الخطأ الكامل في سجلات الخادم للمراجعة لاحقاً
				_logger.LogError(ex, "An exception occurred while trying to send an email.");

				// إرجاع رسالة خطأ أكثر تفصيلاً للمساعدة في التشخيص
				return StatusCode(500, new
				{
					message = "An error occurred while sending the email.",
					error = "Failure sending mail.",
					detailedError = ex.Message, // رسالة الخطأ الرئيسية
					innerException = ex.InnerException?.Message // الرسالة الداخلية (غالباً ما تحتوي على السبب الحقيقي)
				});
			}
		}
	}
}

