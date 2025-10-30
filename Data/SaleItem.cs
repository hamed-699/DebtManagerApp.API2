using System.ComponentModel.DataAnnotations.Schema;

namespace DebtManagerApp.Data
{
    /// <summary>
    /// يمثل منتجاً واحداً ضمن عملية بيع (سطر في الفاتورة).
    /// </summary>
    public class SaleItem
    {
        public int Id { get; set; }

        public int SaleId { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public virtual Sale Sale { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public int ProductId { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public virtual Product Product { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public double Quantity { get; set; } // الكمية المباعة

        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; } // سعر الوحدة عند البيع (قد يختلف عن السعر الحالي للمنتج)

        // --- بداية التعديل النهائي ---
        // تم إضافة [NotMapped] لإخبار نظام قاعدة البيانات بتجاهل هذا الحقل
        // لأنه حقل محسوب وليس حقلاً للتخزين.
        [NotMapped]
        // --- نهاية التعديل النهائي ---
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice => (decimal)Quantity * UnitPrice; // الإجمالي لهذا السطر
    }
}