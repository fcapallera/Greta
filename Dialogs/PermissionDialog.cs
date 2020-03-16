using CoreBot.Controllers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class PermissionDialog : ComponentDialog
    {
        private const string noPermission = "Sorry, you don't have privilege to use this functionality.";

        protected readonly UserController UserController;
        
        public PermissionDialog(UserController userController, string id) : base(id)
        {
            UserController = userController;
        }

        protected async Task<DialogTurnResult> CheckPermissionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var user = await UserController.GetUserByBotIdAsync(stepContext.Context.Activity.From.Id);
            if (user != null && user.Permission >= (int)PermissionLevel)
            {
                return await stepContext.NextAsync(stepContext.Options, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(noPermission), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        public PermissionLevels PermissionLevel { get; set; } = PermissionLevels.Superuser;
    }

    public enum PermissionLevels : int
    {
        [Description("Superuser")]
        Superuser = 0,
        [Description("Vitrosep")]
        Vitrosep = 1,
        [Description("Customer")]
        Customer = 2,
        [Description("Representative")]
        Representative = 3,
        [Description("Lead")]
        Lead = 4,
        [Description("Unregistered")]
        Unregistered = 5
    }
}
