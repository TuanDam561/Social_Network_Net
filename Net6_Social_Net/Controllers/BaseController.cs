using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Net7_Social_Net.Controllers
{
  
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Kiểm tra session
            var sessionUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(sessionUserId))
            {
                // Nếu session hết hạn, kiểm tra cookie
                var userId = Request.Cookies["UserId"];
                var userName = Request.Cookies["UserName"];

                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userName))
                {
                    // Khôi phục session từ cookie
                    HttpContext.Session.SetString("UserId", userId);
                    HttpContext.Session.SetString("UserName", userName);

                    // Gia hạn session bằng cách khôi phục session và gán lại thời gian sống
                    HttpContext.Session.SetString("LastActivity", DateTime.Now.ToString());
                }
                else
                {
                    TempData["Error"] = "Vui lòng đăng ký hoặc đăng nhập";
                    // Nếu cả session và cookie không tồn tại, chuyển hướng đến trang đăng nhập
                    context.Result = RedirectToAction("Index", "Login");
                }
            }
            else
            {
                // Nếu session vẫn còn, gia hạn lại session
                HttpContext.Session.SetString("LastActivity", DateTime.Now.ToString());
            }

            base.OnActionExecuting(context);
        }
    }

}
