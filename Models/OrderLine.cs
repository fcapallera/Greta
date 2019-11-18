using System;
using System.Collections.Generic;

namespace CoreBot.Models
{
    public partial class OrderLine
    {
        public int Id { get; set; }
        public int Product { get; set; }
        public int Amount { get; set; }
        public int UserId { get; set; }

        public UserProfile User { get; set; }
    }
}
