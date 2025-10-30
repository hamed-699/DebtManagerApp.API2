using DebtManagerApp.API.Dtos;
using DebtManagerApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations; // --- إضافة جديدة ---

namespace DebtManagerApp.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly DatabaseContext _context;
		private readonly IConfiguration _config;

		public AuthController(DatabaseContext context, IConfiguration config)
		{
			_context = context;
			_config = config;
		}

		// !!! --- تم تعديل دالة التسجيل بالكامل --- !!!
		[HttpPost("register")]
		public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
		{
			// التحقق مما إذا كان اسم المستخدم موجوداً بالفعل
			if (await _context.Users.AnyAsync(u => u.Username.ToLower() == userRegisterDto.Username.ToLower()))
			{
				// --- (تعديل 1) إرجاع رسالة خطأ واضحة ---
				return BadRequest(new { message = "اسم المستخدم هذا موجود مسبقاً." });
			}

			// التحقق من البريد الإلكتروني (اختياري لكن موصى به)
			if (await _context.Users.AnyAsync(u => u.Email.ToLower() == userRegisterDto.Email.ToLower()))
			{
				return BadRequest(new { message = "هذا البريد الإلكتروني مسجل مسبقاً." });
			}

			// تشفير كلمة المرور باستخدام BCrypt
			var passwordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password);

			// 1. إنشاء المؤسسة الجديدة أولاً
			var newOrganization = new Organization
			{
				Name = userRegisterDto.OrganizationName,
				// إنشاء الإعدادات الافتراضية للمؤسسة
				Settings = new OrganizationSettings { ShopName = userRegisterDto.OrganizationName }
			};

			// 2. إنشاء المستخدم الجديد وربطه بالمؤسسة
			var newUser = new User
			{
				Username = userRegisterDto.Username,
				Email = userRegisterDto.Email,
				PasswordHash = passwordHash,
				Role = UserRole.Admin, // أول مستخدم في المؤسسة هو المشرف دائماً
				Organization = newOrganization
			};

			_context.Users.Add(newUser);
			await _context.SaveChangesAsync();

			// --- (تعديل 2) إرجاع الكائن الصحيح (Token + User) ---
			// جلب المستخدم مع المؤسسة (التي نحتاجها في العميل)
			var userForReturn = await _context.Users
				.Include(u => u.Organization) // <-- تحميل المؤسسة
				.FirstOrDefaultAsync(u => u.Id == newUser.Id);

			// إنشاء "بطاقة الهوية الرقمية" (JWT Token)
			var token = GenerateJwtToken(newUser);

			// إرجاع الكائن الذي يتوقعه العميل
			return Ok(new { token, user = userForReturn });
		}
		// !!! --- نهاية تعديل دالة التسجيل --- !!!


		// !!! --- تم تعديل دالة تسجيل الدخول --- !!!
		[HttpPost("login")]
		public async Task<IActionResult> Login(UserLoginDto userLoginDto)
		{
			// جلب المستخدم مع المؤسسة
			var user = await _context.Users
				.Include(u => u.Organization) // <-- تحميل المؤسسة
				.FirstOrDefaultAsync(u => u.Username.ToLower() == userLoginDto.Username.ToLower());

			// التحقق من وجود المستخدم وصحة كلمة المرور
			if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
			{
				return Unauthorized("Invalid username or password.");
			}

			// إنشاء "بطاقة الهوية الرقمية" (JWT Token)
			var token = GenerateJwtToken(user);

			// --- (تعديل) إرجاع الكائن الكامل (Token + User) ---
			return Ok(new { token, user });
		}
		// !!! --- نهاية تعديل دالة تسجيل الدخول --- !!!

		// --- بداية الإضافة: "صندوق" بيانات لنقطة النهاية الجديدة ---
		public class CloudPasswordResetDto
		{
			[Required]
			public string Username { get; set; } = string.Empty;
			[Required]
			public string NewPassword { get; set; } = string.Empty;
			[Required]
			[EmailAddress]
			public string Email { get; set; } = string.Empty;
		}
		// --- نهاية الإضافة ---

		// --- بداية الإضافة: نقطة نهاية جديدة لتحديث كلمة السر في السحابة ---
		[HttpPost("update-cloud-password")]
		public async Task<IActionResult> UpdateCloudPassword(CloudPasswordResetDto resetDto)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == resetDto.Username.ToLower());

			if (user == null)
			{
				return NotFound("User not found.");
			}

			// خطوة تحقق أمنية: نتأكد أن البريد الإلكتروني المرسل مطابق للبريد المسجل
			if (user.Email == null || user.Email.ToLower() != resetDto.Email.ToLower())
			{
				return Unauthorized("Invalid email verification.");
			}

			// تشفير كلمة السر الجديدة باستخدام BCrypt (نفس طريقة الخادم)
			var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);
			user.PasswordHash = newPasswordHash;

			await _context.SaveChangesAsync();
			return Ok("Cloud password updated successfully.");
		}
		// --- نهاية الإضافة ---


		private string GenerateJwtToken(User user)
		{
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			// المعلومات التي سيتم تخزينها في البطاقة
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Username),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim("userId", user.Id.ToString()),
				new Claim("organizationId", user.OrganizationId.ToString()),
				new Claim(ClaimTypes.Role, user.Role.ToString())
			};

			var token = new JwtSecurityToken(
				issuer: _config["Jwt:Issuer"],
				audience: _config["Jwt:Audience"],
				claims: claims,
				expires: DateTime.Now.AddHours(24), // صلاحية البطاقة: 24 ساعة
				signingCredentials: credentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
