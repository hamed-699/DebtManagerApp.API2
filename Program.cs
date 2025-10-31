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
using Microsoft.Extensions.Logging; // <-- تأكد من وجود هذا

var builder = WebApplication.CreateBuilder(args);

// --- إعدادات Supabase (PostgreSQL) ---
var connectionString = builder.Configuration["SUPABASE_CONNECTION_STRING"];
builder.Services.AddDbContext<DatabaseContext>(options =>
	options.UseNpgsql(connectionString));

// --- إعدادات JWT (JSON Web Token) ---
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]
	?? throw new InvalidOperationException("JWT Key is missing in configuration."));

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
		ValidIssuer = jwtSettings["Issuer"],
		ValidateAudience = true,
		ValidAudience = jwtSettings["Audience"],
		ValidateLifetime = true,
		ClockSkew = TimeSpan.Zero
	};
});

// --- إعدادات خدمة البريد الإلكتروني (SmtpSettings) ---
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<EmailService>();

// إضافة الخدمات الأخرى
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


// إضافة سياسة CORS (مهم جداً)
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

// --- !!! هذا هو الكود الهام الذي أعدناه !!! ---
// هذا الكود يخبر الخادم بأن يتأكد من إنشاء جداول قاعدة البيانات (مثل Users و Organizations)
// في سوباس عند بدء التشغيل، إذا لم تكن موجودة.
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		var dbContext = services.GetRequiredService<DatabaseContext>();
		// هذا السطر هو الذي يقوم بإنشاء الجداول
		dbContext.Database.EnsureCreated();
	}
	catch (Exception ex)
	{
		// في حال حدوث خطأ أثناء إنشاء الجداول، اطبعه في سجلات "رندر"
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "An error occurred while creating the database.");
	}
}
// --- !!! نهاية الكود المُعاد !!! ---


// تفعيل CORS
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

// تطبيق التوجيه (Routing)
app.UseRouting();

// تطبيق المصادقة (Authentication)
app.UseAuthentication();

// تطبيق الصلاحيات (Authorization)
app.UseAuthorization();

app.MapControllers();

// --- تعديل هام جداً للنشر على Render ---
// !!! --- هذا هو السطر الذي تم تصحيحه --- !!!
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://*:{port}");

