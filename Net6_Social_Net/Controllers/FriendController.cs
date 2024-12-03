using Microsoft.AspNetCore.Mvc;
using Net6_Social_Net.Data;
using Net7_Social_Net.Models;
namespace Net7_Social_Net.Controllers
{
    public class FriendController : Controller
    {
        private readonly SocialNetworkContext db;

        public FriendController(SocialNetworkContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            // Lấy UserId từ Session
            var userIdss = HttpContext.Session.GetString("UserId");
            if (userIdss == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng về trang Login
                TempData["Error"] = "Vui lòng đăng nhập hoặc đăng ký";
                return RedirectToAction("Index", "Login");
            }
            // Lấy userId từ session
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            if (userId == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng về trang Login
                TempData["Error"] = "Vui lòng đăng nhập hoặc đăng ký";
                return RedirectToAction("Index", "Login");
            }

            // Lấy các yêu cầu kết bạn chỉ khi FriendID là userId hiện tại
            var pendingRequests = db.Friends
                .Where(f => f.FriendId == userId && f.Status == "Pending")
                .Select(f => new
                {
                    FriendID = f.UserId,
                    Status = f.Status,
                    CreatedAt = f.CreatedAt
                })
                .ToList();

            // Lấy thông tin chi tiết về lời mời kết bạn
            var pendingRequestsDetails = pendingRequests.Select(pf => new FriendViewModel
            {
                FriendID = pf.FriendID,
                Username = db.Users.Where(u => u.UserId == pf.FriendID).Select(u => u.Username).FirstOrDefault()!,
                Email = db.Users.Where(u => u.UserId == pf.FriendID).Select(u => u.Email).FirstOrDefault()!,
                ProfilePicture = db.Users.Where(u => u.UserId == pf.FriendID).Select(u => u.ProfilePicture).FirstOrDefault()!,
                Bio = db.Users.Where(u => u.UserId == pf.FriendID).Select(u => u.Bio).FirstOrDefault()!,
                Status = pf.Status,
                CreatedAt = pf.CreatedAt ?? DateTime.MinValue
            }).ToList();

            // Lấy danh sách bạn bè đã kết bạn
            var friends = db.Friends
                .Where(f => (f.UserId == userId || f.FriendId == userId) && f.Status == "Friend")
                .Select(f => new
                {
                    FriendID = f.UserId == userId ? f.FriendId : f.UserId,
                    Status = f.Status
                })
                .ToList();

            var friendsDetails = friends.Select(f => new FriendViewModel
            {
                FriendID = f.FriendID,
                Username = db.Users.Where(u => u.UserId == f.FriendID).Select(u => u.Username).FirstOrDefault(),
                Email = db.Users.Where(u => u.UserId == f.FriendID).Select(u => u.Email).FirstOrDefault(),
                ProfilePicture = db.Users.Where(u => u.UserId == f.FriendID).Select(u => u.ProfilePicture).FirstOrDefault(),
                Bio = db.Users.Where(u => u.UserId == f.FriendID).Select(u => u.Bio).FirstOrDefault(),
                Status = f.Status
            }).ToList();

            // Gộp danh sách bạn bè và lời mời kết bạn
            var model = friendsDetails.Concat(pendingRequestsDetails).ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult AcceptRequest(int friendId)
        {
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            // Cập nhật trạng thái kết bạn
            var friendRequest = db.Friends.FirstOrDefault(f => f.FriendId == userId && f.UserId == friendId && f.Status == "Pending");
            if (friendRequest != null)
            {
                friendRequest.Status = "Friend";
                db.SaveChanges();

                // Lấy thông tin bạn bè
                var user = db.Users.FirstOrDefault(u => u.UserId == friendId);
                return Json(new
                {
                    username = user?.Username,
                    profilePicture = user?.ProfilePicture,
                    bio = user?.Bio
                });
            }

            return BadRequest();
        }

        [HttpPost]
        public IActionResult DeclineRequest(int friendId)
        {
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            // Xóa yêu cầu kết bạn
            var friendRequest = db.Friends.FirstOrDefault(f => f.FriendId == userId && f.UserId == friendId && f.Status == "Pending");
            if (friendRequest != null)
            {
                db.Friends.Remove(friendRequest);
                db.SaveChanges();
                return Ok();
            }

            return BadRequest();
        }
    }
}
