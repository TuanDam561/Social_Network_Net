using System;
using System.Collections.Generic;

namespace Net6_Social_Net.Data
{
    public partial class User
    {
        public User()
        {
            Comments = new HashSet<Comment>();
            Events = new HashSet<Event>();
            FriendFriendNavigations = new HashSet<Friend>();
            FriendUsers = new HashSet<Friend>();
            MessageReceivers = new HashSet<Message>();
            MessageSenders = new HashSet<Message>();
            Notifications = new HashSet<Notification>();
            Posts = new HashSet<Post>();
            SearchHistories = new HashSet<SearchHistory>();
            UserSettings = new HashSet<UserSetting>();
        }

        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? Imagebanner { get; set; }
        public string? Role { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Event> Events { get; set; }
        public virtual ICollection<Friend> FriendFriendNavigations { get; set; }
        public virtual ICollection<Friend> FriendUsers { get; set; }
        public virtual ICollection<Message> MessageReceivers { get; set; }
        public virtual ICollection<Message> MessageSenders { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<SearchHistory> SearchHistories { get; set; }
        public virtual ICollection<UserSetting> UserSettings { get; set; }
    }
}
