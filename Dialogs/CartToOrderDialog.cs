using AdaptiveCards;
using CoreBot.Controllers;
using CoreBot.Extensions;
using CoreBot.Models;
using CoreBot.Store;
using CoreBot.Store.Entity;
using CoreBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class CartToOrderDialog : CardDialog
    {
        private readonly PurchaseController PurchaseController;
        private const string FROMNOTIFICATION = "FromNotification";
        private const string PRICEVALIDATOR = "PriceValidator";
        private const string CART = "Cart";
        private const string chooseMsg = "Which order do you want to prepare?";
        private readonly IPrestashopApi PrestashopApi;

        public CartToOrderDialog(ConversationState conversationState, UserController userController,
            PurchaseController purchaseController, IPrestashopApi prestashopApi)
            : base(nameof(CartToOrderDialog), userController, conversationState)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt),CardJsonValidator));
            AddDialog(new TextPrompt(PRICEVALIDATOR, PriceCardValidator));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] 
            {
                CheckPermissionStepAsync,
                AskForActionStepAsync,
                ProcessConfirmationStepAsync,
                DisableCardStepAsync,
                ProcessChoiceStepAsync,
                AssignPricesStepAsync,
                ProcessPricesStepAsync
            }));

            PermissionLevel = PermissionLevels.Vitrosep;
            PurchaseController = purchaseController;
            InitialDialogId = nameof(WaterfallDialog);
            PrestashopApi = prestashopApi;
        }

        private async Task<DialogTurnResult> AskForActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Type type = stepContext.Options.GetType();
            if(type == typeof(UserProfile))
            {
                var user = (UserProfile)stepContext.Options;
                stepContext.Values[FROMNOTIFICATION] = true;

                var prestashopUser = (await PrestashopApi.GetCustomerById(user.PrestashopId.Value)).First();

                string confirmMsg = $"{prestashopUser.GetFullName()} has an order pending for confirmation. Would you like to" +
                    $"confirm it now?";

                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(confirmMsg),
                    RetryPrompt = MessageFactory.Text(confirmMsg + "(Yes / No)")
                };

                return await stepContext.PromptAsync(nameof(ConfirmPrompt), promptOptions, cancellationToken);
            }
            else
            {
                stepContext.Values[FROMNOTIFICATION] = false;
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ProcessConfirmationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var infoList = new List<RequestedInfo>();

            if (stepContext.GetValue<bool>(FROMNOTIFICATION))
            {
                var result = (bool)stepContext.Result;

                if (result)
                {
                    var requests = await PurchaseController.GetOrderRequests();

                    if(requests.Count == 0)
                    {
                        await stepContext.Context.SendActivityAsync("There's no pending orders to validate" +
                            "you're done for today!");
                        return await stepContext.EndDialogAsync(null, cancellationToken);
                    }
                    else if(requests.Count == 1)
                    {
                        return await stepContext.NextAsync(requests[0].CartId, cancellationToken);
                    }
                    else
                    {
                        foreach(OrderRequest request in requests)
                        {
                            infoList.Add(await RequestedInfo.BuildRequestedInfoAsync(request.Cart, PrestashopApi));
                        }                 
                    }
                }
                else
                {
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }
            }
            else
            {
                var words = ((string)stepContext.Options).Split(new char[] { ' ' }).ToList();

                var customers = (await PrestashopApi.GetCustomerByWords(words.ToFilterParameterList())).Customers;

                var carts = await PurchaseController.GetCartFromUserList(customers);

                foreach(Models.Cart cart in carts)
                {
                    infoList.Add(await RequestedInfo.BuildRequestedInfoAsync(cart, PrestashopApi));
                }
            }

            var attachments = CardUtils.RequestedListToCarousel(infoList);

            var reply = stepContext.Context.Activity.CreateReply();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = attachments;

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(chooseMsg), cancellationToken);
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("") });
        }


        private async Task<DialogTurnResult> ProcessChoiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if(stepContext.Result.GetType() == typeof(string))
            {
                int id = CardUtils.GetValueFromAction<int>((string)stepContext.Result);

                var cart = await PurchaseController.GetCartById(id);

                return await stepContext.NextAsync(cart, cancellationToken);
            }

            return await stepContext.NextAsync(stepContext.Result, cancellationToken);
        }


        private async Task<DialogTurnResult> AssignPricesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cart = (Models.Cart)stepContext.Result;
            stepContext.Values[CART] = cart;

            var attachment = await CardUtils.CreatePriceAssignationCard(cart, PrestashopApi);

            var activity = new Activity
            {
                Attachments = new List<Attachment>() { attachment },
                Type = ActivityTypes.Message
            };

            var promptOptions = new PromptOptions
            {
                Prompt = activity,
                RetryPrompt = activity
            };

            return await stepContext.PromptAsync(PRICEVALIDATOR, promptOptions, cancellationToken);
        }


        private async Task<DialogTurnResult> ProcessPricesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var jObject = (JObject)stepContext.Result;
            var cart = stepContext.GetValue<Models.Cart>(CART);

            float totalPrice = 0;
            int count = 0;
            
            foreach(OrderLine line in cart.OrderLine)
            {
                var price = (float)jObject["InputCount" + count];
                totalPrice += price * line.Amount;
            }

            await stepContext.Context.SendActivityAsync($"Total price: {totalPrice}");
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }




        private async Task<bool> PriceCardValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if(await CardJsonValidator(promptContext, cancellationToken))
            {
                return await Task.FromResult(false);
            }
            else
            {
                var json = ((AdaptiveCard)promptContext.Context.Activity.Attachments[0].Content).ToJson();
                var jObject = (JObject)promptContext.Context.Activity.Text;

                int inputCount = Regex.Matches(json, "InputCount").Count;

                for(int i = 0; i < inputCount; i++)
                {
                    string varName = "InputCount" + i;
                    if ((string)jObject[varName] == "")
                    {
                        //await promptContext.Context.SendActivityAsync(promptContext.Options.RetryPrompt);
                        return await Task.FromResult(false);
                    }
                }
            }

            return await Task.FromResult(true);
        }
        
    }

    public class RequestedInfo
    {
        public int OrderRequestId { get; set; }
        public string Name { get; set; }
        public string ProductList { get; set; }

        async public static Task<RequestedInfo> BuildRequestedInfoAsync(Models.Cart cart, IPrestashopApi prestashopApi)
        {
            var customer = (await prestashopApi.GetCustomerById(cart.User.Id)).First();

            var productString = "";

            foreach(OrderLine line in cart.OrderLine)
            {
                var product = (await prestashopApi.GetProductById(line.ProductId)).First();

                productString += "- " + product.GetNameByLanguage(Languages.English) + "\n\n";
            }

            return new RequestedInfo(cart.Id, customer.Company, productString);
        }

        private RequestedInfo(int id, string name, string productList)
        {
            OrderRequestId = id;
            Name = name;
            ProductList = productList;
        }
    }
}
