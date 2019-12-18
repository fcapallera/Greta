using AdaptiveCards;
using CoreBot.Utilities;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    /// <summary>
    /// Represents an object that can be rendered as an Adaptive attachment.
    /// </summary>
    public abstract class IAttachable
    {
        [XmlIgnore]
        public abstract int Id { get; set; }
        public Attachment ToAttachment()
        {
            return CardUtils.AdaptiveCardToAttachment(ToAdaptiveCard());
        }

        public abstract AdaptiveCard ToAdaptiveCard();
    }
}
