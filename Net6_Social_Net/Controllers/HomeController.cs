using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient.DataClassification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Net6_Social_Net.Data;
using Net6_Social_Net.Models;
using Net7_Social_Net.Controllers;
using Net7_Social_Net.Models;
using System.Diagnostics;

namespace Social_Network.Controllers
{
    public class HomeController : BaseController
    {
       private SocialNetworkContext db=new SocialNetworkContext();

      
        public async Task<IActionResult> Index()
        {
            // Lấy UserId từ Session
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng về trang Login
                TempData["Error"] = "Vui lòng đăng nhập hoặc đăng ký";
                return RedirectToAction("Index", "Login");
            }

            // Truy vấn bài viết với các mối quan hệ liên quan
            var postsQuery = db.Posts
                .Include(p => p.User) // Bao gồm thông tin người dùng
                .Include(p => p.Comments) // Bao gồm bình luận
                    .ThenInclude(c => c.User) // Bao gồm thông tin người dùng của bình luận
                .Where(p => p.Status == "Public") // Lọc bài viết public
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
                });

            // Chuyển dữ liệu sang danh sách và xử lý thêm
            var postsList = await postsQuery.ToListAsync();

            // Sắp xếp: Ưu tiên bài viết mới đăng trong vòng 1 giờ, sau đó xáo trộn danh sách
            postsList = postsList
                .OrderByDescending(p => p.CreatedAt > DateTime.Now.AddHours(-1)) // Bài viết mới đăng trong 1 giờ lên đầu
                .ThenBy(p => Guid.NewGuid()) // Xáo trộn bài viết còn lại
                .ToList();

            // Trả về danh sách bài viết cho View
            return View(postsList);
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

         [HttpPost]
         public async Task<IActionResult> Deletepost(int id)
         {
             var userid = HttpContext.Session.GetString("UserId");
             int userss = int.Parse(userid);
             if (userid == null)
             {
                 TempData["Error"] = "Phiên đăng nhập hết hạn";
                 return RedirectToAction("Index", "Login");
             }
             // Tìm bài viết theo id
             var post = await db.Posts.FindAsync(id);
             if (post == null)
             {
                 TempData["Error"] = "Bài viết không tồn tại.";
                 return RedirectToAction("Index");
             }
             // Kiểm tra quyền sở hữu bài viết
             if (post.UserId != userss)
             {
                 TempData["Error"] = "Bạn không có quyền xóa bài viết này.";
                 return RedirectToAction("Index");
             }
             // Xóa ảnh nếu có
             if (!string.IsNullOrEmpty(post.ImageUrl))
             {
                 // Đường dẫn đầy đủ đến ảnh trong thư mục wwwroot
                 var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.ImageUrl.TrimStart('/'));

                 // Kiểm tra xem tệp ảnh có tồn tại không trước khi xóa
                 if (System.IO.File.Exists(imagePath))
                 {
                     System.IO.File.Delete(imagePath); // Xóa tệp ảnh
                 }
             }
             // Xóa bài viết
             db.Posts.Remove(post);
             await db.SaveChangesAsync();

             TempData["Success"] = "Đã xóa bài viết thành công!";
             return RedirectToAction("Index");
         }

        [HttpPost]
        public async Task<IActionResult> UpdatePost(int idpost, string Content, IFormFile ImageFile, string StatusPost)
        {
            var userid = HttpContext.Session.GetString("UserId");
            if (userid == null)
            {
                return Json(new { success = false, error = "Phiên đăng nhập hết hạn" });
            }

            int userss = int.Parse(userid);

            // Tìm bài viết và kiểm tra quyền sở hữu
            var post = await db.Posts.FindAsync(idpost);
            if (post == null || post.UserId != userss)
            {
                return Json(new { success = false, error = "Bạn không có quyền cập nhật bài viết này." });
            }

            // Cập nhật nội dung và trạng thái
            post.Content = Content;
            post.Status = StatusPost;
            post.UpdatedAt = DateTime.Now;

            // Xử lý cập nhật ảnh (nếu có)
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Xóa ảnh cũ nếu có
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

                // Cập nhật đường dẫn ảnh mới
                post.ImageUrl = $"/Post_Image/{fileName}";
            }

            await db.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}

