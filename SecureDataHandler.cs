using System;
using System.Security.Cryptography;

namespace DebtManagerApp.API.Services
{
	/// <summary>
	/// هذه نسخة مطابقة لملف القفل الموجود في تطبيق سطح المكتب
	/// لضمان تطابق خوارزميات التشفير بين العميل والخادم.
	/// (تمت إزالة دوال DPAPI لأنها لا تعمل على الخادم)
	/// </summary>
	public static class SecureDataHandler
	{
		/// <summary>
		/// يقوم بتشفير كلمة المرور باستخدام خوارزمية آمنة (PBKDF2).
		/// </summary>
		public static string HashPassword(string password)
		{
			byte[] salt;
			new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

			var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
			byte[] hash = pbkdf2.GetBytes(20);

			byte[] hashBytes = new byte[36];
			Array.Copy(salt, 0, hashBytes, 0, 16);
			Array.Copy(hash, 0, hashBytes, 16, 20);

			return Convert.ToBase64String(hashBytes);
		}

		/// <summary>
		/// يتحقق مما إذا كانت كلمة المرور المدخلة تتطابق مع كلمة المرور المشفرة المحفوظة.
		/// </summary>
		public static bool VerifyPassword(string password, string hashedPassword)
		{
			if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
				return false;

			try
			{
				byte[] hashBytes = Convert.FromBase64String(hashedPassword);
				byte[] salt = new byte[16];
				Array.Copy(hashBytes, 0, salt, 0, 16);

				var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
				byte[] hash = pbkdf2.GetBytes(20);

				for (int i = 0; i < 20; i++)
				{
					if (hashBytes[i + 16] != hash[i])
					{
						return false;
					}
				}
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
