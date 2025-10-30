using DebtManagerApp.Data;
using Microsoft.AspNetCore.Authorization; // تمت الإضافة لتفعيل الحماية
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // تمت الإضافة لقراءة بطاقة الهوية
using System.Threading.Tasks;

namespace DebtManagerApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // <--- هذه هي اللافتة الجديدة: "ممنوع الدخول إلا المصرح لهم"
    public class CustomersController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public CustomersController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            // --- بداية المنطق الجديد والذكي ---

            // 1. اقرأ "بطاقة الهوية الرقمية" الخاصة بالمستخدم الذي أرسل الطلب
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            // 2. استخرج رقم المؤسسة من البطاقة
            var organizationIdClaim = identity.FindFirst("organizationId");
            if (organizationIdClaim == null)
            {
                return Unauthorized("Invalid token: Missing organizationId.");
            }

            var organizationId = int.Parse(organizationIdClaim.Value);

            // 3. اذهب إلى قاعدة البيانات واجلب فقط العملاء الذين ينتمون لهذه المؤسسة
            var customers = await _context.Customers
                                          .Where(c => c.OrganizationId == organizationId)
                                          .ToListAsync();

            return Ok(customers);
            // --- نهاية المنطق الجديد والذكي ---
        }
    }
}

