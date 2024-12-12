namespace Net7_Social_Net
{
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;
    using Net6_Social_Net.Data;
    using System.Threading.Tasks;

    public class ChatHub : Hub
    {
        private readonly SocialNetworkContext _context;

         public ChatHub(SocialNetworkContext context)
          {
              _context = context;
          }

        public async Task SendMessage(int senderId, int receiverId, string message)
        {
            // Kiểm tra người gửi
            var sender = await _context.Users
                .Where(u => u.UserId == senderId)
                .Select(u => new { u.Username, u.ProfilePicture })
                .FirstOrDefaultAsync();

            if (sender == null)
            {
                throw new Exception("Người gửi không tồn tại.");
            }

            // Kiểm tra người nhận
            var receiver = await _context.Users
                .Where(u => u.UserId == receiverId)
                .FirstOrDefaultAsync();

            if (receiver == null)
            {
                throw new Exception("Người nhận không tồn tại.");
            }

            // Tạo một messengerId duy nhất cho tin nhắn
            var messengerId = Guid.NewGuid().ToString();

            // Lưu tin nhắn vào cơ sở dữ liệu và lấy MessageID
            var newMessage = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            // Lấy MessageId sau khi lưu tin nhắn
            var messagerId = newMessage.MessageId;

            // Gửi tin nhắn đến group của người nhận
            await Clients.Group(receiverId.ToString())
                .SendAsync("ReceiveMessage", senderId, sender.ProfilePicture, message, messagerId);

            // Gửi tin nhắn đến group của người gửi để cập nhật UI
            await Clients.Group(senderId.ToString())
            .SendAsync("MyReceiveMessage", senderId, sender.ProfilePicture, message, messagerId);

        }

        public async Task SendNotificationToFriends(string senderId, string receiverId)
        {
            await Clients.All.SendAsync("NotifyNewMessage", senderId, receiverId);
        }

        public async Task DeleteMessage(int messengerId)
        {
            // Tìm tin nhắn trong cơ sở dữ liệu
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.MessageId == messengerId);

            if (message == null)
            {
                throw new Exception("Không tìm thấy tin nhắn.");
            }

            // Lưu thông tin người gửi và người nhận
            var senderId = message.SenderId;
            var receiverId = message.ReceiverId;

            // Xóa tin nhắn khỏi cơ sở dữ liệu
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            // Gửi sự kiện tới cả người gửi và người nhận
            await Clients.Group(senderId.ToString()).SendAsync("MessageDeleted", messengerId);
            await Clients.Group(receiverId.ToString()).SendAsync("MessageDeleted", messengerId);
        }


        // Khi client kết nối, liên kết với userId trong group SignalR
        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext().Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                // Đảm bảo chỉ thêm ConnectionId vào group của chính user
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }


        // Khi client ngắt kết nối, rời khỏi nhóm SignalR
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.GetHttpContext().Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }



}
