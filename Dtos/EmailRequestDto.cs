using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DebtManagerApp.API.Dtos
{
	public class EmailRequestDto
	{
		[Required(ErrorMessage = "حقل البريد الإلكتروني للمستلم (to) مطلوب.")]
		[EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
		[JsonPropertyName("to")]
		public string ToEmail { get; set; }

		[Required(ErrorMessage = "حقل الموضوع (subject) مطلوب.")]
		[JsonPropertyName("subject")]
		public string Subject { get; set; }

		[Required(ErrorMessage = "حقل محتوى الرسالة (body) مطلوب.")]
		[JsonPropertyName("body")]
		public string Body { get; set; }
	}
}

