using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DebtManagerApp.Data
{
	public class Organization
	{
		public Organization()
		{
			Users = new HashSet<User>();
			Customers = new HashSet<Customer>();
			Products = new HashSet<Product>();
			Categories = new HashSet<Category>();
			Sales = new HashSet<Sale>();
			Transactions = new HashSet<Transaction>();
		}

		[Key]
		public int Id { get; set; }

		[Required]
		[MaxLength(256)]
		public string Name { get; set; } = string.Empty; // تمت إضافة قيمة افتراضية

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Navigation properties
		public virtual ICollection<User> Users { get; set; }
		public virtual ICollection<Customer> Customers { get; set; }
		public virtual ICollection<Product> Products { get; set; }
		public virtual ICollection<Category> Categories { get; set; }
		public virtual ICollection<Sale> Sales { get; set; }
		public virtual ICollection<Transaction> Transactions { get; set; }

		// --- بداية الإضافة: خاصية التنقل لعلاقة واحد-لواحد ---
		public virtual OrganizationSettings? Settings { get; set; }
		// --- نهاية الإضافة ---
	}
}
