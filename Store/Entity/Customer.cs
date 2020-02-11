using AdaptiveCards;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    public class Customer : IAttachable, IEquatable<Customer>
    {
        [XmlElement("id")]
        public override int Id { get; set; }

        [XmlElement("id_lang")]
        public CustomerLanguage Language { get; set; }

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
            get { return UpdateDate.ToString("yyyy-MM-dd HH:mm:ss"); }
            set { UpdateDate = DateTime.Parse(value); }
        }

        public bool Equals(Customer other)
        {
            return Id == other.Id;
        }

        public override AdaptiveCard ToAdaptiveCard()
        {
            var card = new AdaptiveCard("1.0");
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = FirstName + " " + LastName,
                Weight = AdaptiveTextWeight.Bolder
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Company: {Company}\n\n Email: {Email}"
            });

            return card;
        }

        public string GetFullName()
        {
            return FirstName + " " + LastName;
        }
    }

    public class CustomerLanguage
    {
        [XmlText]
        public int Id { get; set; }

        [XmlAttribute("href", Namespace = "http://www.w3.org/1999/xlink")]
        public string Url { get; set; }
    }

    [XmlRoot("prestashop")]
    public class CustomerCollection : Carouselable<Customer>
    {
        [XmlArray("customers")]
        [XmlArrayItem("customer", typeof(Customer))]
        public List<Customer> Customers 
        {
            get { return Elements; }
            set { Elements = value; }
        }
    }
}
