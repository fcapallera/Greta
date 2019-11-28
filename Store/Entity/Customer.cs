using System;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    public class Customer
    {
        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("passwd")]
        public string Password { get; set; }

        [XmlElement("lastname")]
        public string LastName { get; set; }

        [XmlElement("firstname")]
        public string FirstName { get; set; }

        [XmlElement("email")]
        public string Email { get; set; }

        [XmlElement("company")]
        public string Company { get; set; }

        [XmlIgnore]
        public DateTime UpdateDate { get; set; }

        [XmlElement("date_upd")]
        public string UpdateDateString
        {
            get { return this.UpdateDate.ToString("yyyy-MM-dd HH:mm:ss"); }
            set { this.UpdateDate = DateTime.Parse(value); }
        }
    }
}
