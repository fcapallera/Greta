using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    public class Address
    {
        [XmlElement("id")]
        public int Id { get; set; }

        //TODO COUNTRY

        [XmlElement("alias")]
        public string Alias { get; set; }

        [XmlElement("lastname")]
        public string LastName { get; set; }

        [XmlElement("firstname")]
        public string FirstName { get; set; }

        [XmlElement("city")]
        public string City { get; set; }
    }
}
