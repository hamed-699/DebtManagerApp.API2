using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtManagerApp.Data
{
	public class Customer : ICloneable // --- تعديل 1: تطبيق الواجهة
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[MaxLength(100)]
		public string Name { get; set; }

		[MaxLength(20)]
		public string? PhoneNumber { get; set; }

		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }
		public decimal? DebtLimit { get; set; }

		[Required]
		public int OrganizationId { get; set; }

		[ForeignKey("OrganizationId")]
		public virtual Organization Organization { get; set; }

		public DateTime DateAdded { get; set; } = DateTime.Now;
		public int InactivityPeriodDays { get; set; } = 30;

		// --- حقول المزامنة ---
		public SyncStatus SyncStatus { get; set; } = SyncStatus.Created;
		public DateTime LastModified { get; set; } = DateTime.UtcNow;

		public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
		public ICollection<Sale> Sales { get; set; } = new List<Sale>();

		[NotMapped]
		public AnalyticsCustomerStatus AnalyticsStatus { get; set; }

		public AnalyticsCustomerStatus Status { get; internal set; }

		// --- تعديل 2: إضافة دالة النسخ ---
		public object Clone()
		{
			return this.MemberwiseClone();
		}
	}
}
