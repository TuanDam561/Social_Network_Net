namespace Net7_Social_Net.Models
{
    public class CommentViewModel
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } // Tên người dùng của bình luận
        public string AvatarUser { get; set; } // Ảnh đại diện của người dùng
    }
}
