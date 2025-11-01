using DebtManagerApp.API.Models;
using DebtManagerApp.API.Services;
// !!! --- هذا هو الإصلاح الصحيح --- !!!
// "using DebtManagerApp.Data;"
// هذا هو العنوان الصحيح لـ "خريطة البناء"
// (DatabaseContext.cs) الذي أرسلته أنت
using DebtManagerApp.Data;
using Microsoft;//www.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
// !!! --- نهاية الإصلاح --- !!!
using Microsoft.OpenApi.Models;
using Npgsql;
using System.Text;
using System.Text.Json.Serialization;

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

// --- تهيئة EF Core (باستخدام "العامل الذكي") ---
builder.Services.AddDbContext<DatabaseContext>(options =>
	options.UseNpgsql(connectionString,
		npgsqlOptions =>
		{
			npgsqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
			// زيادة "الصبر" (Timeout) إلى 90 ثانية لإعطاء
			// قاعدة البيانات المجانية وقتاً "للاستيقاظ"
			npgsqlOptions.CommandTimeout(90);
		}
	)
);

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
	// !!! --- هذا هو التعديل الوحيد للاختبار --- !!!
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "DebtManagerApp.API - TEST V9", Version = "v1" });
	// !!! --- نهاية التعديل --- !!!
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

// --- تطبيق "تعليمات البناء" (Migrations) ---
// "EnsureCreated" سيتأكد من بناء جميع الجداول من الصفر
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var logger = services.GetRequiredService<ILogger<Program>>();

	logger.LogInformation("Attempting to ensure database schema is created...");

	// الآن هذا المتغير "dbContext" سيشير إلى قاعدة البيانات الصحيحة
	// التي تحتوي على "خرائط" المستخدمين والعملاء
	var dbContext = services.GetRequiredService<DatabaseContext>();

	// سيقوم هذا الأمر بإنشاء الجداول مباشرة إذا لم تكن موجودة
	dbContext.Database.EnsureCreated();

	logger.LogInformation("[SUCCESS] Database schema created/verified.");
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

