using CoreBot.Dialogs;
using CoreBot.Models;
using CoreBot.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Controllers
{
    public class NotifyController
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly IPrestashopApi PrestashopApi;
        private readonly UserController UserController;
        private readonly BotState _ConversationState;
        private readonly CartToOrderDialog CartToOrderDialog;

        public NotifyController(IBotFrameworkHttpAdapter adapter, ConcurrentDictionary<string,ConversationReference> references,
            UserController userController, IConfiguration configuration, IPrestashopApi prestashopApi,
            ConversationState conversationState, CartToOrderDialog cartToOrderDialog)
        {
            _adapter = adapter;
            _conversationReferences = references;
            UserController = userController;
            _appId = configuration["MicrosoftAppId"];
            PrestashopApi = prestashopApi;
            _ConversationState = conversationState;
            CartToOrderDialog = cartToOrderDialog;

            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
            }
        }

        public async Task RequestValidationAsync(string id)
        {
            var superusers = await UserController.GetUsersByPermissionLevelAsync(PermissionLevels.Superuser);

            var requestingUser = await UserController.GetUserByBotIdAsync(id);

            var prestashopUser = (await PrestashopApi.GetCustomerById(requestingUser.PrestashopId.Value)).First();

            // We declare a local function to use as BotCallBackHandler in the ContinueConversationAsync Method
            async Task botCallBack(ITurnContext turnContext, CancellationToken cancellationToken) =>
                    await turnContext.SendActivityAsync($"User {prestashopUser.GetFullName()} would like to get his account validated.");

            foreach (Models.UserProfile superuser in superusers)
            {
                var conversationReference = _conversationReferences[superuser.BotUserId];

                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, botCallBack, default);
            }
        }


        public async Task NotifyValidation(int prestashopId)
        {
            var user = await UserController.GetUserByPrestashopIdAsync(prestashopId);

            var conversationReference = _conversationReferences[user.BotUserId];

            async Task botCallBack(ITurnContext turnContext, CancellationToken cancellationToken) =>
                    await turnContext.SendActivityAsync("Your account has been validated.\n\n" +
                    " If you want to log in now say something like **I want to log in** or **Let me authenticate**," +
                    " and you will be ready to enjoy all our services!\n\n You can always log in later.");

            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, botCallBack, default);
        }


        public async Task NotifyCustomerPurchase(Cart cart)
        {
            var user = (await PrestashopApi.GetCustomerById(cart.User.PrestashopId.Value)).First();

            var vitrosep = await UserController.GetUsersByPermissionLevelAsync(PermissionLevels.Vitrosep);

            foreach(UserProfile profile in vitrosep)
            {
                var conversationReference = _conversationReferences[profile.BotUserId];

                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, CartCallBack, default);
            }
        }

        public async Task CartCallBack(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = _ConversationState.CreateProperty<DialogState>(nameof(DialogState));

            var dialogSet = new DialogSet(conversationStateAccessors);
            dialogSet.Add(this.CartToOrderDialog);

            var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);
            var results = await dialogContext.ContinueDialogAsync(cancellationToken);

            if (results.Status == DialogTurnStatus.Empty)
            {
                var user = await UserController.GetUserByBotIdAsync(turnContext.Activity.From.Id);

                await dialogContext.BeginDialogAsync(CartToOrderDialog.Id, user, cancellationToken);
                await _ConversationState.SaveChangesAsync(dialogContext.Context, false, cancellationToken);
            }
            else
                await turnContext.SendActivityAsync("Starting proactive message bot call back");
        }
    }
}
