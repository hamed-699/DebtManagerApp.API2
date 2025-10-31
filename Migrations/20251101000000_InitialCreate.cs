using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DebtManagerApp.API.Migrations
{
	/// <inheritdoc />
	public partial class InitialCreate : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Organizations",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Organizations", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "ShoppingListItems",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
					Quantity = table.Column<int>(type: "integer", nullable: false),
					Price = table.Column<decimal>(type: "decimal(18, 2)", nullable: true),
					IsPurchased = table.Column<bool>(type: "boolean", nullable: false),
					AddedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					OrganizationId = table.Column<int>(type: "integer", nullable: false),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ShoppingListItems", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Categories",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
					ParentCategoryId = table.Column<int>(type: "integer", nullable: true),
					OrganizationId = table.Column<int>(type: "integer", nullable: false),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Categories", x => x.Id);
					table.ForeignKey(
						name: "FK_Categories_Categories_ParentCategoryId",
						column: x => x.ParentCategoryId,
						principalTable: "Categories",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Categories_Organizations_OrganizationId",
						column: x => x.OrganizationId,
						principalTable: "Organizations",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Customers",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
					PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
					Address = table.Column<string>(type: "text", nullable: true),
					DebtLimit = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
					Notes = table.Column<string>(type: "text", nullable: true),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					OrganizationId = table.Column<int>(type: "integer", nullable: false),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Customers", x => x.Id);
					table.ForeignKey(
						name: "FK_Customers_Organizations_OrganizationId",
						column: x => x.OrganizationId,
						principalTable: "Organizations",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "OrganizationSettings",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					ShopName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
					LogoPath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
					CurrencyType = table.Column<int>(type: "integer", nullable: false),
					OrganizationId = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_OrganizationSettings", x => x.Id);
					table.ForeignKey(
						name: "FK_OrganizationSettings_Organizations_OrganizationId",
						column: x => x.OrganizationId,
						principalTable: "Organizations",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Users",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					Username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
					PasswordHash = table.Column<string>(type: "text", nullable: false),
					Email = table.Column<string>(type: "text", nullable: false),
					FullName = table.Column<string>(type: "text", nullable: true),
					Role = table.Column<int>(type: "integer", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					RefreshToken = table.Column<string>(type: "text", nullable: true),
					RefreshTokenExpiryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					OrganizationId = table.Column<int>(type: "integer", nullable: false),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Users", x => x.Id);
					table.ForeignKey(
						name: "FK_Users_Organizations_OrganizationId",
						column: x => x.OrganizationId,
						principalTable: "Organizations",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Products",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
					Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
					Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
					CostPrice = table.Column<decimal>(type: "decimal(18, 2)", nullable: true),
					SellingPrice = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
					StockQuantity = table.Column<int>(type: "integer", nullable: false),
					CategoryId = table.Column<int>(type: "integer", nullable: true),
					OrganizationId = table.Column<int>(type: "integer", nullable: false),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Products", x => x.Id);
					table.ForeignKey(
						name: "FK_Products_Categories_CategoryId",
						column: x => x.CategoryId,
						principalTable: "Categories",
						principalColumn: "Id",
						onDelete: ReferentialAction.SetNull);
					table.ForeignKey(
						name: "FK_Products_Organizations_OrganizationId",
						column: x => x.OrganizationId,
						principalTable: "Organizations",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Sales",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					SaleDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					TotalAmount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
					Notes = table.Column<string>(type: "text", nullable: true),
					CustomerId = table.Column<int>(type: "integer", nullable: true),
					OrganizationId = table.Column<int>(type: "integer", nullable: false),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Sales", x => x.Id);
					table.ForeignKey(
						name: "FK_Sales_Customers_CustomerId",
						column: x => x.CustomerId,
						principalTable: "Customers",
						principalColumn: "Id");
					table.ForeignKey(
						name: "FK_Sales_Organizations_OrganizationId",
						column: x => x.OrganizationId,
						principalTable: "Organizations",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Transactions",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					Amount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
					Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					Type = table.Column<int>(type: "integer", nullable: false),
					Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
					CustomerId = table.Column<int>(type: "integer", nullable: true),
					ProductId = table.Column<int>(type: "integer", nullable: true),
					OrganizationId = table.Column<int>(type: "integer", nullable: false),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Transactions", x => x.Id);
					table.ForeignKey(
						name: "FK_Transactions_Customers_CustomerId",
						column: x => x.CustomerId,
						principalTable: "Customers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_Transactions_Organizations_OrganizationId",
						column: x => x.OrganizationId,
						principalTable: "Organizations",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_Transactions_Products_ProductId",
						column: x => x.ProductId,
						principalTable: "Products",
						principalColumn: "Id",
						onDelete: ReferentialAction.SetNull);
				});

			migrationBuilder.CreateTable(
				name: "SaleItems",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					Quantity = table.Column<int>(type: "integer", nullable: false),
					UnitPrice = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
					SaleId = table.Column<int>(type: "integer", nullable: false),
					ProductId = table.Column<int>(type: "integer", nullable: false),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SaleItems", x => x.Id);
					table.ForeignKey(
						name: "FK_SaleItems_Products_ProductId",
						column: x => x.ProductId,
						principalTable: "Products",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_SaleItems_Sales_SaleId",
						column: x => x.SaleId,
						principalTable: "Sales",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Categories_OrganizationId",
				table: "Categories",
				column: "OrganizationId");

			migrationBuilder.CreateIndex(
				name: "IX_Categories_ParentCategoryId",
				table: "Categories",
				column: "ParentCategoryId");

			migrationBuilder.CreateIndex(
				name: "IX_Customers_OrganizationId",
				table: "Customers",
				column: "OrganizationId");

			migrationBuilder.CreateIndex(
				name: "IX_OrganizationSettings_OrganizationId",
				table: "OrganizationSettings",
				column: "OrganizationId",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Products_CategoryId",
				table: "Products",
				column: "CategoryId");

			migrationBuilder.CreateIndex(
				name: "IX_Products_OrganizationId",
				table: "Products",
				column: "OrganizationId");

			migrationBuilder.CreateIndex(
				name: "IX_SaleItems_ProductId",
				table: "SaleItems",
				column: "ProductId");

			migrationBuilder.CreateIndex(
				name: "IX_SaleItems_SaleId",
				table: "SaleItems",
				column: "SaleId");

			migrationBuilder.CreateIndex(
				name: "IX_Sales_CustomerId",
				table: "Sales",
				column: "CustomerId");

			migrationBuilder.CreateIndex(
				name: "IX_Sales_OrganizationId",
				table: "Sales",
				column: "OrganizationId");

			migrationBuilder.CreateIndex(
				name: "IX_Transactions_CustomerId",
				table: "Transactions",
				column: "CustomerId");

			migrationBuilder.CreateIndex(
				name: "IX_Transactions_OrganizationId",
				table: "Transactions",
				column: "OrganizationId");

			migrationBuilder.CreateIndex(
				name: "IX_Transactions_ProductId",
				table: "Transactions",
				column: "ProductId");

			migrationBuilder.CreateIndex(
				name: "IX_Users_OrganizationId",
				table: "Users",
				column: "OrganizationId");

			migrationBuilder.CreateIndex(
				name: "IX_Users_Username",
				table: "Users",
				column: "Username",
				unique: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "OrganizationSettings");

			migrationBuilder.DropTable(
				name: "SaleItems");

			migrationBuilder.DropTable(
				name: "ShoppingListItems");

			migrationBuilder.DropTable(
				name: "Transactions");

			migrationBuilder.DropTable(
				name: "Users");

			migrationBuilder.DropTable(
				name: "Sales");

			migrationBuilder.DropTable(
				name: "Products");

			migrationBuilder.DropTable(
				name: "Customers");

			migrationBuilder.DropTable(
				name: "Categories");

			migrationBuilder.DropTable(
				name: "Organizations");
		}
	}
}
