using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtManagerApp.Data
{
	public class Category : ICloneable // --- تعديل 1: تطبيق الواجهة
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; } = string.Empty;

		public string? Description { get; set; }

		public int? ParentCategoryId { get; set; }

		[ForeignKey("ParentCategoryId")]
		public virtual Category? ParentCategory { get; set; }

		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }

		[Required]
		public int OrganizationId { get; set; }

		[ForeignKey("OrganizationId")]
		public virtual Organization Organization { get; set; }

		// --- حقول المزامنة ---
		public SyncStatus SyncStatus { get; set; } = SyncStatus.Created;
		public DateTime LastModified { get; set; } = DateTime.UtcNow;

		public virtual ICollection<Product> Products { get; set; } = new List<Product>();

		// --- تعديل 2: إضافة دالة النسخ ---
		public object Clone()
		{
			return this.MemberwiseClone();
		}
	}
}
