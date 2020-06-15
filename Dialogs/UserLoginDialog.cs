using CoreBot.Controllers;
using CoreBot.Extensions;
using CoreBot.Models;
using CoreBot.Store;
using CoreBot.Store.Entity;
using CoreBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class UserLoginDialog : CardDialog
    {
        private const string validationMsg = "I've asked our Staff members to validate your account. You will receive a notification when it's done!";

        //HELP MESSAGES
        private const string EMAILSTEP = "The email associated to your VitrosepStore account is a unique identifier, If you type your email I will quickly identify who you are and we'll proceed to log you in!";
        private const string PASSWORDSTEP = "To complete de verification you need to enter your password in the text box. If the password is there once you submit it you can always delete it so it's not on your screen anymore.";
        private const string VALIDATIONSTEP = "A member of our staff needs to validate your profile. That means identifying your needs and set which features you will be able to access.";
        private const string DEFAULT = "Logging in is something you have to do only once so we can retrieve your data (addresses, company, etc.) from VitrosepStore. Once you are logged in, you'll have access to new features as a registered user!";

        private readonly IPrestashopApi PrestashopApi;
        private readonly IConfiguration Configuration;
        private readonly IServiceProvider ServiceProvider;
        private readonly NotifyController NotifyController;
        private const string CUSTOMER = "Customer";
        private string UserEmail;

        public UserLoginDialog(ConversationState conversationState, IPrestashopApi prestashopApi, IConfiguration configuration,
            IServiceProvider serviceProvider, NotifyController notifyController, UserController userController)
            : base(nameof(UserLoginDialog), userController, conversationState)
        {
            AddDialog(new TextPrompt("EmailValidator", ValidateEmailAsync));
            AddDialog(new TextPrompt("PasswordValidator", ValidatePasswordAsync));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskEmailStepAsync,
                ConfirmPasswordStepAsync,
                DisableCardStepAsync,
                CheckUserProfileStepAsync,
                AskForVerificationStepAsync
            }));

            PrestashopApi = prestashopApi;
            Configuration = configuration;
            ServiceProvider = serviceProvider;
            NotifyController = notifyController;
            PermissionLevel = PermissionLevels.Unregistered;
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Tell me, what e-mail do you have associated in your vitrosepStore account? " +
                "Your e-mail works like an username, I will be able to find you in your database instantly!"),
                RetryPrompt = MessageFactory.Text("Couldn't find that e-mail anywhere, make sure it's written correctly.\n" +
                "So again, what's your e-mail?")
            };

            return await stepContext.PromptAsync("EmailValidator", promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmPasswordStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserEmail = (string)stepContext.Result;
            var customer = (await PrestashopApi.GetCustomerByEmail(UserEmail)).First();
            stepContext.Values[CUSTOMER] = customer;
            var card = CardUtils.CreateCardFromJson("submitPassword");

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

            var promptOptions = new PromptOptions
            {
                Prompt = activity,
                RetryPrompt = activity
            };

            await stepContext.Context.SendActivityAsync($"Hi, in order to verify you're {customer.GetFullName()}, please enter your password",
                null,InputHints.IgnoringInput, cancellationToken);

            return await stepContext.PromptAsync("PasswordValidator", promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> CheckUserProfileStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var customer = stepContext.GetValue<Customer>(CUSTOMER);

            //UserProfile already exists, check if it's validated.
            if (await UserController.CheckForUserProfileAsync(customer.Id))
            {
                if(await CheckForValidationAsync(customer.Id))
                {
                    await UserController.UpdateBotId(customer.Id, stepContext.Context.Activity.From.Id);

                    await stepContext.Context.SendActivityAsync("Login Successful");

                    return await stepContext.EndDialogAsync(true, cancellationToken);
                }
            }
            //User profile doesn't exist, create one and ask for validation.
            else
            {
                var profile = new Models.UserProfile
                {
                    PrestashopId = customer.Id,
                    BotUserId = stepContext.Context.Activity.From.Id,
                    Validated = false
                };

                await UserController.AddUserAsync(profile);
            }

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Mmmm... I see your profile is not validated yet. You want me to contact "
                + "a member of our staff so they can validate it for you?"),
                RetryPrompt = MessageFactory.Text("Your profile is not validated yet, would you like to ask for validation? (Yes/No)")
            };

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), promptOptions, cancellationToken);
        }


        private async Task<DialogTurnResult> AskForVerificationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the result of the user's choice
            var result = (bool)stepContext.Result;

            if (result)
            {
                var userId = stepContext.Context.Activity.From.Id;
                await NotifyController.RequestValidationAsync(userId);

                await stepContext.Context.SendActivityAsync(validationMsg);

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Ok then. Remember you can ask for validation "
                    + "whenever you want, just ask me and I'll contact a member of our staff!");
                await stepContext.Context.SendActivityAsync("What can I do for you today?");
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }


        private async Task<bool> ValidateEmailAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var email = promptContext.Context.Activity.Text;

            var userCollection = await PrestashopApi.GetCustomerByEmail(email);

            return await Task.FromResult(userCollection.Elements.Count != 0);
        }

        private async Task<bool> ValidatePasswordAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var customer = (await PrestashopApi.GetCustomerByEmail(UserEmail)).First();

            string json = promptContext.Context.Activity.Text;
            try
            {
                CardUtils.GetValueFromAction<string>(json);
            }
            catch (JsonReaderException)
            {
                await promptContext.Context.SendActivityAsync("Please, type your Vitrosep Store password on the textbox or cancel the operation! (You can do so by type **cancel** or **quit**)");
                await promptContext.Context.SendActivityAsync(promptContext.Options.RetryPrompt);
                return await Task.FromResult(false);
            }   

            var password = CardUtils.GetValueFromAction<string>(json);
            bool isValid;

            //BCrypt algorythm for newer generated passwords
            if (customer.Password.Length == 60)
            {
                isValid = BCrypt.Net.BCrypt.Verify(password, customer.Password);
            }
            //MD5 hashing for older generated passwords
            else
            {
                var provider = MD5.Create();
                string salt = Configuration.GetSection("PrestashopSettings").GetSection("CookieKey").Value;
                byte[] bytes = provider.ComputeHash(Encoding.ASCII.GetBytes(salt + password));
                string computedHash = BitConverter.ToString(bytes);

                isValid = computedHash == customer.Password;
            }

            if (!isValid)
            {
                await promptContext.Context.SendActivityAsync("Incorrect password, you must've got something wrong :(");
                await promptContext.Context.SendActivityAsync(promptContext.Options.RetryPrompt);
            }

            return await Task.FromResult(isValid);
        }

        
        private async Task<bool> CheckForValidationAsync(int prestaId)
        {
            var user = await UserController.GetUserByPrestashopIdAsync(prestaId);
            return await Task.FromResult(user.Validated);
        }

        
        protected override async Task DialogHelpMessage(DialogContext innerDc)
        {
            var dialog = innerDc.Stack[innerDc.Stack.Count - 1];

            if(dialog.Id == WATERFALL)
            {
                var stepIndex = dialog.State["stepIndex"];

                string helpMsg;

                switch (stepIndex)
                {
                    case 0:
                        helpMsg = EMAILSTEP;
                        break;
                    case 1:
                        helpMsg = PASSWORDSTEP;
                        break;
                    case 3:
                        helpMsg = VALIDATIONSTEP;
                        break;
                    default:
                        helpMsg = DEFAULT;
                        break;
                }

                await innerDc.Context.SendActivityAsync(helpMsg);
            }
        }
    }
}
