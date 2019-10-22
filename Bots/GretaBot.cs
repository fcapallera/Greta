using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

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
                activity.Text = JsonConvert.SerializeObject(activity.Value);
            }

            // Run the MainDialog, dispatch intent or continue current dialog.
            await Dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);

            // Save any changes on the User/Conversation State.
            await _userState.SaveChangesAsync(turnContext);
            await _conversationState.SaveChangesAsync(turnContext);
        }
    }
}
