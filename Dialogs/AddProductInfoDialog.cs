using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class AddProductInfoDialog : CancelAndHelpDialog
    {
        private readonly IConfiguration Configuration;
        public AddProductInfoDialog(UserState userState, IConfiguration configuration) : base(nameof(AddProductInfoDialog), userState)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt), ValidateCardInputAsync));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CheckPermissionStepAsync,
                PromptCardStepAsync
            }));

            // VITROSEP and Superusers only
            PermissionLevel = 1;
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
                            Content = card,
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
    }
}
