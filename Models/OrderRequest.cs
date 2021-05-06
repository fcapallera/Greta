using System;
using System.Collections.Generic;

namespace CoreBot.Models
{
    public partial class OrderRequest
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public bool Confirmed { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ConfirmationDate { get; set; }

        public Cart Cart { get; set; }
    }
}
