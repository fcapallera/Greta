using CoreBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class ConfirmOrderDialog : CancelAndHelpDialog
    {
        private const string noOrderMsg = "Sorry, you need to order something first.";
        private const string ignoreOrder = "Your order is still pending, you can confirm, cancel or add more products!";

        public ConfirmOrderDialog(UserState userState) : base(nameof(ConfirmOrderDialog),userState)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CheckPermissionStepAsync,
                CheckOrderStepAsync,
                ShowCardStepAsync,
                ProcessValueStepAsync,
                FinalStepAsync
            }));

            PermissionLevel = 3;
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> CheckOrderStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _profileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            if (userProfile.ProductCart == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(noOrderMsg), cancellationToken);
                return await stepContext.EndDialogAsync();
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> ShowCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _profileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            var attachment = CardUtils.CreateCardFromOrder(userProfile);

            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Attachments = new List<Attachment>() { attachment },
                    Type = ActivityTypes.Message
                }
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), opts);
        }

        private async Task<DialogTurnResult> ProcessValueStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var jobject = JObject.Parse((string)stepContext.Result);
            var action = (string)jobject["action"];
            stepContext.Values["action"] = action;

            switch (action)
            {
                case "confirmOrder":
                case "cancelOrder":
                    var msg = action.Replace("Order", "");

                    return await stepContext.PromptAsync(nameof(ConfirmPrompt),
                        new PromptOptions { Prompt = MessageFactory.Text("Are you sure you want to " + msg + " this order?") }, cancellationToken);

                default:
                    await stepContext.Context.SendActivityAsync("TODO: Implement cathing text prompt redirection.");
                    return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }
        
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string action = (string)stepContext.Values["action"];

            if ((bool)stepContext.Result)
            {
                var filling = action == "confirmOrder" ? " sent to us " : " cancelled ";

                var userProfile = await _profileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

                userProfile.ProductCart = null;

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your order was" + filling + "successfully!"), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(ignoreOrder), cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(whatElse), cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
