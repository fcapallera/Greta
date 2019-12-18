using System;
using System.Collections.Generic;

namespace CoreBot.Models
{
    public partial class UserProfile
    {
        public UserProfile()
        {
            Naquestions = new HashSet<Naquestions>();
            OrderLine = new HashSet<OrderLine>();
        }

        public UserProfile(int permission)
        {
            Permission = permission;
            Naquestions = new HashSet<Naquestions>();
            OrderLine = new HashSet<OrderLine>();
        }

        public int Id { get; set; }
        public int? PrestashopId { get; set; }
        public bool Welcomed { get; set; }
        public int Permission { get; set; }
        public DateTime CreationDate { get; set; }

        public ICollection<Naquestions> Naquestions { get; set; }
        public ICollection<OrderLine> OrderLine { get; set; }
    }
}
