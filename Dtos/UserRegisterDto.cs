using System.ComponentModel.DataAnnotations;

namespace DebtManagerApp.API.Dtos
{
    // هذا النموذج يستخدم لاستقبال بيانات المستخدم عند تسجيل حساب جديد
    public class UserRegisterDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string OrganizationName { get; set; } // اسم المحل أو الشركة الجديدة
    }
}
