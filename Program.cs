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
var connectionString = builder.Configuration["SUPABASE_CONNECTION_STRING"];

if (string.IsNullOrEmpty(connectionString))
{
	connectionString = builder.Configuration["DATABASE_URL"];
}

// ✅ هنا نتحقق إن كانت الصيغة تبدأ بـ postgres:// ثم نحولها
if (!string.IsNullOrEmpty(connectionString))
{
	// طباعة السجل (اختياري لكن مفيد)
	Console.WriteLine($"[DEBUG] Original connection string found.");
	if (connectionString.StartsWith("postgres://"))
	{
		connectionString = ConvertSupabaseUrlToNpgsql(connectionString);
		Console.WriteLine($"[DEBUG] Connection string converted to Npgsql format.");
	}
}
else
{
	Console.WriteLine("[ERROR] No connection string found! (SUPABASE_CONNECTION_STRING or DATABASE_URL)");
}

// --- تهيئة EF Core ---
// (نتأكد أن نص الاتصال ليس فارغاً قبل استخدامه)
if (string.IsNullOrEmpty(connectionString))
{
	Console.WriteLine("[FATAL ERROR] Connection string is null. Cannot configure DbContext.");
	// هذا سيمنع التطبيق من البدء إذا لم يجد المفتاح
	throw new InvalidOperationException("Database connection string is missing.");
}
builder.Services.AddDbContext<DatabaseContext>(options =>
	options.UseNpgsql(connectionString));

// --- إعدادات JWT ---
var jwtSettings = builder.Configuration.GetSection("Jwt");

// !!! --- هذا هو السطر الذي يقرأ المفتاح السري --- !!!
var jwtKey = jwtSettings["Key"]
	?? builder.Configuration["JWT_KEY"]; // <-- سنضيف هذا كاحتياط

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
		IssuerSigningKey = new SymmetricSecurityKey(key), // <-- هنا يتم استخدامه
		ValidateIssuer = true,
		ValidIssuer = jwtSettings["Issuer"] ?? builder.Configuration["JWT_ISSUER"], // <-- احتياط
		ValidateAudience = true,
		ValidAudience = jwtSettings["Audience"] ?? builder.Configuration["JWT_AUDIENCE"], // <-- احتياط
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
	// !!! --- هذا هو السطر الذي تم إصلاحه --- !!!
	// (كانت علامة = ناقصة قبل "v1")
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

// --- إنشاء الجداول ---
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{ // !!! --- تم حذف كلمة 'De' الخاطئة من هنا --- !!!
		var dbContext = services.GetRequiredService<DatabaseContext>();
		dbContext.Database.EnsureCreated();
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "An error occurred while creating the database.");
	}
}

// --- تفعيل الميدل وير ---
app.UseCors("AllowAll"); // !!! --- تم إصلاح الخطأ الإملائي هنا --- !!!
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://*:{port}");

// 🔄 دالة التحويل
static string ConvertSupabaseUrlToNpgsql(string databaseUrl)
{
	// شكل الرابط: postgres://user:password@host:5432/dbname
	var uri = new Uri(databaseUrl);
	var userInfo = uri.UserInfo.Split(':');

	var builder = new NpgsqlConnectionStringBuilder
	{
		Host = uri.Host,
		Port = uri.Port,
		Username = userInfo[0],
		Password = userInfo.Length > 1 ? userInfo[1] : "",
		Database = uri.AbsolutePath.Trim('/'),
		SslMode = SslMode.Require,
		TrustServerCertificate = true
	};

	return builder.ToString();
}

