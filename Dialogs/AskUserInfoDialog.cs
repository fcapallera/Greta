using CoreBot.Controllers;
using CoreBot.Extensions;
using CoreBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class AskUserInfoDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _profileAccessor;
        private const string askCompanyMsg = "What company do you work for?";
        private const string finishMsg = "Thank you, what can I do for you today?";
        private const string registeredMsg = "Are you registered in our web store? (You need to be registered in order to get the most out of me!)";
        private const string waitForValidationMsg = "Thanks for registering.\nA VITROSEP administrator will validate your user soon, we will send you a notification asap!";

        public AskUserInfoDialog(UserState userState, UserLoginDialog userLoginDialog)
            : base(nameof(AskUserInfoDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                WelcomeStepAsync,
                AskForRegistrationStepAsync,
                RegisterOptionalStepAsync,
                FinalStepAsync,
            }));
            AddDialog(userLoginDialog);

            _profileAccessor = userState.CreateProperty<UserProfile>(nameof(UserProfile));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> WelcomeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var card = CardUtils.CreateCardFromJson("gretaWelcomeCard");

            var activity = new Microsoft.Bot.Schema.Activity
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

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(registeredMsg),
                RetryPrompt = MessageFactory.Text(registeredMsg + " (Yes/No)")
            };

            await stepContext.Context.SendActivityAsync(activity);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForRegistrationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (bool)stepContext.Result;

            if (result)
            {
                await stepContext.Context.SendActivityAsync("Use your VitrosepStore credentials to log in",null,InputHints.IgnoringInput,cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(UserLoginDialog), null, default);
            }
            else
            { 
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Would you like to register?"),
                    RetryPrompt = MessageFactory.Text("Would you like to register? (Yes/No)")
                };

                return await stepContext.PromptAsync(nameof(ConfirmPrompt), promptOptions, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> RegisterOptionalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (bool)stepContext.Result;
            if (result)
            {
                var ps = new ProcessStartInfo("https://vitrosepstore.com/en/login?create_account=1")
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(ps);

                await stepContext.Context.SendActivityAsync(waitForValidationMsg);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("You can register whenever you want (just tell me " +
                    "*I want to register*).\n Now you're currently interacting with me as a non-registered user, " +
                    "what that means is that your actions are limited! In order to enjoy all of our services you must " +
                    "register on our VitrosepStore and then Log in :)");
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }


        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            

            await stepContext.Context.SendActivityAsync(finishMsg);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

    }
}
