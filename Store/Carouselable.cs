using AdaptiveCards;
using CoreBot.Store.Entity;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CoreBot.Store
{
    /// <summary>
    /// A container whose elements can be displayed in a carousel of attachments.
    /// </summary>
    public class Carouselable<T> where T : IAttachable
    {
        [XmlIgnore]
        public T[] Elements { get; set; }

        public Attachment[] ToCarousel()
        {
            int i = 0;
            List<Attachment> attachments = new List<Attachment>();

            while (i < Elements.Length)
            {
                attachments.Add(Elements[i].ToAttachment());
                i++;
            }

            return attachments.ToArray();
        }

        public Attachment[] ToSelectionCarousel()
        {
            int i = 0;
            List<Attachment> attachments = new List<Attachment>();
            Guid guid = Guid.NewGuid();

            while (i < Elements.Length)
            {
                var card = Elements[i].ToAdaptiveCard();
                card.Actions.Add(new AdaptiveSubmitAction
                {
                    Title = "SELECT",
                    DataJson = $@"{{ ""id"" : ""{guid.ToString()}"", ""action"" : ""{Elements[i].Id}""}}"
                });

                var attachment = new Attachment
                {
                    Content = card,
                    ContentType = "application/vnd.microsoft.card.adaptive"
                };

                attachments.Add(attachment);
                i++;
            }

            return attachments.ToArray();
        }

        public T First()
        {
            return Elements.Length > 0 ? Elements[0] : null;
        }
    }
}
