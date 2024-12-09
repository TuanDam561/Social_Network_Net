using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Net6_Social_Net.Data;
using Net7_Social_Net.Models;
using System;
using static Net7_Social_Net.ChatHub;

namespace Net7_Social_Net.Controllers
{
    public class MessengerController : BaseController
    {
        private readonly SocialNetworkContext _context;

        public MessengerController(SocialNetworkContext context)
        {
            _context = context;
        }


        public IActionResult Mess()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng về trang Login
                TempData["Error"] = "Vui lòng đăng nhập hoặc đăng ký";
                return RedirectToAction("Index", "Login");
            }

            // Lấy UserID của người dùng hiện tại
            int currentUserId = int.Parse(userId); // Chuyển đổi UserId từ chuỗi sang số

            // Lấy danh sách bạn bè và thời gian tin nhắn mới nhất
            var friendDetails = _context.Friends
                .Where(f => f.UserId == currentUserId || f.FriendId == currentUserId)
                .Select(f => new
                {
                    FriendID = f.UserId == currentUserId ? f.FriendId : f.UserId,
                    Status = f.Status,
                })
                .Join(_context.Users,
                      f => f.FriendID,
                      u => u.UserId,
                      (f, u) => new
                      {
                          FriendID = f.FriendID,
                          Username = u.Username,
                          Email = u.Email,
                          ProfilePicture = u.ProfilePicture,
                          Bio = u.Bio,
                          Status = f.Status,
                          CreatedAt = u.CreatedAt ?? DateTime.Now,
                          LastMessageTime = _context.Messages
                              .Where(m =>
                                  (m.SenderId == currentUserId && m.ReceiverId == f.FriendID) ||
                                  (m.SenderId == f.FriendID && m.ReceiverId == currentUserId))
                              .OrderByDescending(m => m.CreatedAt)
                              .Select(m => (DateTime?)m.CreatedAt)
                              .FirstOrDefault() // Lấy thời gian tin nhắn mới nhất
                      })
                .OrderByDescending(f => f.LastMessageTime ?? DateTime.MinValue) // Sắp xếp theo tin nhắn mới nhất
                .ThenBy(f => f.Username) // Nếu không có tin nhắn, sắp xếp theo Username
                .Select(f => new FriendViewModel
                {
                    FriendID = f.FriendID,
                    Username = f.Username,
                    Email = f.Email,
                    ProfilePicture = f.ProfilePicture,
                    Bio = f.Bio,
                    Status = f.Status,
                    CreatedAt = f.CreatedAt
                })
                .ToList();

            // Truyền danh sách bạn bè vào model
            return View(friendDetails);
        }



        [HttpGet]
        public IActionResult GetMessages(int friendId)
        {
            var userId = HttpContext.Session.GetString("UserId");

            int currentUserId = int.Parse(userId);

            // Lấy tên người bạn từ bảng Users
            var friendName = _context.Users
                .Where(u => u.UserId == friendId)
                .Select(u => u.Username)
                .FirstOrDefault();

            if (friendName == null)
            {
                return NotFound("Friend not found.");
            }

            // Lấy tin nhắn giữa người dùng và người bạn
            var messages = _context.Messages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == friendId) ||
                            (m.SenderId == friendId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageViewModel
                {
                    MessengerId = m.MessageId,
                    SenderID = m.SenderId,
                    ReceiverID = m.ReceiverId,
                    Content = m.Content,
                    SentAt = m.CreatedAt ?? DateTime.Now,
                    Avatar = (m.SenderId == currentUserId) ? GetAvatar(currentUserId) : GetAvatar(friendId),
                    IsSender = (m.SenderId == currentUserId),
                    FriendName = friendName // FriendName đã được lấy ở trên
                })
                .ToList();

            // Trả về danh sách tin nhắn và tên người bạn
            return Json(messages);
        }


        private string GetAvatar(int userId)
        {
            // Trả về đường dẫn Avatar từ bảng Users
            return _context.Users.FirstOrDefault(u => u.UserId == userId)?.ProfilePicture;
        }
    /*    [HttpGet]
        public IActionResult SreachFriend(string friendName)
        {
            if (friendName == null)
            {
                return Json(new { status = "error", message = "Không tìn thấy bạn bè" });
            }
            var friend = _context.Friends.FindAsync(friendName);
            if (friend == null)
            {
                return Json(new { status = "error", message = "Không tìn thấy bạn bè" });
            }
            var friendname = _context.Friends
                .Where(f=>(friendName ==f.friendName));
        }*/
    }
}
