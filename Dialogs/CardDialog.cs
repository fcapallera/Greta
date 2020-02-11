using CoreBot.Controllers;
using CoreBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class CardDialog : CancelAndHelpDialog
    {
        private readonly IStatePropertyAccessor<ConversationData> _conversationAccessor;
 
        public CardDialog(string id, UserController userController, ConversationState conversationState)
            : base(id,userController)
        {
            _conversationAccessor = conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
        }

        protected async Task<DialogTurnResult> DisableCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var conversationData = await _conversationAccessor.GetAsync(stepContext.Context, () => new ConversationData());

            var result = (string)stepContext.Result;
            if (result.StartsWith('{'))
            {
                var guid = CardUtils.GetGuidFromResult(result);
                conversationData.DisabledCards.Add(guid, DateTime.Now);
            }

            return await stepContext.NextAsync(stepContext.Result, cancellationToken);
        }
    }
}
