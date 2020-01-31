using System.Collections.Concurrent;
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
    public class GretaBot<T> : ActivityHandler
        where T : Dialog
    {
        private readonly Dialog Dialog;
        private readonly BotState _conversationState;
        private readonly BotState _userState;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private const string askingMsg = "If you want me to perform an action for you just ask it straight forward. You can say things like:\n- I want to log in.\n- I want to order vitrocool.\n- Show me my shopping card.\n- What products can I buy?";
        private const string cancelMsg = "Once we start a conversation (let's say you want to order something and I start asking things) and you want to cancel, you can say something like **cancel** or **quit** to exit the current operation.";
        private const string helpMsg = "Also, if you don't know what's happening just type **?** or **help** and I will try to explain what is going on!";

        public GretaBot(ConversationState conversationState, UserState userState, T dialog,
            ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _conversationState = conversationState;
            _userState = userState;
            _conversationReferences = conversationReferences;
            Dialog = dialog;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;

            AddConversationReference(activity);

            // If the message comes from an Adaptive Card we serialize the answer and send it as a message.
            if(string.IsNullOrWhiteSpace(activity.Text) && activity.Value != null)
            {
                var _conversationAccessor = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
                var conversationData = await _conversationAccessor.GetAsync(turnContext, () => new ConversationData());

                var jobject = (JObject)activity.Value;

                if (jobject["id"].ToString().StartsWith("Greta"))
                    await SendGretaHelpMessageAsync(turnContext);

                else if (conversationData.DisabledCards.ContainsKey((string)jobject["id"]))
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

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }


        public static async Task SendGretaHelpMessageAsync(ITurnContext turnContext)
        {
            var json = (JObject)turnContext.Activity.Value;
            var code = (string)json["id"];

            switch (code)
            {
                case "GretaMoreInfo":
                    await turnContext.SendActivityAsync(askingMsg);
                    await turnContext.SendActivityAsync(cancelMsg);
                    await turnContext.SendActivityAsync(helpMsg);
                    break;

                case "GretaActions":
                    await turnContext.SendActivityAsync("TO_DO");
                    break;
            }
        }
    }
}
