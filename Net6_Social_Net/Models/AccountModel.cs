namespace Net6_Social_Net.Models
{
    public class AccountModel
    {
        public int UserId { get; set; }

        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? ProfilePicture { get; set; }

        public string? Bio { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? Imagebanner { get; set; }

        public string? Role { get; set; }
    }
}
