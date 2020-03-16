using System;
using System.Collections.Generic;

namespace CoreBot.Models
{
    public partial class OrderLine
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Amount { get; set; }

        public Cart Cart { get; set; }
    }
}
