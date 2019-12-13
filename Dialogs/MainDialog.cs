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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected IBotServices BotServices;
        private readonly IStatePropertyAccessor<UserProfile> _profileAccessor;
        private readonly IConfiguration Configuration;
        private readonly IPrestashopApi PrestashopApi;

        public MainDialog(IBotServices botServices, TechnicalAssistanceDialog technicalAssistance,
            OrderProductDialog orderProduct, AskUserInfoDialog infoDialog, UserState userState, ConfirmOrderDialog confirmOrderDialog,
            AddProductInfoDialog addProductInfoDialog, UserValidationDialog userValidationDialog, IConfiguration configuration)
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
                //FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
            BotServices = botServices;
            _profileAccessor = userState.CreateProperty<UserProfile>(nameof(UserProfile));
            Configuration = configuration;
        }

        private async Task<DialogTurnResult> WelcomeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _profileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            if (!userProfile.AskedForUserInfo)
            {
                return await stepContext.BeginDialogAsync(nameof(AskUserInfoDialog), null, cancellationToken);
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
                return await stepContext.NextAsync(results.First().Answer, cancellationToken);
            }
            else
            {
                stepContext.Values["Intent"] = intent;
                return await stepContext.NextAsync(recognizerResult, cancellationToken);
            }
        }



        private async Task<DialogTurnResult> DispatchStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var intent = stepContext.Values["Intent"];
            var recognizer = stepContext.Result as RecognizerResult ?? null;

            switch (intent)
            {
                case "ProductInformation":
                    var luisResult = (LuisResult)recognizer.Properties["luisResult"];
                    // Invoquem el mètode d'extensió que hem creat
                    var productInfo = luisResult.ToProductInfo(Configuration);
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
                    break;

                case "OrderProduct":
                    // Invoquem el mètode d'extensió per extreure una comanda del json.
                    var singleOrder = recognizer.ToSingleOrder();
                    return await stepContext.BeginDialogAsync(nameof(OrderProductDialog), singleOrder, cancellationToken);

                case "TechnicalAssistance":
                    NodeConstructor nbuilder = new NodeConstructor();
                    NodeDecisio arrel = nbuilder.BuildTree("ExempleArbre.txt");

                    return await stepContext.BeginDialogAsync(nameof(TechnicalAssistanceDialog), arrel, cancellationToken);

                case "ConfirmCart":
                    return await stepContext.BeginDialogAsync(nameof(ConfirmOrderDialog), cancellationToken);

                case "AddProductInfo":
                    return await stepContext.BeginDialogAsync(nameof(AddProductInfoDialog), cancellationToken);

                case "CheckProducts":
                    string products = await GetProductNamesAsync(stepContext.Context);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(products), cancellationToken);
                    break;

                case "ValidateUser":
                    return await stepContext.BeginDialogAsync(nameof(UserValidationDialog), null, cancellationToken);

                case "QnA":
                    var answer = (string)stepContext.Result;
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(answer), cancellationToken);
                    break;
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }


        private async Task<string> GetProductNamesAsync(ITurnContext turnContext)
        {
            var userProfile = await _profileAccessor.GetAsync(turnContext, () => new UserProfile());

            if (userProfile.Permission > 1)
            {
                return "Sorry, you don't have permission to use this feature.";
            }
            else
            {
                string connectionString = Configuration.GetConnectionString("DefaultConnection");
                string query = "SELECT ProductName FROM [dbo].[ProductInfo]";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    string products = "This is the list of products in the DataBase:\n";

                    while (reader.Read())
                    {
                        products += $"- {reader.GetString(0)}\n";
                    }

                    command.Dispose();
                    reader.Close();
                    connection.Close();

                    return products;
                }
            }
        }
    }
}
