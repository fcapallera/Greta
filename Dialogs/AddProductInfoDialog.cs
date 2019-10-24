using AdaptiveCards;
using CoreBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class AddProductInfoDialog : CancelAndHelpDialog
    {
        private const string Preview = "Do you want to preview the Product you just added?";
        private readonly IConfiguration Configuration;

        public AddProductInfoDialog(UserState userState, IConfiguration configuration) : base(nameof(AddProductInfoDialog), userState)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt), ValidateCardInputAsync));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CheckPermissionStepAsync,
                PromptCardStepAsync,
                ProcessResultStepAsync,
                PromptProductCardAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);

            // VITROSEP and Superusers only
            PermissionLevel = 5;
            Configuration = configuration;
        }

        private async Task<DialogTurnResult> PromptCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string[] paths = { ".", "Cards", "productInfoFillingCard.json" };
            var cardJson = File.ReadAllText(Path.Combine(paths));
            var card = AdaptiveCard.FromJson(cardJson).Card;

            var activity = new Activity
            {
                Attachments = new List<Attachment>() {
                        new Attachment()
                        {
                            Content = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(card)),
                            ContentType = "application/vnd.microsoft.card.adaptive"
                        }
                    },
                Type = ActivityTypes.Message
            };

            var opts = new PromptOptions
            {
                Prompt = activity,
                RetryPrompt = activity
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), opts);
        }

        private async Task<DialogTurnResult> ProcessResultStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var jobject = JObject.Parse((string)stepContext.Result);
            var productInfo = new ProductInfo
            {
                Name = (string)jobject["ProdName"],
                Title = (string)jobject["ProdTitle"],
                DisplayText = (string)jobject["ProdDesc"]
            };

            if (!string.IsNullOrEmpty((string)jobject["StoreURL"]))
            {
                productInfo.StoreURL = (string)jobject["StoreURL"];
            }
            if (!string.IsNullOrEmpty((string)jobject["ImageURL"]))
            {
                productInfo.ImageURL = (string)jobject["ImageURL"];
            }

            stepContext.Values["ProductInfo"] = productInfo;
            int connection = SendProductInfoToDb(productInfo);

            if (connection < 0)
            {
                await stepContext.Context.SendActivityAsync("There was an error sending the Product to the DataBase.");
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("The product was added to the DataBase successfully");
                return await stepContext.PromptAsync(nameof(ConfirmPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text(Preview) }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> PromptProductCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var productInfo = (ProductInfo)stepContext.Values["ProductInfo"];

                var attachment = CardUtils.CreateCardFromProductInfo(productInfo);
                var adaptiveCard = stepContext.Context.Activity.CreateReply();

                adaptiveCard.Attachments = new List<Attachment>() { attachment };
                await stepContext.Context.SendActivityAsync(adaptiveCard, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(whatElse), cancellationToken);
            return await stepContext.EndDialogAsync(null,cancellationToken);
        }


        private async Task<bool> ValidateCardInputAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var jobject = JObject.Parse(promptContext.Context.Activity.Text);

            if (string.IsNullOrEmpty((string)jobject["ProdName"]))
            {
                await promptContext.Context.SendActivityAsync(MessageFactory.Text("Product name can't be empty!"));
                return await Task.FromResult(false);
            }
            else if (string.IsNullOrEmpty((string)jobject["ProdTitle"]))
            {
                await promptContext.Context.SendActivityAsync(MessageFactory.Text("Product title can't be empty!"));
                return await Task.FromResult(false);
            }
            else if (string.IsNullOrEmpty((string)jobject["ProdDesc"]))
            {
                await promptContext.Context.SendActivityAsync(MessageFactory.Text("Product description can't be empty!"));
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }


        private int SendProductInfoToDb(ProductInfo productInfo)
        {
            bool storeEmpty = string.IsNullOrEmpty(productInfo.StoreURL);
            bool imageEmpty = string.IsNullOrEmpty(productInfo.ImageURL);
            string query = $@"INSERT INTO [dbo].[ProductInfo]
                VALUES (@ProductName, @ProductDesc, {(storeEmpty ? "NULL" : "@StoreURL")}, {(imageEmpty ? "NULL" : "@ImageURL")}, @ProductTitle)";

            string connectionString = Configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ProductName",productInfo.Name);
                command.Parameters.AddWithValue("@ProductDesc", productInfo.DisplayText);

                if(!storeEmpty)
                    command.Parameters.AddWithValue("@StoreURL", productInfo.StoreURL);

                if(!imageEmpty)
                    command.Parameters.AddWithValue("@ImageURL", productInfo.ImageURL);

                command.Parameters.AddWithValue("@ProductTitle", productInfo.Title);

                connection.Open();

                int result = command.ExecuteNonQuery();

                connection.Close();
                command.Dispose();

                return result;
            }
        }
    }
}
