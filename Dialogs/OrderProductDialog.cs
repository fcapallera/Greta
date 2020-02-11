using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.Controllers;
using CoreBot.Store;
using CoreBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace CoreBot.Dialogs
{
    public class OrderProductDialog : CardDialog
    {
        private const string addToCartMsg = "Do you want to add the following product to your shopping cart?";
        private const string notAddedMsg = "The order was not added to your shopping cart.";
        private const string addedMsg = "Your order was successfully added to your shopping cart\nIf you want to view your shopping cart, say something like \"Let me see my shopping cart\" or \"I want to check out my order\".";
        private const string chooseMsg = "Choose one of these products from the carousel.";
        private readonly IPrestashopApi PrestashopApi;
        private readonly IConfiguration Configuration;

        public OrderProductDialog(UserController userController, ConversationState conversationState, IPrestashopApi prestashopApi, IConfiguration configuration) 
            : base(nameof(OrderProductDialog),userController,conversationState)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new TextPrompt("TextValidator", ValidateQuantityAsync));
            AddDialog(new TextPrompt("ProductValidator", ValidateProductAsync));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
               {
                   CheckPermissionStepAsync,
                   ProductStepAsync,
                   SelectionCardStepAsync,
                   DisableCardStepAsync,
                   AmountStepAsync,
                   ConfirmAmountStepAsync,
                   AddToCartStepAsync,
               }));

            InitialDialogId = nameof(WaterfallDialog);
            PermissionLevel = PermissionLevels.Representative;
            PrestashopApi = prestashopApi;
            Configuration = configuration;
        }

        private async Task<DialogTurnResult> ProductStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var singleOrder = (SingleOrder)stepContext.Options;

            if (singleOrder.Product == null)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("What product would you want to buy?") }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(singleOrder.Product, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SelectionCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var product = (string)stepContext.Result;

            var products = await PrestashopApi.GetProductByName(product);

            var reply = stepContext.Context.Activity.CreateReply();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = products.ToSelectionCarousel(Configuration);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(chooseMsg), cancellationToken);
            await stepContext.Context.SendActivityAsync(reply,cancellationToken);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("")});
        }

        private async Task<DialogTurnResult> AmountStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var singleOrder = (SingleOrder)stepContext.Options;

            var productId = CardUtils.GetValueFromAction<int>((string)stepContext.Result);

            var product = (await PrestashopApi.GetProductById(productId)).First();

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("How much/many do you want to buy?"),
                RetryPrompt = MessageFactory.Text("Express a quantity or measure, like \"60\", \"10kg\" or \"3L\""),
            };

            if (singleOrder.Quantity == 0)
            {
                return await stepContext.PromptAsync("TextValidator", promptOptions, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ConfirmAmountStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var singleOrder = (SingleOrder)stepContext.Options;

            if (stepContext.Result != null)
            {
                var unprocessedResult = (string)stepContext.Result;

                var matches = Regex.Matches(unprocessedResult, @"^[1-9][0-9]*[A-Za-z]+", RegexOptions.IgnoreCase);

                if (matches.Count > 0)
                {
                    var processedResult = matches[0].Value;
                    var numero = new String(processedResult.TakeWhile(Char.IsDigit).ToArray());
                    singleOrder.Quantity = Convert.ToInt32(numero);
                    singleOrder.Dimension = processedResult.Replace(numero, "");
                }
            }

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), 
                new PromptOptions { Prompt = MessageFactory.Text(addToCartMsg + "\n- " + singleOrder.ToString()) }, cancellationToken);
        }


        private async Task<DialogTurnResult> AddToCartStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //TODO Add Order to Prestashop (esperar GSP)

            /*var singleOrder = (SingleOrder)stepContext.Options;

            if ((bool)stepContext.Result)
            {
                var userProfile = await _profileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

                if (userProfile.ProductCart == null)
                {
                    userProfile.ProductCart = new ProductCart(singleOrder);
                }
                else
                {
                    userProfile.ProductCart.AddOrder(singleOrder);
                }

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(addedMsg),cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(notAddedMsg), cancellationToken);
            }*/

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(whatElse), cancellationToken);

            return await stepContext.EndDialogAsync((bool)stepContext.Result, cancellationToken);
        }

        private async Task<bool> ValidateProductAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var product = promptContext.Context.Activity.Text;
            var collection = await PrestashopApi.GetProductByName(product);
            return await Task.FromResult(collection.Products.Count == 0);
        }

        private Task<bool> ValidateQuantityAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            bool result = false;
            if (int.TryParse(promptContext.Context.Activity.Text, out _))
            {
                result = true;
            }
            else if (Regex.Matches(promptContext.Context.Activity.Text, @"^[1-9][0-9]*[A-Za-z]+", RegexOptions.IgnoreCase).Count > 0)
            {
                result = true;
            }
            return Task.FromResult(result);
        }

    }
}
