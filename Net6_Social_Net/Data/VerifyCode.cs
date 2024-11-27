using System;
using System.Collections.Generic;

namespace Net6_Social_Net.Data
{
    public partial class VerifyCode
    {
        public int VerifyId { get; set; }
        public string Gmail { get; set; } = null!;
        public string VerifyCode1 { get; set; } = null!;
        public bool? IsVerify { get; set; }
        public DateTime? SendDate { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
