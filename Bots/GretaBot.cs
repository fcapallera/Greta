using System.Threading;
using System.Threading.Tasks;
using CoreBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreBot.Bots
{
    public class GretaBot<T> : IBot
        where T : Dialog
    {
        private readonly Dialog Dialog;
        private readonly BotState _conversationState;
        private readonly BotState _userState;

        public GretaBot(ConversationState conversationState, UserState userState, T dialog)
        {
            _conversationState = conversationState;
            _userState = userState;
            Dialog = dialog;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;

            // If the message comes from an Adaptive Card we serialize the answer and send it as a message.
            if(string.IsNullOrWhiteSpace(activity.Text) && activity.Value != null)
            {
                var _conversationAccessor = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
                var conversationData = await _conversationAccessor.GetAsync(turnContext, () => new ConversationData());

                var jobject = (JObject)activity.Value;

                if (conversationData.DisabledCards.ContainsKey((string)jobject["id"]))
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(CardUtils.sameCardMsg), cancellationToken);
                }
                else
                {
                    activity.Text = JsonConvert.SerializeObject(activity.Value);
                    await Dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                }
            }
            else
            {
                await Dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
            }

            // Save any changes on the User/Conversation State.
            await _userState.SaveChangesAsync(turnContext);
            await _conversationState.SaveChangesAsync(turnContext);
        }
    }
}
