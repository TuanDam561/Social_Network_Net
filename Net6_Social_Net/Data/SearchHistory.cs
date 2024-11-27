using System;
using System.Collections.Generic;

namespace Net6_Social_Net.Data
{
    public partial class SearchHistory
    {
        public int SearchId { get; set; }
        public int UserId { get; set; }
        public string Query { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
