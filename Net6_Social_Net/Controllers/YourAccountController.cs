using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Net6_Social_Net.Data;
using Net6_Social_Net.Models;
using Net7_Social_Net.Models;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Social_Network.Controllers
{
    public class YourAccountController : Controller
    {
        private readonly SocialNetworkContext db;

        public YourAccountController(SocialNetworkContext context)
        {
            db = context;
        }
        /* public async Task<IActionResult> Index()
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
                                 .Where(u => u.UserId == parsedUserId)
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

             // Lấy danh sách bài viết
             var posts = await db.Posts
                                 .Where(p => p.UserId == parsedUserId) // Chỉ lấy bài viết của người dùng này
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
                                 })
                                 .ToListAsync();

             // Tạo ViewModel
             var viewModel = new UserProfileViewModel
             {
                 User = user,
                 Posts = posts
             };

             return View(viewModel);
         }*/

        public async Task<IActionResult> Index()
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
                                .Where(u => u.UserId == parsedUserId)
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

            // Truy vấn bài viết với các mối quan hệ liên quan
            var posts = await db.Posts
                .Where(p => p.UserId == parsedUserId) // Chỉ lấy bài viết của người dùng này
                .Include(p => p.User) // Bao gồm thông tin người dùng
                .Include(p => p.Comments) // Bao gồm bình luận
                    .ThenInclude(c => c.User) // Bao gồm thông tin người dùng của bình luận
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostViewModel
                {
                    Id = p.PostId,
                    UserId = p.UserId,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt ?? DateTime.Now,
                    Status = p.Status,
                    UserName = p.User.Username,
                    UpdatedAt = p.UpdatedAt ?? DateTime.Now,
                    AvatarUser = p.User.ProfilePicture,

                    Comments = p.Comments
                        .OrderBy(c => c.CreatedAt) // Sắp xếp bình luận theo thời gian tạo
                        .Select(c => new CommentViewModel
                        {
                            CommentId = c.CommentId,
                            PostId = c.PostId,
                            UserId = c.UserId,
                            Content = c.Content,
                            CreatedAt = c.CreatedAt ?? DateTime.Now,
                            UserName = c.User.Username,
                            AvatarUser = c.User.ProfilePicture
                        }).ToList()
                })
                .ToListAsync();

            // Tạo ViewModel
            var viewModel = new UserProfileViewModel
            {
                User = user,
                Posts = posts
            };

            return View(viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> DeleteyourPosts(int id)
        {
            var userid = HttpContext.Session.GetString("UserId");
            int userss = int.Parse(userid);
            if (userid == null)
            {
                return Json(new { success = false, error = "Phiên đăng nhập hết hạn" });
            }

            // Tìm bài viết theo id
            var post = await db.Posts.FindAsync(id);
            if (post == null)
            {
                return Json(new { success = false, error = "Bài viết không tồn tại." });
            }

            // Kiểm tra quyền sở hữu bài viết
            if (post.UserId != userss)
            {
                return Json(new { success = false, error = "Bạn không có quyền xóa bài viết này." });
            }

            // Xóa ảnh nếu có
            if (!string.IsNullOrEmpty(post.ImageUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath); // Xóa tệp ảnh
                }
            }
            // Xóa bài viết
            db.Posts.Remove(post);
            await db.SaveChangesAsync();

            return Json(new { success = true });
        }


      
        [HttpPost]
        public async Task<IActionResult> UpdatePost(int idpost, string Content, IFormFile ImageFile, String StatusPost)
        {
            var userid = HttpContext.Session.GetString("UserId");
            if (userid == null)
            {
                return Json(new { success = false, error = "Phiên đăng nhập hết hạn" });
            }

            int userss = int.Parse(userid);

            var post = await db.Posts.FindAsync(idpost);
            if (post == null || post.UserId != userss)
            {
                return Json(new { success = false, error = "Bạn không có quyền cập nhật bài viết này." });
            }

            post.Content = Content;
            post.Status = StatusPost;
            post.UpdatedAt = DateTime.Now;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(post.ImageUrl))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Post_Image");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var fileName = $"{Guid.NewGuid()}_{ImageFile.FileName}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                post.ImageUrl = $"/Post_Image/{fileName}";
            }

            await db.SaveChangesAsync();

            return Json(new { success = true });
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProfile(int id, string username, IFormFile banner, IFormFile avatar, string bio)
        {
            var usersession = HttpContext.Session.GetString("UserId");
            if (usersession == null)
            {
                TempData["Error"] = "Phiên đăng nhập hết hạn!";
                return RedirectToAction("Index", "Login");
            }

            int userid = int.Parse(usersession);
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userid);
            if (user == null || user.UserId != id)
            {
                TempData["Error"] = "Bạn không được quyền sửa thông tin người khác";
                return RedirectToAction("Index");
            }

            // Cập nhật thông tin cơ bản
            user.Username = username;
            user.Bio = bio;

            // Xử lý cập nhật ảnh banner nếu có
            if (banner != null && banner.Length > 0)
            {
                // Xóa ảnh banner cũ nếu có
                if (!string.IsNullOrEmpty(user.Imagebanner))
                {
                    var oldBannerPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.Imagebanner.TrimStart('/'));
                    if (System.IO.File.Exists(oldBannerPath))
                    {
                        System.IO.File.Delete(oldBannerPath); // Xóa ảnh cũ
                    }
                }

                var bannerPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Profile_Banners");
                if (!Directory.Exists(bannerPath))
                {
                    Directory.CreateDirectory(bannerPath);
                }

                var bannerFileName = $"{Guid.NewGuid()}_{banner.FileName}";
                var bannerFilePath = Path.Combine(bannerPath, bannerFileName);

                using (var stream = new FileStream(bannerFilePath, FileMode.Create))
                {
                    await banner.CopyToAsync(stream);
                }

                // Cập nhật đường dẫn ảnh banner mới
                user.Imagebanner = $"/Profile_Banners/{bannerFileName}";
            }

            // Xử lý cập nhật ảnh avatar nếu có
            if (avatar != null && avatar.Length > 0)
            {
                // Xóa ảnh avatar cũ nếu có
                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    var oldAvatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicture.TrimStart('/'));
                    if (System.IO.File.Exists(oldAvatarPath))
                    {
                        System.IO.File.Delete(oldAvatarPath); // Xóa ảnh cũ
                    }
                }
                var avatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Profile_Avatars");
                if (!Directory.Exists(avatarPath))
                {
                    Directory.CreateDirectory(avatarPath);
                }

                var avatarFileName = $"{Guid.NewGuid()}_{avatar.FileName}";
                var avatarFilePath = Path.Combine(avatarPath, avatarFileName);

                using (var stream = new FileStream(avatarFilePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                // Cập nhật đường dẫn ảnh avatar mới
                user.ProfilePicture = $"/Profile_Avatars/{avatarFileName}";
            }
            // Lưu thay đổi vào cơ sở dữ liệu
            await db.SaveChangesAsync();

            TempData["Success"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Post(string Content, IFormFile ImageFile, String StatusPost)
        {
            if (string.IsNullOrEmpty(Content))
            {
                TempData["Error"] = "Nội dung bài viết không được để trống.";
                return RedirectToAction("Index");
            }

            var userID = HttpContext.Session.GetString("UserId");
            if (userID == null)
            {
                TempData["Error"] = "Phiên đăng nhập hết hạn";
                return RedirectToAction("Index", "Login");
            }

            int useriD = int.Parse(userID);

            // Kiểm tra và lưu file ảnh nếu có
            string imageUrl = null;
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Tạo đường dẫn lưu ảnh trong wwwroot
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Post_Image");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Đặt tên file và lưu ảnh
                var fileName = $"{Guid.NewGuid()}_{ImageFile.FileName}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                // Lưu đường dẫn ảnh để lưu trong cơ sở dữ liệu
                imageUrl = $"/Post_Image/{fileName}";
            }

            // Tạo đối tượng bài viết mới
            var post = new Post
            {
                UserId = useriD,
                Content = Content,
                ImageUrl = imageUrl,
                Status = StatusPost,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            db.Posts.Add(post);
            await db.SaveChangesAsync();

            TempData["Success"] = "Đăng bài viết thành công!";
            return RedirectToAction("Index");
        }

    }
}
