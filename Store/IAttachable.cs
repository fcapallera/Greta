using AdaptiveCards;
using CoreBot.Utilities;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Store.Entity
{
    public abstract class IAttachable
    {
        public Attachment ToAttachment()
        {
            return CardUtils.AdaptiveCardToAttachment(ToAdaptiveCard());
        }

        public abstract AdaptiveCard ToAdaptiveCard();
    }
}
