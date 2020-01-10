using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    [XmlRoot("prestashop")]
    public class Language
    {
        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("iso_code")]
        public string IsoCode { get; set; }

        [XmlElement("locale")]
        public string Locale { get; set; }

        [XmlElement("language_code")]
        public string LanguageCode { get; set; }

        [XmlElement("active")]
        public byte IsActive { get; set; }
    }
}
