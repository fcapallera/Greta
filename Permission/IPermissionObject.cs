using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Interfaces
{
    interface IPermissionObject
    {
        int PermissionLevel { get; set; }

        bool HasPermission(int permission);
    }
}
