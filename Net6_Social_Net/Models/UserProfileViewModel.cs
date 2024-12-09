using Net7_Social_Net.Models;
using System.Collections.Generic;

namespace Net6_Social_Net.Models
{
    public class UserProfileViewModel
    {
        public AccountModel User { get; set; }
        public List<PostViewModel> Posts { get; set; }
        public List<FriendViewModel> Friends { get; set; }
    }
}
