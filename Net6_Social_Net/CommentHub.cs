namespace Net7_Social_Net
{
    using Microsoft.AspNetCore.SignalR;
    using Net6_Social_Net.Data;

    /* public class CommentHub : Hub
     {
         private readonly SocialNetworkContext _context;

         public CommentHub(SocialNetworkContext context)
         {
             _context = context;
         }

         public async Task SendComment(int postId, int userId, string content, string createdAt)
         {
             try
     {
                 // Lấy tên người dùng từ userId
                 var userName = _context.Users
                                        .Where(u => u.UserId == userId)
                                        .Select(u => u.Username)
                                        .FirstOrDefault();

                 if (userName == null)
                 {
                     // Nếu không tìm thấy người dùng, có thể throw lỗi hoặc sử dụng tên mặc định
                     userName = "Người dùng";
                 }

                 // Lưu bìnhvar comment = new Comment
                 var comment = new Comment
                 {
                     PostId = postId,
                     UserId = userId,
                     Content = content,
                     CreatedAt = DateTime.Parse(createdAt)
                 };

                 _context.Comments.Add(comment);
                 await _context.SaveChangesAsync();

                 // Gửi bình luận và tên người dùng qua SignalR
                 await Clients.All.SendAsync("ReceiveComment", postId, userName, content, createdAt);
             }
             catch (Exception ex)
             {
                 // Ghi lại lỗi để kiểm tra
                 Console.WriteLine($"Error in SendComment: {ex.Message}");
                 throw new InvalidOperationException("Failed to save comment", ex);
             }
         }


     }*/
    public class CommentHub : Hub
    {
        private readonly SocialNetworkContext _context;

        public CommentHub(SocialNetworkContext context)
        {
            _context = context;
        }

        public async Task JoinPostGroup(int postId)
        {
            // Thêm client vào nhóm dựa trên postId
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Post-{postId}");
        }

        public async Task LeavePostGroup(int postId)
        {
            // Loại client khỏi nhóm dựa trên postId
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Post-{postId}");
        }

        public async Task SendComment(int postId, int userId, string content, string createdAt)
        {
            try
            {
                // Lấy tên người dùng từ userId
                var userName = _context.Users
                                       .Where(u => u.UserId == userId)
                                       .Select(u => u.Username)
                                       .FirstOrDefault();

                if (userName == null)
                {
                    // Nếu không tìm thấy người dùng, sử dụng tên mặc định
                    userName = "Người dùng";
                }

                // Lưu bình luận vào cơ sở dữ liệu
                var comment = new Comment
                {
                    PostId = postId,
                    UserId = userId,
                    Content = content,
                    CreatedAt = DateTime.Parse(createdAt)
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                // Gửi bình luận và tên người dùng qua SignalR đến nhóm tương ứng
                await Clients.Group($"Post-{postId}").SendAsync("ReceiveComment", postId, userName, content, createdAt);
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi để kiểm tra
                Console.WriteLine($"Error in SendComment: {ex.Message}");
                throw new InvalidOperationException("Failed to save comment", ex);
            }
        }
    }

}
