using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Required for ForeignKey attribute

namespace DebtManagerApp.Data
{
	public class OrganizationSettings
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int OrganizationId { get; set; } // Foreign key to Organization

		public string? ShopName { get; set; }
		public string? LogoPath { get; set; }
		public int OverdueDays { get; set; } = 30; // Default value
		public string? Theme { get; set; } = "Light"; // Default value

		// --- بداية الإضافة: خاصية التنقل العكسية ---
		[ForeignKey("OrganizationId")]
		public virtual Organization Organization { get; set; } = null!; // Organization is required
																		// --- نهاية الإضافة ---

		// يمكن إضافة إعدادات أخرى خاصة بالمؤسسة هنا
	}
}
