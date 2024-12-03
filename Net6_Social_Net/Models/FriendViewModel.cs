using Microsoft.AspNetCore.Mvc;
using Net7_Social_Net.Models;
namespace Net7_Social_Net.Models
{
    public class FriendViewModel
    {
        public int FriendID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
        public string Bio { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
