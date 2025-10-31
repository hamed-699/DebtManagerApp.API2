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
using Npgsql; // 👈 مهم جداً

var builder = WebApplication.CreateBuilder(args);

// --- إعدادات Supabase (PostgreSQL) ---
// --- أبسط طريقة: اقرأ المفتاح واستخدمه كما هو ---
var connectionString = builder.Configuration["SUPABASE_CONNECTION_STRING"];

if (string.IsNullOrEmpty(connectionString))
{
	Console.WriteLine("[FATAL ERROR] No connection string found! (SUPABASE_CONNECTION_STRING)");
	throw new InvalidOperationException("Database connection string is missing.");
}
else
{
	// نطبع أول 10 حروف للتأكد أننا نقرأه (بدون طباعة كلمة السر)
	Console.WriteLine($"[DEBUG] Connection string found, starting with: {connectionString.Substring(0, Math.Min(connectionString.Length, 10))}...");
}

// --- تهيئة EF Core ---
// (سيتم استخدام النص "كما هو" مباشرة)
// --- (الرجوع إلى الطريقة البسيطة) ---
builder.Services.AddDbContext<DatabaseContext>(options =>
	options.UseNpgsql(connectionString)
); // <-- قمنا بإزالة السطر الخاص بـ Migrations

// --- إعدادات JWT ---
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
	// --- (تم تصحيح الخطأ المطبعي هنا) ---
	options.RequireHttpsMetadata = false; // <-- كانت HttspMetadata
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
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
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
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

var app = builder.Build();

// --- إنشاء الجداول (باستخدام الطريقة "الكسولة" الأصلية) ---
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		var dbContext = services.GetRequiredService<DatabaseContext>();

		// ------------------ التغيير الوحيد هنا ------------------
		dbContext.Database.EnsureCreated(); // <-- إرجاع الأمر "الكسول"
											// dbContext.Database.Migrate(); // <-- حذف الأمر "الذكي" الخاطئ
											// -----------------------------------------------------------

		Console.WriteLine("[SUCCESS] Database connection verified and tables ensured.");
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "An error occurred while ensuring the database was created.");
		// سنسمح للتطبيق بالاستمرار لكي نرى الأخطاء الأخرى إذا وجدت
	}
}

// --- تفعيل الميدل وير ---
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://*:{port}");

// 🔄 دالة التحويل (تم حذفها لأننا سنستخدم النص البسيط)

