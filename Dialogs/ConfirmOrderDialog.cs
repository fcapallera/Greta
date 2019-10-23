using AdaptiveCards;
using CoreBot.Store;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
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

            var attachment = CreateCardFromOrder(userProfile);

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

        private Attachment CreateCardFromOrder(UserProfile userProfile)
        {
            string[] paths = { ".", "Cards", "confirmOrderCard.json" };
            var cardJson = File.ReadAllText(Path.Combine(paths));
            var card = AdaptiveCard.FromJson(cardJson).Card;

            //Ara hem convertit el JSON a un AdaptiveCard i editarem els fragments que ens interessen.

            //Primer editem el FactSet (informació de l'usuari que sortirà a la fitxa).
            var containerFact = (card.Body[1] as AdaptiveContainer);
            var factSet = (containerFact.Items[1] as AdaptiveFactSet);
            factSet.Facts.Add(new AdaptiveFact("Ordered by:", userProfile.Name));
            factSet.Facts.Add(new AdaptiveFact("Company:", userProfile.Company));

            //Ara editarem la informació que sortirà dels productes
            var containerProducts = (card.Body[3] as AdaptiveContainer);

            userProfile.ProductCart.Products.RemoveAll(item => item == null);

            foreach (SingleOrder order in userProfile.ProductCart.Products)
            {
                AdaptiveColumnSet columns = new AdaptiveColumnSet();
                AdaptiveColumn productColumn = new AdaptiveColumn();

                AdaptiveTextBlock product = new AdaptiveTextBlock(order.Product);
                product.Wrap = true;

                productColumn.Width = "stretch";
                productColumn.Items.Add(product);
                columns.Columns.Add(productColumn);

                AdaptiveColumn amountColumn = new AdaptiveColumn();

                AdaptiveTextBlock amount = new AdaptiveTextBlock(order.AmountToString());
                amount.Wrap = true;

                amountColumn.Width = "auto";
                amountColumn.Items.Add(amount);
                columns.Columns.Add(amountColumn);

                containerProducts.Items.Add(columns);
            }

            var attachment = new Attachment()
            {
                Content = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(card)),
                ContentType = "application/vnd.microsoft.card.adaptive"
            };

            return attachment;
        }
    }
}
