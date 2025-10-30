using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtManagerApp.Data
{
    /// <summary>
    /// يمثل هذا الكلاس مادة واحدة تم شراؤها.
    /// </summary>
    public class MaterialItem
    {
        public int Id { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string Name { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public double WholesalePrice { get; set; } // سعر الجملة للقطعة الواحدة
        public double Quantity { get; set; } // الكمية
        public double ProfitMargin { get; set; } // نسبة الربح (مثال: 0.2 for 20%)
        public DateTime DateAdded { get; set; }

        /// <summary>
        /// خاصية محسوبة لسعر البيع للمستهلك. لا يتم تخزينها في قاعدة البيانات.
        /// </summary>
        [NotMapped]
        public double ConsumerPrice => WholesalePrice * (1 + ProfitMargin);

        /// <summary>
        /// خاصية محسوبة للتكلفة الإجمالية لهذه المادة. لا يتم تخزينها في قاعدة البيانات.
        /// </summary>
        [NotMapped]
        public double TotalItemCost => WholesalePrice * Quantity;
    }
}
