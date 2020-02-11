using CoreBot.Controllers;
using CoreBot.Extensions;
using CoreBot.Store;
using CoreBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class UserValidationDialog : CardDialog
    {
        private const string askCustNameMsg = "Enter the name (and first name) of the new customer you want to validate.";
        private const string carouselMsg = "Which one of these customers do you want to validate?";
        private const string permissionMsg = "Which level of permission do you want the user to have? (sorted from highest to lowest)";
        private readonly IPrestashopApi PrestashopApi;
        private readonly NotifyController NotifyController;

        public UserValidationDialog(ConversationState conversationState, IPrestashopApi prestashopApi,
            UserController userController, NotifyController notifyController) : 
            base(nameof(UserValidationDialog), userController, conversationState)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new TextPrompt("CustomerValidationPrompt",ValidateCustomerInputAsync));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CheckPermissionStepAsync,
                CustomerNameStepAsync,
                DisplayChoiceStepAsync,
                DisableCardStepAsync,
                SetPermissionStepAsync,
                ValidateUserStepAsync
            }));

            NotifyController = notifyController;
            PrestashopApi = prestashopApi;
            PermissionLevel = PermissionLevels.Superuser;
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> CustomerNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var opts = new PromptOptions
            {
                Prompt = MessageFactory.Text(askCustNameMsg)
            };
            return await stepContext.PromptAsync("CustomerValidationPrompt", opts, cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayChoiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var name = ((string)stepContext.Result).Split(null);

            var customers = name.Length == 1 
                ? (await PrestashopApi.GetCustomerByFirstName(name[0]))
                : (await PrestashopApi.GetCustomerByFullName(name[0], name[1]));

            if (customers.Customers.Count == 1)
            {
                return await stepContext.NextAsync(customers.First().Id, cancellationToken);
            } 
            else
            {
                var activity = MessageFactory.Carousel(customers.ToSelectionCarousel(), carouselMsg);
                await stepContext.Context.SendActivityAsync(activity,cancellationToken);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("") });
            }
        }

        private async Task<DialogTurnResult> SetPermissionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["id"] = stepContext.Result.GetType() == typeof(int) 
                ? (int)stepContext.Result 
                : CardUtils.GetValueFromAction<int>((string)stepContext.Result);

            var permList = Enum.GetValues(typeof(PermissionLevels)).Cast<PermissionLevels>().ToList();
            var permStrings = (from permission in permList select permission.GetDescription()).ToList();

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(permissionMsg),
                RetryPrompt = MessageFactory.Text("Choose a value from the list below (sorted from highest to lowest)"),
                Choices = ChoiceFactory.ToChoices(permStrings)
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ValidateUserStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choice = (FoundChoice)stepContext.Result;
            var permission = choice.Value.GetValueFromDescription<PermissionLevels>();

            var prestashopId = stepContext.GetValue<int>("id");

            //If the user already asked for permission
            if (await UserController.CheckForUserProfileAsync(prestashopId))
            {
                await UserController.ValidateUserAsync(prestashopId, (int)permission);
                await NotifyController.NotifyValidation(prestashopId);
            }
            //If we're validating the user ahead of his first login
            else
            {
                var profile = new Models.UserProfile()
                {
                    Permission = (int)permission,
                    PrestashopId = prestashopId,
                    Validated = true
                };

                await UserController.AddUserAsync(profile);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(
                "A new user has been added. He can now login and start using Greta!"),cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<bool> ValidateCustomerInputAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            string[] words = promptContext.Context.Activity.Text.Split(null);
            bool result = false;

            if (words.Length == 2)
            {
                var collection = await PrestashopApi.GetCustomerByFullName(words[0],words[1]);

                result = collection.Customers.Count > 0;
            }
            else if (words.Length == 1)
            {
                var collection = await PrestashopApi.GetCustomerByFirstName(words[0]);

                result = collection.Customers.Count > 0;
            }

            if (!result)
            {
                var response = $"There's no customer named {promptContext.Context.Activity.Text} in PrestaShop. Please, try again or cancel.";
                await promptContext.Context.SendActivityAsync(response);
            }

            return await Task.FromResult(result);
        }
    }
}
