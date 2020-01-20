using AdaptiveCards;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store
{
    /// <summary>
    /// A container whose elements can be displayed in a carousel of attachments which get their images from Prestashop Urls.
    /// </summary>
    public class ImageCarouselable<T> : Carouselable<T> where T : IImageAttachable
    {
        public Attachment[] ToSelectionCarousel(IConfiguration configuration)
        {
            List<Attachment> attachments = new List<Attachment>();
            Guid guid = Guid.NewGuid();

            foreach(T element in Elements)
            {
                var card = element.ToImageAdaptiveCard(configuration);
                card.Actions.Add(new AdaptiveSubmitAction
                {
                    Title = "SELECT",
                    DataJson = $@"{{ ""id"" : ""{guid.ToString()}"", ""action"" : ""{element.Id}""}}"
                });

                var attachment = new Attachment
                {
                    Content = card,
                    ContentType = "application/vnd.microsoft.card.adaptive"
                };

                attachments.Add(attachment);
            }

            return attachments.ToArray();
        }
    }
}
