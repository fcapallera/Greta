// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using CoreBot.Permission;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace CoreBot.Dialogs
{
    public class CancelAndHelpDialog : ComponentDialog, IPermissionObject
    {
        // Permission levels
        protected const int SUPERUSER = 0;
        protected const int VITROSEP = 1;
        protected const int CUSTOMERS = 2;
        protected const int REPRESENTATIVE = 3;
        protected const int LEAD = 4;
        protected const int UNREGISTERED = 5;
        protected static readonly string[] permissions = { "Superuser", "Vitrosep", "Customer", "Representative", "Lead", "Unregistered"};

        protected const string whatElse = "What else can I do for you today?";
        private const string noPermission = "Sorry, you don't have privilege to use this functionality.";
        protected readonly IStatePropertyAccessor<UserProfile> _profileAccessor;
        public int PermissionLevel { get; set; } = 5;

        public CancelAndHelpDialog(string id, UserState userState)
            : base(id)
        {
            _profileAccessor = userState.CreateProperty<UserProfile>(nameof(UserProfile));
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text.ToLowerInvariant();

                switch (text)
                {
                    case "help":
                    case "?":
                        await innerDc.Context.SendActivityAsync($"Show Help...", cancellationToken: cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);

                    case "cancel":
                    case "quit":
                        await innerDc.Context.SendActivityAsync($"Cancelling", cancellationToken: cancellationToken);
                        return await innerDc.CancelAllDialogsAsync();
                }
            }

            return null;
        }

        protected async Task<DialogTurnResult> CheckPermissionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _profileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            if (HasPermission(userProfile.Permission))
            {
                return await stepContext.NextAsync(stepContext.Options, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(noPermission),cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

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

        public bool HasPermission(int permission)
        {
            return PermissionLevel >= permission;
        }
    }
}
