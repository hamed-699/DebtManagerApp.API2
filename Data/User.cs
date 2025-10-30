using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtManagerApp.Data
{
    /// <summary>
    /// يمثل مستخدمًا في النظام، مع بيانات تسجيل الدخول والدور الخاص به.
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        // --- بداية الإضافة الأساسية ---

        // المفتاح الخارجي لجدول المؤسسات (إجباري)
        public int OrganizationId { get; set; }

        // خاصية التنقل للوصول إلى بيانات المؤسسة المرتبطة
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; }

        // --- نهاية الإضافة الأساسية ---
    }
}

