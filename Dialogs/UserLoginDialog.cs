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

        private readonly IPrestashopApi PrestashopApi;
        private readonly IConfiguration Configuration;
        private readonly IServiceProvider ServiceProvider;
        private readonly NotifyController NotifyController;
        private readonly UserController UserController;
        private const string CUSTOMER = "Customer";
        private string UserEmail;

        public UserLoginDialog(UserState userState, ConversationState conversationState, IPrestashopApi prestashopApi, IConfiguration configuration,
            IServiceProvider serviceProvider, NotifyController notifyController, UserController userController)
            : base(nameof(UserLoginDialog), userState, conversationState)
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
                AskForVerificationStepAsync
            }));

            PrestashopApi = prestashopApi;
            Configuration = configuration;
            ServiceProvider = serviceProvider;
            NotifyController = notifyController;
            UserController = userController;
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
                    using (var context = ServiceProvider.CreateScope())
                    {
                        var db = context.ServiceProvider.GetRequiredService<GretaDBContext>();

                        var user = await UserController.GetUserByPrestashopIdAsync(customer.Id);
                        user.BotUserId = stepContext.Context.Activity.From.Id;
                        await db.SaveChangesAsync();
                    }

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
                Prompt = MessageFactory.Text("Your profile is not validated yet, would you like to ask for validation?"),
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
                return await stepContext.EndDialogAsync(false, cancellationToken);
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

        
        private async Task<bool> CheckForValidationAsync(int prestaId)
        {
            var user = await UserController.GetUserByPrestashopIdAsync(prestaId);
            return await Task.FromResult(user.Validated);
        }
    }
}
