using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class AskUserInfoDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _profileAccessor;
        private const string askNameMsg = "My name is Greta, who are you?";
        private const string askCompanyMsg = "What company do you work for?";
        private const string finishMsg = "Thank you, what can I do for you today?";

        public AskUserInfoDialog(UserState userState) : base(nameof(AskUserInfoDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
                {
                    AskNameAsync,
                    AskIfCompanyAsync,
                    AskCompanyAsync,
                    FinalStepAsync,
                }));

            _profileAccessor = userState.CreateProperty<UserProfile>(nameof(UserProfile));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("First of all, let's get to know each other."), cancellationToken);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(askNameMsg) }, cancellationToken);
        }

        private async Task<DialogTurnResult> AskIfCompanyAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var name = (string)stepContext.Result;

            var userProfile = await _profileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            userProfile.name = name;

            stepContext.Values["profile"] = userProfile;

            var message = MessageFactory.Text($"Nice to meet you {name}, do you work for a company we might of?");

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = message }, cancellationToken);
        }

        private async Task<DialogTurnResult> AskCompanyAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

            userProfile.askedForUserInfo = true;
            userProfile.company = company;
            await _profileAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(finishMsg) }, cancellationToken);
        }

    }
}
