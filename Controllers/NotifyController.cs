using CoreBot.Models;
using CoreBot.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
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

        public NotifyController(IBotFrameworkHttpAdapter adapter, ConcurrentDictionary<string,ConversationReference> references,
            UserController userController, IConfiguration configuration, IPrestashopApi prestashopApi)
        {
            _adapter = adapter;
            _conversationReferences = references;
            UserController = userController;
            _appId = configuration["MicrosoftAppId"];
            PrestashopApi = prestashopApi;

            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
            }
        }

        public async Task RequestValidationAsync(string id)
        {
            var superusers = await UserController.GetUsersByPermissionLevelAsync(0);

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
                    await turnContext.SendActivityAsync("Your account has been validated, you can start using Greta in its full potential!");

            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, botCallBack, default);
        }
    }
}
