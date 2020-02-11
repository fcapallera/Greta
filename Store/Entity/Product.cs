using AdaptiveCards;
using CoreBot.Utilities;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    public class Product : IImageAttachable
    {
        [XmlElement("id")]
        public override int Id { get; set; }

        [XmlElement("id_default_image")]
        public override Image Image { get; set; }

        [XmlElement("weight")]
        public float Weight { get; set; }
        
        [XmlElement("price")]
        public float Price { get; set; }

        [XmlArray("name")]
        [XmlArrayItem("language", typeof(LanguageTraduction))]
        public List<LanguageTraduction> Name { get; set; }

        [XmlArray("description")]
        [XmlArrayItem("language", typeof(LanguageTraduction))]
        public List<LanguageTraduction> Description { get; set; }

        public string GetNameByLanguage(int language)
        {
            foreach(LanguageTraduction l in Name)
            {
                if (l.Id == language) return l.Text;
            }   

            return "";
        }

        public string GetDescriptionByLanguage(int language)
        {
            foreach (LanguageTraduction l in Description)
            {
                if (l.Id == language) return l.Text;
            }

            return "";
        }

        public override AdaptiveCard ToAdaptiveCard()
        {
            var card = new AdaptiveCard("1.0");
            card.Body.Add(new AdaptiveTextBlock
            {
                Text = GetNameByLanguage(CardUtils.ENGLISH),
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Large,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock(GetDescriptionByLanguage(CardUtils.ENGLISH))
            {
                Wrap = true
            });

            return card;
        }
    }

    public class LanguageTraduction
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
    public class ProductCollection : ImageCarouselable<Product>
    {
        [XmlArray("products")]
        [XmlArrayItem("product", typeof(Product))]
        public List<Product> Products 
        {
            get { return Elements; }
            set { Elements = value; }
        }
    }
}
