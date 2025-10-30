using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtManagerApp.Data
{
    /// <summary>
    /// يمثل عملية بيع واحدة (فاتورة).
    /// </summary>
    public class Sale
    {
        [Key]
        public int Id { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.Now;

        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; } // المبلغ الإجمالي للفاتورة

        public bool IsPaid { get; set; } = false; // هل تم دفع الفاتورة بالكامل؟

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }


        // --- بداية الإضافة الأساسية: ربط الفاتورة بالمؤسسة ---
        [Required]
        public int OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; }
        // --- نهاية الإضافة الأساسية ---


        // --- بداية حقول المزامنة ---
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Created;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        // --- نهاية حقول المزامنة ---


        // علاقة بالمنتجات المباعة في هذه الفاتورة
        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}

