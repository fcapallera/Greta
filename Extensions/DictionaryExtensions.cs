using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Extensions
{
    public static class DictionaryExtensions
    {
        public static T Get<T>(this Dictionary<string,object> instance, string name)
        // Mètode per accedir a les stepContext options que es passen entre step i step al Waterfall
        {
            return (T)instance[name];
        }
    }
}
