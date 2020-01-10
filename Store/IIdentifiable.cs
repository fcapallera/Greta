using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Store
{
    public abstract class IIdentifiable
    {
        public abstract int Id { get; set; }
    }
}
