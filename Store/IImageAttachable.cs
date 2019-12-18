using AdaptiveCards;
using CoreBot.Store.Entity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store
{
    public abstract class IImageAttachable : IAttachable
    {
        [XmlIgnore]
        public abstract Image Image { get; set; }

        public AdaptiveCard ToImageAdaptiveCard(IConfiguration configuration)
        {
            var card = ToAdaptiveCard();
            var apiKey = configuration.GetSection("PrestashopSettings").GetSection("ApiKey").Value;

            card.Body.Insert(0,new AdaptiveImage
            {
                Url = new Uri(Image.Url +$"?ws_key={apiKey}")
            });

            return card;
        }
    }

}
