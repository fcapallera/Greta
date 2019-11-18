using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    [XmlRoot("prestashop")]
    public class Product
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("id_manufacturer")]
        public int ManufacturerId { get; set; }

        [XmlAttribute("id_supplier")]
        public int SupplierId { get; set; }
    }
}
