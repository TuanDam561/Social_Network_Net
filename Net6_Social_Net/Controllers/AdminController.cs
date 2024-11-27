using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Net6_Social_Net.Data;

namespace Social_Network.Controllers
{

    public class AdminController : Controller
    {
        private readonly SocialNetworkContext db = new SocialNetworkContext();
        public async Task<IActionResult> Index()
        {
            var useId = HttpContext.Session.GetString("UserId");
            if (useId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập hoặc đăng ký";
                return RedirectToAction("Index", "Login");
            }

            if (!int.TryParse(useId, out int use))
            {
                TempData["Error"] = "ID người dùng không hợp lệ";
                return RedirectToAction("Index", "Login");
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == use);
            if (user == null)
            {
                TempData["Error"] = "Người dùng không tồn tại";
                return RedirectToAction("Index", "Login");
            }

            if (user.Role != "Admin")
            {
                TempData["Error"] = "Khỏi vào";
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Login");
            }

            return View();
        }
    }
}
