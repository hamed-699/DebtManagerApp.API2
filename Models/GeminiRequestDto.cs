namespace DebtManagerApp.API.Dtos
{
	/// <summary>
	/// هذا هو "نموذج نقل البيانات" الذي سيستخدمه تطبيق سطح المكتب
	/// لإرسال الطلب إلى الخادم.
	/// </summary>
	public class GeminiRequestDto
	{
		// يجب أن يتطابق اسم المتغير هذا "Prompt" مع الـ JSON
		// الذي يرسله العميل
		public string? Prompt { get; set; }
	}
}
