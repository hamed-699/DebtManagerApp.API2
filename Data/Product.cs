using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtManagerApp.Data
{
	public class Product : ICloneable // --- تعديل 1: تطبيق الواجهة
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(200)]
		public string Name { get; set; }

		[StringLength(50)]
		public string? Sku { get; set; }

		public string? Description { get; set; }

		public decimal? CostPrice { get; set; }
		public decimal? SellingPrice { get; set; }

		public double? StockQuantity { get; set; }
		public double? LowStockThreshold { get; set; }

		public bool IsActive { get; set; } = true;

		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }

		public int? CategoryId { get; set; }

		[ForeignKey("CategoryId")]
		public virtual Category? Category { get; set; }

		public string? Supplier { get; set; }
		public string? Barcode { get; set; }

		[Required]
		public int OrganizationId { get; set; }

		[ForeignKey("OrganizationId")]
		public virtual Organization Organization { get; set; }

		// --- حقول المزامنة ---
		public SyncStatus SyncStatus { get; set; } = SyncStatus.Created;
		public DateTime LastModified { get; set; } = DateTime.UtcNow;

		public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();

		// --- تعديل 2: إضافة دالة النسخ ---
		public object Clone()
		{
			return this.MemberwiseClone();
		}
	}
}
