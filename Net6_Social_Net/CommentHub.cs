namespace Net7_Social_Net
{
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;
    using Net6_Social_Net.Data;

   
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
                var userpost=_context.Posts
                                       .Where(u => u.PostId == postId)
                                       .Select(u => u.UserId)
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
                var commentId = comment.CommentId;
                // Gửi bình luận và tên người dùng qua SignalR đến nhóm tương ứng
                await Clients.Group($"Post-{postId}").SendAsync("ReceiveComment",userId, userpost, postId, userName, content, createdAt, commentId);
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi để kiểm tra
                Console.WriteLine($"Error in SendComment: {ex.Message}");
                throw new InvalidOperationException("Failed to save comment", ex);
            }
        }
       
        public async Task DeleteComment(int commentId, int userId)
        {
            try
            {
                // Tìm bình luận trong cơ sở dữ liệu
                var comment = await _context.Comments.FindAsync(commentId);

                if (comment == null)
                {
                    throw new InvalidOperationException("Bình luận không tồn tại.");
                }

                // Kiểm tra xem userId trong bình luận có khớp với userId trong session không
           /*     if (comment.UserId != userId)
                {
                    // Nếu không khớp, gửi thông báo không cho phép xóa bình luận
                    throw new UnauthorizedAccessException("Bạn không có quyền xóa bình luận này.");
                }*/

                // Xóa bình luận khỏi cơ sở dữ liệu
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();

                // Gửi thông báo xóa bình luận tới tất cả các client
                await Clients.All.SendAsync("CommentDeleted", commentId);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Nếu không có quyền xóa, thông báo lỗi đến client
                await Clients.Caller.SendAsync("DeleteCommentFailed", ex.Message);  // Gửi thông báo lỗi về phía client
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi nếu có sự cố
                Console.WriteLine($"Error in DeleteComment: {ex.Message}");
                throw new InvalidOperationException("Failed to delete comment", ex);
            }
        }


    }

}
