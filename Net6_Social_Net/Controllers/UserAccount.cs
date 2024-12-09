using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Net6_Social_Net.Data;
using Net6_Social_Net.Models;
using Net7_Social_Net.Models;

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

            // Truy vấn bài viết với các mối quan hệ liên quan
            var postsQuery = db.Posts
                .Where(p => p.UserId == id) // Chỉ lấy bài viết của người dùng này
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
        [HttpPost]
        public async Task<IActionResult> AddFriend(int friendId)
        {
            if (friendId == 0)
            {
                return Json(new { status = "error", message = "Không thể kết bạn" });
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Json(new { status = "error", message = "Vui lòng đăng nhập" });
            }

            int userId = int.Parse(userIdStr);

           

            var friendExists = await db.Users.AnyAsync(u => u.UserId == friendId);
            if (!friendExists)
            {
                return Json(new { status = "error", message = "Người dùng không tồn tại" });
            }

            var existingFriendship = await db.Friends
                .FirstOrDefaultAsync(f => (f.UserId == userId && f.FriendId == friendId)
                                       || (f.UserId == friendId && f.FriendId == userId));
            if (existingFriendship != null)
            {
                return Json(new { status = "error", message = "Yêu cầu đã tồn tại hoặc đã là bạn bè" });
            }

            try
            {
                var newFriendRequest = new Friend
                {
                    UserId = userId,
                    FriendId = friendId,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                db.Friends.Add(newFriendRequest);
                await db.SaveChangesAsync();

                return Json(new { status = "success", message = "Đã gửi yêu cầu kết bạn" });
            }
            catch (Exception)
            {
                return Json(new { status = "error", message = "Đã xảy ra lỗi, vui lòng thử lại sau" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelFriendRequest(int friendId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Json(new { status = "error", message = "Vui lòng đăng nhập" });
            }

            int userId = int.Parse(userIdStr);

            var friendRequest = await db.Friends
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FriendId == friendId && f.Status == "Pending");

            if (friendRequest == null)
            {
                return Json(new { status = "error", message = "Không tìm thấy yêu cầu kết bạn" });
            }

            try
            {
                db.Friends.Remove(friendRequest);
                await db.SaveChangesAsync();

                return Json(new { status = "success", message = "Đã hủy yêu cầu kết bạn" });
            }
            catch (Exception)
            {
                return Json(new { status = "error", message = "Đã xảy ra lỗi, vui lòng thử lại sau" });
            }
        }

        /* public async Task<IActionResult> GetFriendStatus(int friendId)
         {
             // Lấy UserId từ session
             var userIdStr = HttpContext.Session.GetString("UserId");
             if (string.IsNullOrEmpty(userIdStr))
             {
                 return Json(new { status = "error", message = "Vui lòng đăng nhập" });
             }

             int userId = int.Parse(userIdStr);

             // Tìm yêu cầu kết bạn giữa userId và friendId (hoặc ngược lại)
             var friendRequest = await db.Friends
                 .FirstOrDefaultAsync(f => (f.UserId == userId && f.FriendId == friendId)
                                        || (f.UserId == friendId && f.FriendId == userId));

             if (friendRequest == null)
             {
                 // Nếu không tìm thấy yêu cầu kết bạn, trả về trạng thái "none"
                 return Json(new { status = "success", message = "Kết bạn", friendStatus = "none" });
             }

             // Kiểm tra trạng thái của yêu cầu kết bạn
             if (friendRequest.Status == "Friend")
             {
                 // Nếu trạng thái là "Friend", trả về "Bạn bè"
                 return Json(new { status = "success", message = "Bạn bè", friendStatus = "Friend" });
             }

             // Nếu yêu cầu kết bạn đang chờ hoặc có trạng thái khác, trả về "Đang chờ"
             return Json(new { status = "success", message = "Đang chờ", friendStatus = friendRequest.Status });
         }*/
        public async Task<IActionResult> GetFriendStatus(int friendId)
        {
            // Lấy UserId từ session
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Json(new { status = "error", message = "Vui lòng đăng nhập" });
            }

            int userId = int.Parse(userIdStr);

            // Tìm yêu cầu kết bạn giữa userId và friendId (hoặc ngược lại)
            var friendRequest = await db.Friends
                .Where(f => (f.UserId == userId && f.FriendId == friendId)
                         || (f.UserId == friendId && f.FriendId == userId))
                .Select(f => new
                {
                    UserId = f.UserId,
                    FriendId = f.FriendId,
                    Status = f.Status,
                    CreatedAt = f.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (friendRequest == null)
            {
                // Nếu không tìm thấy yêu cầu kết bạn, trả về trạng thái "none"
                return Json(new
                {
                    status = "success",
                    message = "Kết bạn",
                    friendStatus = "none",
                    friendInfo = new { UserId = userId, FriendId = friendId }
                });
            }

            // Kiểm tra trạng thái của yêu cầu kết bạn
            if (friendRequest.Status == "Friend")
            {
                // Nếu trạng thái là "Friend", trả về "Bạn bè"
                return Json(new
                {
                    status = "success",
                    message = "Bạn bè",
                    friendStatus = "Friend",
                    friendInfo = new { friendRequest.UserId, friendRequest.FriendId, friendRequest.CreatedAt }
                });
            }

            // Nếu yêu cầu kết bạn đang chờ hoặc có trạng thái khác, trả về "Đang chờ"
            return Json(new
            {
                status = "success",
                message = "Đang chờ",
                friendStatus = friendRequest.Status,
                friendInfo = new { friendRequest.UserId, friendRequest.FriendId, friendRequest.CreatedAt }
            });
        }



        [HttpPost]
        public async Task<IActionResult> RemoveFriend(int friendId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Json(new { status = "error", message = "Vui lòng đăng nhập" });
            }

            int userId = int.Parse(userIdStr);

            // Tìm và xóa kết bạn giữa userId và friendId
            var friendRequest = await db.Friends
                .FirstOrDefaultAsync(f => (f.UserId == userId && f.FriendId == friendId)
                                       || (f.UserId == friendId && f.FriendId == userId));

            if (friendRequest == null)
            {
                return Json(new { status = "error", message = "Không tìm thấy kết bạn" });
            }

            db.Friends.Remove(friendRequest);
            await db.SaveChangesAsync();

            return Json(new { status = "success", message = "Đã hủy kết bạn" });
        }

        [HttpPost]
        public async Task<IActionResult> AcceptFriendRequest(int friendId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Json(new { status = "error", message = "Vui lòng đăng nhập" });
            }

            int userId = int.Parse(userIdStr);

            var friendRequest = await db.Friends
                .FirstOrDefaultAsync(f => f.UserId == friendId && f.FriendId == userId && f.Status == "Pending");

            if (friendRequest == null)
            {
                return Json(new { status = "error", message = "Không tìm thấy lời mời kết bạn" });
            }

            try
            {
                friendRequest.Status = "Friend"; // Cập nhật trạng thái thành "Friend"
                db.Friends.Update(friendRequest);
                await db.SaveChangesAsync();

                return Json(new { status = "success", message = "Đã chấp nhận lời mời kết bạn" });
            }
            catch (Exception)
            {
                return Json(new { status = "error", message = "Đã xảy ra lỗi, vui lòng thử lại sau" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeclineFriendRequest(int friendId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Json(new { status = "error", message = "Vui lòng đăng nhập" });
            }

            int userId = int.Parse(userIdStr);

            var friendRequest = await db.Friends
                .FirstOrDefaultAsync(f => f.UserId == friendId && f.FriendId == userId && f.Status == "Pending");

            if (friendRequest == null)
            {
                return Json(new { status = "error", message = "Không tìm thấy lời mời kết bạn" });
            }

            try
            {
                db.Friends.Remove(friendRequest); // Xóa lời mời kết bạn
                await db.SaveChangesAsync();

                return Json(new { status = "success", message = "Đã từ chối lời mời kết bạn" });
            }
            catch (Exception)
            {
                return Json(new { status = "error", message = "Đã xảy ra lỗi, vui lòng thử lại sau" });
            }
        }
    }
}
