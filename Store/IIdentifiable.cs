using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store
{
    public abstract class IIdentifiable
    {
        [XmlIgnore]
        public abstract int Id { get; set; }
    }
}
