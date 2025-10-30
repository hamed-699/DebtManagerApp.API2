using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtManagerApp.Data
{
	public class Transaction : ICloneable // --- تعديل 1: تطبيق الواجهة
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int CustomerId { get; set; }

		[ForeignKey("CustomerId")]
		public virtual Customer Customer { get; set; }

		[Required]
		public int OrganizationId { get; set; }

		[ForeignKey("OrganizationId")]
		public virtual Organization Organization { get; set; }

		[Required]
		public decimal Amount { get; set; }

		[Required]
		public TransactionType Type { get; set; }

		public DateTime Date { get; set; } = DateTime.Now;

		public string? Notes { get; set; }

		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }

		[Required]
		public CurrencyType Currency { get; set; }

		public string? ReceiptId { get; set; }

		public int? ProductId { get; set; }

		[ForeignKey("ProductId")]
		public virtual Product? Product { get; set; }

		public double? Quantity { get; set; }

		// --- حقول المزامنة ---
		public SyncStatus SyncStatus { get; set; } = SyncStatus.Created;
		public DateTime LastModified { get; set; } = DateTime.UtcNow;

		// --- تعديل 2: إضافة دالة النسخ ---
		public object Clone()
		{
			return this.MemberwiseClone();
		}
	}
}
