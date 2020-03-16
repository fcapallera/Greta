using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.Controllers;
using CoreBot.Extensions;
using CoreBot.Models;
using CoreBot.Store;
using CoreBot.Store.Entity;
using CoreBot.Utilities;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
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
        private const string ORDER = "OrderLine";
        private const string WORDLIST = "WordList";
        private const string FOUNDPRODUCT = "FoundProduct";
        private readonly IPrestashopApi PrestashopApi;
        private readonly IConfiguration Configuration;
        private readonly PurchaseController PurchaseController;

        public OrderProductDialog(UserController userController, ConversationState conversationState, IPrestashopApi prestashopApi,
            IConfiguration configuration, PurchaseController purchaseController) 
            : base(nameof(OrderProductDialog),userController,conversationState)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new TextPrompt("TextValidator", ValidateQuantityAsync));
            AddDialog(new TextPrompt("ProductValidator", ValidateProductAsync));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
               {
                   CheckPermissionStepAsync,
                   LuisConversionStepAsync,
                   ProductStepAsync,
                   SelectionCardStepAsync,
                   DisableCardStepAsync,
                   AmountStepAsync,
                   ConfirmLineStepAsync,
                   AddToCartStepAsync,
               }));

            InitialDialogId = nameof(WaterfallDialog);
            PermissionLevel = PermissionLevels.Representative;
            PrestashopApi = prestashopApi;
            Configuration = configuration;
            PurchaseController = purchaseController;
        }

        private async Task<DialogTurnResult> LuisConversionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = (RecognizerResult)stepContext.Options;

            var orderLine = new OrderLine();

            var product = luisResult.Entities["Product"]?.FirstOrDefault()?.ToString();
            var quantity = (int?)luisResult.Entities["number"]?.FirstOrDefault();

            var words = product.Split(new char[] { ' ' }).ToList();

            var products = await PrestashopApi.GetProductsByKeyWords(words.ToFilterParameterList());

            if(products.Products.Count > 0)
            {
                stepContext.Values[FOUNDPRODUCT] = true;

                if (products.Products.Count == 1) orderLine.ProductId = products.First().Id;
                else
                {
                    stepContext.Values[WORDLIST] = words;
                    orderLine.ProductId = -1;
                }
            }
            else
            {
                stepContext.Values[FOUNDPRODUCT] = false;
            }

            orderLine.Amount = quantity ?? -1;

            return await stepContext.NextAsync(orderLine, cancellationToken);

        }

        private async Task<DialogTurnResult> ProductStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[ORDER] = (OrderLine)stepContext.Result;

            if (!stepContext.GetValue<bool>(FOUNDPRODUCT))
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("What product would you want to buy?") }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SelectionCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            List<string> wordList;
            if (stepContext.Values.ContainsKey(WORDLIST))
            {
                wordList = stepContext.GetValue<List<string>>(WORDLIST);
            }
            else
            {
                wordList = ((string)stepContext.Result).Split(new char[] { ' ' }).ToList();
            }

            var products = await PrestashopApi.GetProductsByKeyWords(wordList.ToFilterParameterList());

            var reply = stepContext.Context.Activity.CreateReply();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = products.ToSelectionCarousel(Configuration);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(chooseMsg), cancellationToken);
            await stepContext.Context.SendActivityAsync(reply,cancellationToken);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("")});
        }

        private async Task<DialogTurnResult> AmountStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var orderLine = stepContext.GetValue<OrderLine>(ORDER);

            if(orderLine.ProductId == -1)
            {
                orderLine.ProductId = CardUtils.GetValueFromAction<int>((string)stepContext.Result);
            }            

            var product = (await PrestashopApi.GetProductById(orderLine.ProductId)).First();

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text($"How many {product.GetNameByLanguage(Languages.English)} do you want to buy?"),
                RetryPrompt = MessageFactory.Text("How many you want to buy? Just type a number!"),
            };

            if (orderLine.Amount == -1)
            {
                return await stepContext.PromptAsync("TextValidator", promptOptions, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ConfirmLineStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var orderLine = stepContext.GetValue<OrderLine>(ORDER);

            var product = await PrestashopApi.GetProductById(orderLine.ProductId);

            string orderString = orderLine.Amount.ToString() + (orderLine.Amount > 1 ? " units of " : " unit of ")
                + product.First().GetNameByLanguage(Languages.English);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), 
                new PromptOptions { Prompt = MessageFactory.Text(addToCartMsg + "\n- " + orderString) }, cancellationToken);
        }


        private async Task<DialogTurnResult> AddToCartStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            { 
                var orderLine = stepContext.GetValue<OrderLine>(ORDER);
                await PurchaseController.AddOrderLineToUser(stepContext.Context.Activity.From.Id, orderLine);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(addedMsg),cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(notAddedMsg), cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(whatElse), cancellationToken);


            return await stepContext.EndDialogAsync((bool)stepContext.Result, cancellationToken);
        }

        private async Task<bool> ValidateProductAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var product = promptContext.Context.Activity.Text;
            var collection = await PrestashopApi.GetProductByName(product);
            return await Task.FromResult(collection.Products.Count == 0);
        }

        private async Task<bool> ValidateQuantityAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            return await Task.FromResult(int.TryParse(promptContext.Context.Activity.Text, out _));
        }

    }
}
