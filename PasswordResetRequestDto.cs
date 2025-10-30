namespace DebtManagerApp.API.Dtos
{
	/// <summary>
	/// (ملف جديد)
	/// هذا هو "الصندوق" الصحيح الذي يرسله العميل عند طلب إعادة تعيين كلمة المرور.
	/// </summary>
	public class PasswordResetRequestDto
	{
		public string Username { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Token { get; set; } = string.Empty;
		public string ShopName { get; set; } = string.Empty;
	}
}
