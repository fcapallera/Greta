using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    public class Product
    {
        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("id_default_image")]
        public Image Image { get; set; }

        [XmlArray("name")]
        [XmlArrayItem("language", typeof(Language))]
        public Language[] Name { get; set; }

        [XmlArray("description")]
        [XmlArrayItem("language", typeof(Language))]
        public Language[] Description { get; set; }

        public string GetNameByLanguage(int language)
        {
            foreach(Language l in Name)
            {
                if (l.Id == language) return l.Text;
            }

            return "";
        }

        public string GetDescriptionByLanguage(int language)
        {
            foreach (Language l in Description)
            {
                if (l.Id == language) return l.Text;
            }

            return "";
        }
    }

    public class Language
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("href", Namespace ="http://www.w3.org/1999/xlink")]
        public string Url { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    public class Image
    {
        [XmlAttribute("href", Namespace = "http://www.w3.org/1999/xlink")]
        public string Url { get; set; }

        [XmlAttribute("notFilterable")]
        public bool Filterable { get; set; }

        [XmlText]
        public string ImageId { get; set; }
    }

    [XmlRoot("prestashop")]
    public class ProductCollection
    {
        [XmlArray("products")]
        [XmlArrayItem("product", typeof(Product))]
        public Product[] Products { get; set; }
    }
}
