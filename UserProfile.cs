using CoreBot.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot
{
    public class UserProfile
    {
        public string name { get; set; }
        public string username { get; set; }
        public ProductCart productCart { get; set; }
        public bool askedForUserInfo { get; set; } = false;
        public string company { get; set; }
    }
}
