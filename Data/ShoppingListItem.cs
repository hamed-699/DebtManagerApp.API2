using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtManagerApp.Data
{
    public class ShoppingListItem
    {
        public int Id { get; set; }

        // التأكد من أن هذه الخاصية تقبل القيمة الفارغة
        public int? CustomerId { get; set; }

        public int? ProductId { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string Description { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public double Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
        public Guid ReceiptId { get; set; }

        [NotMapped]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Product Product { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}
