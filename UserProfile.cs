using CoreBot.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot
{
    public class UserProfile
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public ProductCart ProductCart { get; set; }
        public bool AskedForUserInfo { get; set; } = false;
        public string Company { get; set; }

        public int Permission { get; set; } = 5;
    }
}
