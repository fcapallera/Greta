using CoreBot.Controllers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class PermissionDialog : ComponentDialog
    {
        // Permission levels
        protected const int SUPERUSER = 0;
        protected const int VITROSEP = 1;
        protected const int CUSTOMERS = 2;
        protected const int REPRESENTATIVE = 3;
        protected const int LEAD = 4;
        protected const int UNREGISTERED = 5;
        protected static readonly string[] permissions = { "Superuser", "Vitrosep", "Customer", "Representative", "Lead", "Unregistered" };
        private const string noPermission = "Sorry, you don't have privilege to use this functionality.";

        protected readonly UserController UserController;
        
        public PermissionDialog(UserController userController, string id) : base(id)
        {
            UserController = userController;
        }

        protected async Task<DialogTurnResult> CheckPermissionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var user = await UserController.GetUserByBotIdAsync(stepContext.Context.Activity.From.Id);
            if (user.Permission >= PermissionLevel)
            {
                return await stepContext.NextAsync(stepContext.Options, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(noPermission), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        public int PermissionLevel { get; set; } = 5;

        protected int ResolvePermission(string permission)
        {
            switch (permission)
            {
                case "Superuser": return SUPERUSER;
                case "Vitrosep": return VITROSEP;
                case "Customer": return CUSTOMERS;
                case "Representative": return REPRESENTATIVE;
                case "Lead": return LEAD;
                case "Unregistered": return UNREGISTERED;
                default: return -1;
            }
        }
    }
}
