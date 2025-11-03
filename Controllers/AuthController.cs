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
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Storage; // --- إضافة جديدة للأمان الذري ---

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

		[HttpPost("register")]
		public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
		{
			// --- (تعديل 1) إضافة "الأمان الذري" (Transaction) ---
			// هذا يضمن أن جميع العمليات تنجح معاً أو تفشل معاً
			await using var transaction = await _context.Database.BeginTransactionAsync();

			// --- (تعديل 2) إضافة "كاشف الأخطاء" ---
			try
			{
				// التحقق مما إذا كان اسم المستخدم موجوداً بالفعل
				if (await _context.Users.AnyAsync(u => u.Username.ToLower() == userRegisterDto.Username.ToLower()))
				{
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
					Settings = new OrganizationSettings { ShopName = userRegisterDto.OrganizationName }
				};

				// 2. إنشاء المستخدم الجديد وربطه بالمؤسسة
				var newUser = new User
				{
					Username = userRegisterDto.Username,
					Email = userRegisterDto.Email,
					PasswordHash = passwordHash,
					Role = UserRole.Admin,
					Organization = newOrganization
				};

				_context.Users.Add(newUser);
				await _context.SaveChangesAsync();

				// --- تأكيد نجاح العملية ---
				await transaction.CommitAsync();

				// جلب المستخدم مع المؤسسة (التي نحتاجها في العميل)
				var userForReturn = await _context.Users
					.Include(u => u.Organization)
					.FirstOrDefaultAsync(u => u.Id == newUser.Id);

				// إنشاء "بطاقة الهوية الرقمية" (JWT Token)
				var token = GenerateJwtToken(newUser);

				// إرجاع الكائن الذي يتوقعه العميل
				return Ok(new { token, user = userForReturn });
			}
			catch (Exception ex)
			{
				// --- إذا حدث خطأ، قم بإلغاء كل شيء ---
				await transaction.RollbackAsync();

				// طباعة الخطأ في سجلات الخادم (Logs)
				Console.WriteLine($"[AUTH-REGISTER-ERROR] {ex.ToString()}");

				// --- إرجاع رسالة الخطأ الحقيقية إلى التطبيق ---
				// هذا سيوقف التخمين ويظهر الخطأ "relation Users does not exist"
				return StatusCode(500, new { message = $"فشل تسجيل الحساب: {ex.Message}" });
			}
		}


		[HttpPost("login")]
		public async Task<IActionResult> Login(UserLoginDto userLoginDto)
		{
			// --- (تعديل) إضافة "كاشف الأخطاء" ---
			try
			{
				// جلب المستخدم مع المؤسسة
				var user = await _context.Users
					.Include(u => u.Organization)
					.FirstOrDefaultAsync(u => u.Username.ToLower() == userLoginDto.Username.ToLower());

				// التحقق من وجود المستخدم وصحة كلمة المرور
				if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
				{
					return Unauthorized(new { message = "اسم المستخدم أو كلمة المرور غير صحيحة." });
				}

				// إنشاء "بطاقة الهوية الرقمية" (JWT Token)
				var token = GenerateJwtToken(user);

				return Ok(new { token, user });
			}
			catch (Exception ex)
			{
				// طباعة الخطأ في سجلات الخادم (Logs)
				Console.WriteLine($"[AUTH-LOGIN-ERROR] {ex.ToString()}");

				// --- إرجاع رسالة الخطأ الحقيقية إلى التطبيق ---
				return StatusCode(500, new { message = $"فشل تسجيل الدخول: {ex.Message}" });
			}
		}

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

		[HttpPost("update-cloud-password")]
		public async Task<IActionResult> UpdateCloudPassword(CloudPasswordResetDto resetDto)
		{
			// --- (تعديل) إضافة "كاشف الأخطاء" ---
			try
			{
				var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == resetDto.Username.ToLower());

				if (user == null)
				{
					return NotFound(new { message = "المستخدم غير موجود." });
				}

				if (user.Email == null || user.Email.ToLower() != resetDto.Email.ToLower())
				{
					return Unauthorized(new { message = "البريد الإلكتروني غير مطابق." });
				}

				var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);
				user.PasswordHash = newPasswordHash;

				await _context.SaveChangesAsync();
				return Ok(new { message = "تم تحديث كلمة المرور سحابياً بنجاح." });
			}
			catch (Exception ex)
			{
				// طباعة الخطأ في سجلات الخادم (Logs)
				Console.WriteLine($"[AUTH-UPDATEPW-ERROR] {ex.ToString()}");

				// --- إرجاع رسالة الخطأ الحقيقية إلى التطبيق ---
				return StatusCode(500, new { message = $"فشل تحديث كلمة المرور: {ex.Message}" });
			}
		}


		private string GenerateJwtToken(User user)
		{
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

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
				expires: DateTime.Now.AddHours(24),
				signingCredentials: credentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
