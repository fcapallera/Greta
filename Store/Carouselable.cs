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
        public List<T> Elements { get; set; }

        public Attachment[] ToCarousel()
        {
            List<Attachment> attachments = new List<Attachment>();

            foreach(T element in Elements){
                attachments.Add(element.ToAttachment());
            }

            return attachments.ToArray();
        }

        public Attachment[] ToSelectionCarousel()
        {
            List<Attachment> attachments = new List<Attachment>();
            Guid guid = Guid.NewGuid();

            foreach(T element in Elements)
            {
                var card = element.ToAdaptiveCard();
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

        public T First()
        {
            return Elements.Count > 0 ? Elements[0] : throw new NullReferenceException("This collection is empty");
        }
    }
}
