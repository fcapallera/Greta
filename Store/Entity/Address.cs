using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    public class Address : IIdentifiable
    {
        public Address() { }

        [XmlElement("id")]
        public override int Id { get; set; }

        [XmlElement("id_country")]
        public int CountryId { get; set; }

        [XmlElement("alias")]
        public string Alias { get; set; }

        [XmlElement("lastname")]
        public string LastName { get; set; }

        [XmlElement("firstname")]
        public string FirstName { get; set; }

        [XmlElement("address1")]
        public string Address1 { get; set; }

        [XmlElement("city")]
        public string City { get; set; }
    }

    [XmlRoot("prestashop")]
    public class AddressCollection
    {
        [XmlArray("addresses")]
        [XmlArrayItem("address", typeof(Address))]
        public List<Address> Addresses { get; set; }

        public Address First()
        {
            return Addresses.FirstOrDefault();
        }
    }

}
