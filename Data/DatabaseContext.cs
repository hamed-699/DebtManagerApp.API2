using Microsoft.EntityFrameworkCore;

namespace DebtManagerApp.Data
{
	public class DatabaseContext : DbContext
	{
		public DbSet<OrganizationSettings> OrganizationSettings { get; set; } = null!;
		public DbSet<User> Users { get; set; } = null!;
		public DbSet<Customer> Customers { get; set; } = null!;
		public DbSet<Transaction> Transactions { get; set; } = null!;
		public DbSet<ShoppingListItem> ShoppingListItems { get; set; } = null!;
		public DbSet<Product> Products { get; set; } = null!;
		public DbSet<Sale> Sales { get; set; } = null!;
		public DbSet<SaleItem> SaleItems { get; set; } = null!;
		public DbSet<Category> Categories { get; set; } = null!;
		public DbSet<Organization> Organizations { get; set; } = null!;


		public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Organization>()
				.HasOne(o => o.Settings)
				.WithOne(s => s.Organization)
				.HasForeignKey<OrganizationSettings>(s => s.OrganizationId)
				.OnDelete(DeleteBehavior.Cascade);


			modelBuilder.Entity<User>(entity =>
			{
				entity.Property(u => u.Username).HasMaxLength(256);
				entity.HasIndex(u => u.Username).IsUnique();
				entity.Property(u => u.PasswordHash).IsRequired();
			});


			modelBuilder.Entity<Customer>(entity =>
			{
				entity.Property(c => c.DebtLimit).HasColumnType("decimal(18, 2)");
				entity.Property(c => c.Name).HasMaxLength(256).IsRequired();
				entity.Property(c => c.PhoneNumber).HasMaxLength(50);
				// Address property configuration was removed as it doesn't exist
			});

			modelBuilder.Entity<Transaction>(entity =>
			{
				entity.Property(t => t.Amount).HasColumnType("decimal(18, 2)");
				entity.Property(t => t.Notes).HasMaxLength(1000);
			});

			modelBuilder.Entity<ShoppingListItem>(entity =>
			{
				entity.Property(s => s.Price).HasColumnType("decimal(18, 2)");
				// --- بداية التعديل: تغيير Name إلى Description ---
				entity.Property(s => s.Description).HasMaxLength(500).IsRequired(); // كان Name
																					// --- نهاية التعديل ---
			});

			modelBuilder.Entity<Product>(entity =>
			{
				entity.Property(p => p.CostPrice).HasColumnType("decimal(18, 2)").IsRequired(false);
				entity.Property(p => p.SellingPrice).HasColumnType("decimal(18, 2)").IsRequired();
				entity.Property(p => p.Name).HasMaxLength(256).IsRequired();
				entity.Property(p => p.Sku).HasMaxLength(100);
				entity.Property(p => p.Description).HasMaxLength(2000);
			});

			modelBuilder.Entity<Sale>(entity =>
			{
				entity.Property(s => s.TotalAmount).HasColumnType("decimal(18, 2)");
			});

			modelBuilder.Entity<SaleItem>(entity =>
			{
				entity.Property(si => si.UnitPrice).HasColumnType("decimal(18, 2)");
				entity.Property(si => si.Quantity).IsRequired();
			});

			modelBuilder.Entity<Category>(entity =>
			{
				entity.Property(c => c.Name).HasMaxLength(256).IsRequired();
			});

			modelBuilder.Entity<Organization>(entity =>
			{
				entity.Property(o => o.Name).HasMaxLength(256).IsRequired();
			});

			modelBuilder.Entity<OrganizationSettings>(entity =>
			{
				entity.Property(os => os.ShopName).HasMaxLength(256);
				entity.Property(os => os.LogoPath).HasMaxLength(1024);
			});


			// --- العلاقات ---
			modelBuilder.Entity<SaleItem>()
				.HasOne(si => si.Sale)
				.WithMany(s => s.SaleItems)
				.HasForeignKey(si => si.SaleId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<SaleItem>()
				.HasOne(si => si.Product)
				.WithMany(p => p.SaleItems)
				.HasForeignKey(si => si.ProductId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Product>()
				.HasOne(p => p.Category)
				.WithMany(c => c.Products)
				.HasForeignKey(p => p.CategoryId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.SetNull);

			modelBuilder.Entity<Category>()
				.HasOne(c => c.ParentCategory)
				.WithMany() // EF Core سيستنتج العلاقة العكسية
				.HasForeignKey(c => c.ParentCategoryId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<User>()
				.HasOne(u => u.Organization)
				.WithMany(o => o.Users)
				.HasForeignKey(u => u.OrganizationId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Customer>()
				.HasOne(c => c.Organization)
				.WithMany(o => o.Customers)
				.HasForeignKey(c => c.OrganizationId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Product>()
			   .HasOne(p => p.Organization)
			   .WithMany(o => o.Products)
			   .HasForeignKey(p => p.OrganizationId)
			   .OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Category>()
			   .HasOne(cat => cat.Organization)
			   .WithMany(o => o.Categories)
			   .HasForeignKey(cat => cat.OrganizationId)
			   .OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Sale>()
			   .HasOne(s => s.Organization)
			   .WithMany(o => o.Sales)
			   .HasForeignKey(s => s.OrganizationId)
			   .OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Transaction>()
			   .HasOne(t => t.Organization)
			   .WithMany(o => o.Transactions)
			   .HasForeignKey(t => t.OrganizationId)
			   .OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Transaction>()
			   .HasOne(t => t.Customer)
			   .WithMany(c => c.Transactions)
			   .HasForeignKey(t => t.CustomerId)
			   .OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Transaction>()
			   .HasOne(t => t.Product)
			   .WithMany()
			   .HasForeignKey(t => t.ProductId)
			   .IsRequired(false)
			   .OnDelete(DeleteBehavior.SetNull);

		}
	}
}

