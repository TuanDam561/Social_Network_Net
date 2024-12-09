using Net7_Social_Net;
namespace Net7_Social_Net.Models
{
    public class MessageViewModel
    { 
        public int MessengerId { get; set; }
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public string Avatar { get; set; } // Avatar của người gửi tin nhắn
        public bool IsSender { get; set; } // Xác định tin nhắn là của người gửi hay người nhận
        public string FriendName { get; set; } // Thuộc tính mới
    }

}
