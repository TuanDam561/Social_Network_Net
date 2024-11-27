
using Net7_Social_Net.Models;
using System.Runtime.CompilerServices;

namespace Net6_Social_Net.Models
{
    public class PostViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UserName { get; set; } // Thêm trường này
        public string Status { get; set; }  
        public string AvatarUser { get; set; }
        public List<CommentViewModel> Comments { get; set; } // Danh sách bình luận
    }
}
