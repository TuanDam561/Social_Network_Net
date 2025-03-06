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
            var useId = HttpContext.Session.GetString("UserId");

            if (useId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập hoặc đăng ký";
                return RedirectToAction("Index", "Login");
            }
            int userId = int.Parse(useId);

          

            //lấy dữ liệu nạp vào database
            var model = db.Friends
               .Where(f => (f.UserId == userId || f.FriendId == userId) ||
                           (f.FriendId == userId && f.Status == "Pending"))
              .Join(db.Users,
                f => f.UserId == userId ? f.FriendId : f.UserId,
                u => u.UserId,
                (f, u) => new FriendViewModel
                {
                    FriendID = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    ProfilePicture = u.ProfilePicture,
                    Bio = u.Bio,
                    Status = f.Status,
                    CreatedAt = f.CreatedAt ?? DateTime.MinValue,
                })
               .ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult AcceptRequest(int friendId)
        {
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            // Tìm yêu cầu kết bạn có trạng thái "Pending"
            var friendRequest = db.Friends.FirstOrDefault(f => (f.UserId == userId && f.FriendId == friendId || f.UserId == friendId && f.FriendId == userId) && f.Status == "Pending");

            if (friendRequest != null)
            {
                // Cập nhật trạng thái của yêu cầu kết bạn thành "Friend"
                friendRequest.Status = "Friend";
                db.SaveChanges();

                // Lấy thông tin người bạn
                var user = db.Users.FirstOrDefault(u => u.UserId == (friendRequest.UserId == userId ? friendRequest.FriendId : friendRequest.UserId));

                if (user != null)
                {
                    // Trả về thông tin người bạn đã được xác nhận kết bạn
                    return Json(new
                    {
                        friendId = user.UserId,  // Trả về friendId
                        username = user.Username,
                        profilePicture = user.ProfilePicture,
                        bio = user.Bio
                    });
                }
            }

            return BadRequest("Yêu cầu kết bạn không hợp lệ.");
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

        [HttpPost]
        public IActionResult Unfriend(int friendId)
        {
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            // Tìm bạn bè trong bảng Friends
            var friendship = db.Friends.FirstOrDefault(f =>
                (f.UserId == userId && f.FriendId == friendId && f.Status == "Friend") ||
                (f.UserId == friendId && f.FriendId == userId && f.Status == "Friend"));

            if (friendship != null)
            {
                db.Friends.Remove(friendship);
                db.SaveChanges();
                return Ok(); // Trả về thành công
            }

            return BadRequest(); // Trả về lỗi nếu không tìm thấy
        }

    }
}
