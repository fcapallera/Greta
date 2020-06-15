using CoreBot.Controllers;
using CoreBot.Extensions;
using CoreBot.Store;
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
    public class ConfirmOrderDialog : CardDialog
    {
        private const string ACTION = "action";
        private const string noOrderMsg = "Sorry, you need to order something first.";
        private const string ignoreOrder = "Your order is still pending, you can confirm, cancel or add more products!";
        private readonly PurchaseController PurchaseController;
        private readonly IPrestashopApi PrestashopApi;

        public ConfirmOrderDialog(UserController userController, ConversationState conversationState, 
            PurchaseController purchaseController, IPrestashopApi prestashopApi) : base(nameof(ConfirmOrderDialog),userController, conversationState)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt),CardJsonValidator));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CheckPermissionStepAsync,
                CheckOrderStepAsync,
                ShowCardStepAsync,
                DisableCardStepAsync,
                ProcessValueStepAsync,
                FinalStepAsync
            }));

            PermissionLevel = PermissionLevels.Representative;
            InitialDialogId = nameof(WaterfallDialog);
            PurchaseController = purchaseController;
            PrestashopApi = prestashopApi;
        }

        private async Task<DialogTurnResult> CheckOrderStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var activeCart = await PurchaseController.GetActiveCartFromUser(stepContext.Context.Activity.From.Id);

            if (activeCart == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(noOrderMsg), cancellationToken);
                return await stepContext.EndDialogAsync();
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> ShowCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var activeCart = await PurchaseController.GetActiveCartFromUser(stepContext.Context.Activity.From.Id);

            var attachment = await activeCart.ToAdaptiveCard(PrestashopApi);
            var activity = new Activity
            {
                Attachments = new List<Attachment>() { attachment },
                Type = ActivityTypes.Message
            };

            var opts = new PromptOptions
            {
                Prompt = activity,
                RetryPrompt = activity
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), opts);
        }

        private async Task<DialogTurnResult> ProcessValueStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var action = CardUtils.GetValueFromAction<string>((string)stepContext.Result);
            stepContext.Values[ACTION] = action;

            switch (action)
            {
                case "confirmOrder":
                case "cancelOrder":
                    var msg = action.Replace("Order", "");

                    return await stepContext.PromptAsync(nameof(ConfirmPrompt),
                        new PromptOptions { Prompt = MessageFactory.Text("Are you sure you want to " + msg + " this order?") }, cancellationToken);

                default:
                    return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }
        
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string action = stepContext.GetValue<string>(ACTION);

            if ((bool)stepContext.Result)
            {
                var userId = stepContext.Context.Activity.From.Id;

                if (action == "confirmOrder")
                {
                    var cart = await PurchaseController.GetActiveCartFromUser(userId);
                    //var order = await Store.Entity.Order.BuildOrderAsync(cart, PrestashopApi);

                    //await PrestashopApi.PostOrder(order);
                    
                }

                await PurchaseController.InactivateCartFromUser(userId);

                string filling = action == "confirmOrder" ? "sent" : "cancelled";

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your order was " + filling + " successfully!"), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(ignoreOrder), cancellationToken);
            }
            
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

    }
}
