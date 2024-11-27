using System;
using System.Collections.Generic;

namespace Net6_Social_Net.Data
{
    public partial class UserSetting
    {
        public int SettingId { get; set; }
        public int UserId { get; set; }
        public string? PrivacySetting { get; set; }
        public bool? NotificationSetting { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
