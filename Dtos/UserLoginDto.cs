using System.ComponentModel.DataAnnotations;

namespace DebtManagerApp.API.Dtos
{
    // هذا النموذج يستخدم لاستقبال بيانات المستخدم عند تسجيل الدخول
    public class UserLoginDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
