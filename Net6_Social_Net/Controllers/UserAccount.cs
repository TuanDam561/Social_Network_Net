using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Net6_Social_Net.Data;
using Net6_Social_Net.Models;

namespace Net6_Social_Net.Controllers
{
    public class UserAccount : Controller
    {
        private readonly SocialNetworkContext db;

        public UserAccount(SocialNetworkContext context)
         {
             db = context;
         }
         public async Task<IActionResult> Index(int id)
         {
             var useId = HttpContext.Session.GetString("UserId");

             if (useId == null)
             {
                 TempData["Error"] = "Vui lòng đăng nhập hoặc đăng ký";
                 return RedirectToAction("Index", "Login");
             }

             int parsedUserId = int.Parse(useId);

             // Lấy thông tin người dùng
             var user = await db.Users
                                 .Where(u => u.UserId == id)
                                 .Select(u => new AccountModel
                                 {
                                     UserId = u.UserId,
                                     Username = u.Username,
                                     Email = u.Email,
                                     PhoneNumber = u.PhoneNumber,
                                     ProfilePicture = u.ProfilePicture,
                                     Bio = u.Bio,
                                     CreatedAt = u.CreatedAt,
                                     Imagebanner = u.Imagebanner,
                                     Role = u.Role,
                                 })
                                 .FirstOrDefaultAsync();

             // Kiểm tra xem người dùng có quyền xem các bài viết hay không
             var postsQuery = db.Posts
                                 .Where(p => p.UserId == id) // Chỉ lấy bài viết của người dùng này
                                 .Include(p => p.User)
                                 .OrderByDescending(p => p.CreatedAt)
                                 .Select(p => new PostViewModel
                                 {
                                     Id = p.PostId,
                                     UserId = p.UserId,
                                     Content = p.Content,
                                     ImageUrl = p.ImageUrl,
                                     CreatedAt = p.CreatedAt ?? DateTime.Now,
                                     Status = p.Status,
                                     UserName = p.User.Username
                                 });

             // Nếu UserId không trùng với id trong DB, chỉ lấy bài viết công khai
             if (parsedUserId != id)
             {
                 postsQuery = postsQuery.Where(p => p.Status != "private");
             }

             // Lấy danh sách bài viết
             var posts = await postsQuery.ToListAsync();

             // Tạo ViewModel
             var viewModel = new UserProfileViewModel
             {
                 User = user,
                 Posts = posts
             };

             return View(viewModel);
         }



    }
}
