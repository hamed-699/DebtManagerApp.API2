using DebtManagerApp.API.Services;
using Microsoft.EntityFrameworkCore;
using DebtManagerApp.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using DebtManagerApp.Data;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Npgsql;
// --- !!! --- تعديل: تم حذف "using EFCore.NamingConventions;" --- !!! ---

internal class Program
{
	// --- (تم الإبقاء عليه) تغيير الدالة الرئيسية لتدعم التشغيل غير المتزامن (async) ---
	private static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// --- إعدادات Supabase (PostgreSQL) ---
		var connectionString = builder.Configuration["SUPABASE_CONNECTION_STRING"];

		if (string.IsNullOrEmpty(connectionString))
		{
			Console.WriteLine("[FATAL ERROR] No connection string found! (SUPABASE_CONNECTION_STRING)");
			throw new InvalidOperationException("Database connection string is missing.");
		}
		else
		{
			Console.WriteLine($"[DEBUG] Connection string found, starting with: {connectionString.Substring(0, Math.Min(connectionString.Length, 10))}...");
		}

		// --- (تم الإبقاء عليه) هذا هو إصلاحك لمشكلة EndOfStreamException ---
		if (!connectionString.Trim().EndsWith(";"))
		{
			connectionString += ";";
		}
		connectionString += "Pooling=false;";
		Console.WriteLine("[DEBUG] Connection string modified to disable pooling.");
		// --- !!! --- نهاية الإصلاح --- !!! ---


		// --- تهيئة EF Core (باستخدام "العامل الذكي") ---
		builder.Services.AddDbContext<DatabaseContext>(options =>
			options.UseNpgsql(connectionString, // <-- استخدام سلسلة الاتصال المُعدلة
				npgsqlOptions =>
				{
					npgsqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
					npgsqlOptions.CommandTimeout(60); // (تم الإبقاء عليه)
				})
		// --- !!! --- تعديل: تم حذف ".UseSnakeCaseNamingConvention()" --- !!! ---
		// .UseSnakeCaseNamingConvention()
		);

		// --- إعدادات JWT ---
		// (لا تغيير، تم الإبقاء على الكود الخاص بك كما هو)
		var jwtSettings = builder.Configuration.GetSection("Jwt");
		var jwtKey = jwtSettings["Key"]
			?? builder.Configuration["JWT_KEY"];

		if (string.IsNullOrEmpty(jwtKey))
		{
			Console.WriteLine("[FATAL ERROR] JWT Key is missing. Cannot configure authentication.");
			throw new InvalidOperationException("JWT Key is missing in configuration (Jwt:Key or JWT_KEY).");
		}

		var key = Encoding.ASCII.GetBytes(jwtKey);

		builder.Services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(options =>
		{
			options.RequireHttpsMetadata = false;
			options.SaveToken = true;
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = true,
				ValidIssuer = jwtSettings["Issuer"] ?? builder.Configuration["JWT_ISSUER"],
				ValidateAudience = true,
				ValidAudience = jwtSettings["Audience"] ?? builder.Configuration["JWT_AUDIENCE"],
				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero
			};
		});

		// --- إعدادات البريد الإلكتروني ---
		builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtMtpSettings"));
		builder.Services.AddTransient<EmailService>();

		// --- الخدمات ---
		builder.Services.AddControllers()
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
			});

		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo { Title = "DebtManagerApp.API", Version = "v1" });
			c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
				Name = "Authorization",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.ApiKey,
				Scheme = "Bearer"
			});
			c.AddSecurityRequirement(new OpenApiSecurityRequirement()
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						},
						Scheme = "oauth2",
						Name = "Bearer",
						In = ParameterLocation.Header,
					},
					new List<string>()
				}
			});
		});

		// --- سياسة CORS ---
		builder.Services.AddCors(options =>
		{
			options.AddPolicy("AllowAll", policy => // (هذا هو الاسم الصحيح "AllowAll")
			{
				policy.AllowAnyOrigin()
					  .AllowAnyMethod()
					  .AllowAnyHeader();
			});
		});

		var app = builder.Build();

		// --- (تم الإبقاء عليه) هذا هو الكود الذي ينشئ الجداول ---
		using (var scope = app.Services.CreateScope())
		{
			var services = scope.ServiceProvider;
			var logger = services.GetRequiredService<ILogger<Program>>();

			try
			{
				logger.LogInformation("Attempting to apply database migrations (Async)...");
				var dbContext = services.GetRequiredService<DatabaseContext>();

				// --- (تم الإبقاء عليه) استخدام النسخة غير المتزامنة (Async) لتجنب الـ Timeout ---
				await dbContext.Database.MigrateAsync();

				logger.LogInformation("[SUCCESS] Database connection verified and tables migrated.");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "An error occurred while migrating the database.");
				// لا توقف التطبيق، فقط سجل الخطأ
			}
		}
		// --- !!! --- نهاية قسم إنشاء الجداول --- !!! ---


		// --- تفعيل الميدل وير ---
		app.UseCors("AllowAll");
		app.UseSwagger();
		app.UseSwaggerUI();
		app.UseRouting();
		app.UseAuthentication();
		app.UseAuthorization();
		app.MapControllers();

		var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
		app.Run($"http://0.0.0.0:{port}");
	}
}