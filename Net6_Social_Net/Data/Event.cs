using System;
using System.Collections.Generic;

namespace Net6_Social_Net.Data
{
    public partial class Event
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string EventTitle { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
