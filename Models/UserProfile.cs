using System;
using System.Collections.Generic;

namespace CoreBot.Models
{
    public partial class UserProfile
    {
        public UserProfile()
        {
            Cart = new HashSet<Cart>();
            Naquestions = new HashSet<Naquestions>();
        }

        public int Id { get; set; }
        public string BotUserId { get; set; }
        public int? PrestashopId { get; set; }
        public bool Validated { get; set; }
        public int Permission { get; set; }
        public DateTime CreationDate { get; set; }

        public ICollection<Cart> Cart { get; set; }
        public ICollection<Naquestions> Naquestions { get; set; }
    }
}
