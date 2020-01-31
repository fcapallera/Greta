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
        private const string unregisteredName = "As a non registered user, you must have a name!\nWhat do you want me to call you?";

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
                AskIfCompanyStepAsync,
                AskCompanyStepAsync,
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
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(unregisteredName)
                };

                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> AskIfCompanyStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var name = (string)stepContext.Result;

            var userProfile = await _profileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            userProfile.Name = name;

            stepContext.Values["profile"] = userProfile;

            var message = MessageFactory.Text($"Nice to meet you {name}, do you work for a company we might of?");

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = message }, cancellationToken);
        }


        private async Task<DialogTurnResult> AskCompanyStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Values["profile"];

            if ((bool)stepContext.Result)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(askCompanyMsg) }, cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }


        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var company = (string)stepContext.Result;
            var userProfile = (UserProfile)stepContext.Values["profile"];

            userProfile.AskedForUserInfo = true;
            userProfile.Company = company;
            await _profileAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(finishMsg) }, cancellationToken);
        }

    }
}
