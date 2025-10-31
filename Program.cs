using DebtManagerApp.API.Services;
using Microsoft.EntityFrameworkCore;
using DebtManagerApp.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using DebtManagerApp.Data; // <-- (الإصلاح الأول) تم تصحيح المسار
						   // using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // (تمت الإزالة لأنها سببت أخطاء)
using Microsoft.OpenApi.Models; // لـ OpenApiInfo
								// using Google.Apis.Auth.OAuth2; // (تمت الإزالة)
								// using Google.Cloud.Language.V1; // (تمت الإزالة)

// !!! --- هذا هو السطر الجديد الذي أضفته --- !!!
using System.Text.Json.Serialization;
// !!! --- نهاية السطر الجديد --- !!!

var builder = WebApplication.CreateBuilder(args);

// --- إعدادات Supabase (PostgreSQL) ---
// !!! --- هذا هو السطر الذي تم إصلاحه نهائياً --- !!!
// قمنا بتغيير "DefaultConnection" إلى "SupabaseConnection"
// ليطابق الاسم الذي وضعناه في "متغيرات البيئة" في Render
var connectionString = builder.Configuration.GetConnectionString("SupabaseConnection");
builder.Services.AddDbContext<DatabaseContext>(options =>
	options.UseNpgsql(connectionString));

// --- (الإصلاح الثاني) ---
// تم حذف إعدادات .AddIdentity() بالكامل لأنها كانت تسبب أخطاء
// ويبدو أن المشروع يعتمد على JWT فقط للمصادقة

// --- إعدادات JWT (JSON Web Token) ---
// (هذا هو نظام المصادقة الذي كان موجوداً)
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
	options.RequireHttpsMetadata = false; // في الوضع المجاني، قد لا نستخدم HTTPS دائماً
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
		ClockSkew = TimeSpan.Zero // لا تسمح بفارق زمني
	};
});

// --- إعدادات خدمة البريد الإلكتروني (SmtpSettings) ---
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<EmailService>(); // (هذا صحيح)

// --- (الإصلاح الثالث) ---
// تم حذف إعدادات Google Cloud (Gemini) من هنا
// لأن الـ Controller سيتعامل معها بنفسه، وهذا الإعداد كان يسبب أخطاء


// إضافة الخدمات الأخرى
// !!! --- هذا هو التعديل الثاني لحل خطأ 500 --- !!!
builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		// هذا السطر يخبر الخادم بتجاهل الحلقات المفرغة
		options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
	});
// !!! --- نهاية التعديل --- !!!

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "DebtManagerApp.API", Version = "v1" });
	// إضافة إعدادات JWT لـ Swagger UI
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

// --- (الإصلاح الرابع) ---
// تم حذف إعدادات SecureDataHandler
// يبدو أنه كلاس static ولا يجب حقنه (inject)


var app = builder.Build();

// تفعيل CORS
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

// تطبيق التوجيه (Routing)
app.UseRouting();

// تطبيق المصادقة (Authentication)
app.UseAuthentication();

// تطبيق الصلاحيات (Authorization)
app.UseAuthorization();

app.MapControllers();

// --- تعديل هام جداً للنشر على Render ---
// قراءة المنفذ (PORT) من متغيرات البيئة التي تضبطها Render
// إذا لم يتم العثور عليه، استخدم منفذ افتراضي مثل 8080
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

// تشغيل الخادم على جميع العناوين (http://*) وعلى المنفذ المحدد
app.Run($"http://*:{port}");


