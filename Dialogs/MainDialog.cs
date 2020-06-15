using CoreBot.Controllers;
using CoreBot.Extensions;
using CoreBot.Store;
using CoreBot.Utilities;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected IBotServices BotServices;
        private readonly IStatePropertyAccessor<ConversationData> _conversationAccessor;
        private readonly IConfiguration Configuration;
        private readonly UserController UserController;
        private readonly NotifyController NotifyController;
        private readonly IPrestashopApi PrestashopApi;

        public MainDialog(IBotServices botServices, TechnicalAssistanceDialog technicalAssistance,
            OrderProductDialog orderProduct, AskUserInfoDialog infoDialog, ConversationState conversationState, ConfirmOrderDialog confirmOrderDialog,
            AddProductInfoDialog addProductInfoDialog, UserValidationDialog userValidationDialog, IConfiguration configuration,
            UserController userController, NotifyController notifyController, IPrestashopApi prestashopApi)
            : base(nameof(MainDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(orderProduct);
            AddDialog(technicalAssistance);
            AddDialog(infoDialog);
            AddDialog(confirmOrderDialog);
            AddDialog(addProductInfoDialog);
            AddDialog(userValidationDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                WelcomeStepAsync,
                InitialStepAsync,
                DispatchStepAsync,
                FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
            BotServices = botServices;
            UserController = userController;
            NotifyController = notifyController;
            PrestashopApi = prestashopApi;
            _conversationAccessor = conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            Configuration = configuration;
        }

        private async Task<DialogTurnResult> WelcomeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var conversationData = await _conversationAccessor.GetAsync(
                stepContext.Context, () => new ConversationData()
            );

            if (!conversationData.Welcomed)
            {
                conversationData.Welcomed = true;
                return await stepContext.ReplaceDialogAsync(nameof(AskUserInfoDialog), null, cancellationToken);
            }

            return await stepContext.NextAsync(null,cancellationToken);
        }


        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Cridem el recognizer per comprovar quin servei cognitiu farem servir (LUIS o QnA)
            var recognizerResult = await BotServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);

            //El top intent ens dirà quin servei farem servir
            var (intent, score) = recognizerResult.GetTopScoringIntent();

            var results = await BotServices.QnA.GetAnswersAsync(stepContext.Context);
            if (results.Any() && (results.First().Score > score) || intent == "None")
            {
                stepContext.Values["Intent"] = "QnA";
                var answer = results.Length == 0 ? "Sorry I didn't understand that" : results.First().Answer;
                return await stepContext.NextAsync(answer, cancellationToken);
            }
            else
            {
                stepContext.Values["Intent"] = intent;
                return await stepContext.NextAsync(recognizerResult, cancellationToken);
            }
        }



        private async Task<DialogTurnResult> DispatchStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var intent = stepContext.GetValue<string>("Intent");
            

            switch (intent)
            {
                case "ProductInformation":
                    // TODO (ARREGLAR-HO)

                    // Invoquem el mètode d'extensió que hem creat
                    /*var productInfo = luisResult.ToProductInfo(Configuration);
                    if (productInfo != null)
                    {
                        var attachment = CardUtils.CreateCardFromProductInfo(productInfo);
                        var adaptiveCard = stepContext.Context.Activity.CreateReply();

                        adaptiveCard.Attachments = new List<Attachment>() { attachment };
                        await stepContext.Context.SendActivityAsync(adaptiveCard, cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Sorry, we couldn't find a product matching your requirements."), cancellationToken);
                    }
                    break;*/

                case "OrderProduct":
                    var recognizer = stepContext.Result as RecognizerResult ?? null;
                    // Invoquem el mètode d'extensió per extreure una comanda del json.
                    return await stepContext.BeginDialogAsync(nameof(OrderProductDialog), recognizer, cancellationToken);

                case "TechnicalAssistance":
                    NodeConstructor nbuilder = new NodeConstructor();
                    NodeDecisio arrel = nbuilder.BuildTree("ExempleArbre.txt");

                    return await stepContext.BeginDialogAsync(nameof(TechnicalAssistanceDialog), arrel, cancellationToken);

                case "ConfirmCart":
                    return await stepContext.BeginDialogAsync(nameof(ConfirmOrderDialog), null, cancellationToken);

                case "AddProductInfo":
                    return await stepContext.BeginDialogAsync(nameof(AddProductInfoDialog), null, cancellationToken);

                case "CheckProducts":
                    var task = DisplayAllProductsAsync(stepContext,cancellationToken);

                    await stepContext.Context.SendActivityAsync("Wait till I load them all...");

                    await task;
                    break;

                case "Register":
                    var ps = new ProcessStartInfo("https://vitrosepstore.com/en/login?create_account=1")
                    {
                        UseShellExecute = true,
                        Verb = "open"
                    };
                    Process.Start(ps);
                    break;

                case "AskValidation":
                    var botId = stepContext.Context.Activity.From.Id;
                    var user = await UserController.GetUserByBotIdAsync(botId);

                    if(user != null && user.PrestashopId != null)
                    {
                        await NotifyController.RequestValidationAsync(botId);
                        await stepContext.Context.SendActivityAsync("I sent a validation request to a member of our staff");
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync("You're not logged in! In order to ask for validation " +
                            "you have to log in first (just ask me *I want to log in*), and if you're not registered yet " +
                            "ask me to register (*I want to register*) and then log in.\n You can request the validation right after :)");
                    }
                    break;

                case "ValidateUser":
                    return await stepContext.BeginDialogAsync(nameof(UserValidationDialog), null, cancellationToken);

                case "CartToOrder":


                case "QnA":
                    var answer = (string)stepContext.Result;
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(answer), cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }



        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.GetValue<string>("Intent")!="QnA")
            {
                await stepContext.Context.SendActivityAsync("What else can I do for you today?");
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }


        private async Task DisplayAllProductsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var products = await PrestashopApi.GetAllProducts();

            var reply = stepContext.Context.Activity.CreateReply();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = products.ToCarousel(Configuration);

            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
        }

        private async Task DisplayProductInfoAsync(LuisResult luisResult)
        {
            //TODO
        }
    }
}
