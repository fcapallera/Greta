using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class CardDialog : CancelAndHelpDialog
    {
        private readonly IStatePropertyAccessor<ConversationData> _conversationAccessor;
        public CardDialog(string id, UserState userState, ConversationState conversationState) : base(id, userState)
        {
            _conversationAccessor = conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
        }

        protected async Task<DialogTurnResult> DisableCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var conversationData = await _conversationAccessor.GetAsync(stepContext.Context, () => new ConversationData());

            var jobject = JObject.Parse((string)stepContext.Result);

            var guid = (string)jobject["id"];

            conversationData.DisabledCards.Add(guid, DateTime.Now);

            return await stepContext.NextAsync(stepContext.Result, cancellationToken);
        }
    }
}
