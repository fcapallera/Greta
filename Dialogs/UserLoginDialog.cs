using CoreBot.Extensions;
using CoreBot.Models;
using CoreBot.Store;
using CoreBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class UserLoginDialog : CardDialog
    {
        private readonly IPrestashopApi PrestashopApi;
        private readonly IConfiguration Configuration;
        private readonly IServiceProvider ServiceProvider;
        private string UserEmail;

        public UserLoginDialog(UserState userState, ConversationState conversationState, IPrestashopApi prestashopApi, IConfiguration configuration,
            IServiceProvider serviceProvider) : base(nameof(UserLoginDialog), userState, conversationState)
        {
            AddDialog(new TextPrompt("EmailValidator", ValidateEmailAsync));
            AddDialog(new TextPrompt("PasswordValidator", ValidatePasswordAsync));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CheckPermissionStepAsync,
                AskEmailStepAsync,
                ConfirmPasswordStepAsync,
                DisableCardStepAsync,
                CheckUserProfileStepAsync,
                AskForVerificationStepAsync,
                FinalStepAsync
            }));

            PrestashopApi = prestashopApi;
            Configuration = configuration;
            ServiceProvider = serviceProvider;
            PermissionLevel = UNREGISTERED;
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter your username (email)"),
                RetryPrompt = MessageFactory.Text("This email does not exist. Please try again. You can cancel anytime.")
            };

            return await stepContext.PromptAsync("EmailValidator", promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmPasswordStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserEmail = (string)stepContext.Result;
            var customer = (await PrestashopApi.GetCustomerByEmail(UserEmail)).First();
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

            await stepContext.Context.SendActivityAsync($"Hi, in order to verify you're {customer.FirstName + " " + customer.LastName}, please enter your password",
                null,InputHints.IgnoringInput, cancellationToken);

            return await stepContext.PromptAsync("PasswordValidator", promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> CheckUserProfileStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // The user passed the email validation so we can assume the user exists. 
            var customer = (await PrestashopApi.GetCustomerByEmail(UserEmail)).First();

            if (await CheckForUserProfileAsync(customer.Id))
            {
                stepContext.Values["verificationStep"] = false;
                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                stepContext.Values["verificationStep"] = true;

                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Your profile is not validated yet, would you like to ask for validation?"),
                    RetryPrompt = MessageFactory.Text("Your profile is not validated yet, would you like to ask for validation? (Yes/No)")
                };

                return await stepContext.PromptAsync(nameof(ConfirmPrompt), promptOptions, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> AskForVerificationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var verification = stepContext.GetValue<bool>("verificationStep");

            if (verification)
            {
                // Get the result of the user's choice
                var result = (bool)stepContext.Result;

                if (result)
                {
                    await stepContext.Context.SendActivityAsync("TO_DO Send notification for user Validation");
                    return await stepContext.EndDialogAsync(true, cancellationToken);
                }
                else
                {
                    return await stepContext.EndDialogAsync(false, cancellationToken);
                }
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }


        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Login Successful");

            return await stepContext.EndDialogAsync(true, cancellationToken);
        }

        private async Task<bool> ValidateEmailAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var email = promptContext.Context.Activity.Text;

            var userCollection = await PrestashopApi.GetCustomerByEmail(email);

            if(userCollection.Elements.Count == 0)
            {
                //await promptContext.Context.SendActivityAsync(MessageFactory.Text("This email does not exist"));
                return await Task.FromResult(false);
            }

            return await Task.FromResult(true);
        }

        private async Task<bool> ValidatePasswordAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var customer = (await PrestashopApi.GetCustomerByEmail(UserEmail)).First();

            string json = promptContext.Context.Activity.Text;
            var password = CardUtils.GetValueFromAction<string>(json);

            //BCrypt algorythm for newer generated passwords
            if (customer.Password.Length == 60)
            {
                return await Task.FromResult(BCrypt.Net.BCrypt.Verify(password, customer.Password));
            }
            //MD5 hashing for older generated passwords
            else
            {
                var provider = MD5.Create();
                string salt = Configuration.GetSection("PrestashopSettings").GetSection("CookieKey").Value;
                byte[] bytes = provider.ComputeHash(Encoding.ASCII.GetBytes(salt + password));
                string computedHash = BitConverter.ToString(bytes);

                return await Task.FromResult(computedHash == customer.Password);
            }
        }

        private async Task<bool> CheckForUserProfileAsync(int userId)
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<GretaDBContext>();

                var sources = await db.UserProfile
                    .Where(x => x.Id == userId)
                    .SingleOrDefaultAsync();

                return await Task.FromResult(sources != null);
            }
        }
    }
}
