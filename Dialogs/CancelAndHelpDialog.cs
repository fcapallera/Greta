// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.Controllers;
using CoreBot.Permission;
using CoreBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace CoreBot.Dialogs
{
    public class CancelAndHelpDialog : PermissionDialog
    {
        protected const string whatElse = "What else can I do for you today?";
        protected const string WATERFALL = "WaterfallDialog";

        public CancelAndHelpDialog(string id, UserController userController)
            : base(userController,id)
        {
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
                        await ShowHelpAsync(innerDc);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);

                    case "cancel":
                    case "quit":
                        await innerDc.Context.SendActivityAsync($"Cancelling", cancellationToken: cancellationToken);
                        return await innerDc.CancelAllDialogsAsync();
                }
            }

            return null;
        }


        protected virtual async Task ShowHelpAsync(DialogContext innerDc)
        {
            var card = CardUtils.CreateCardFromJson("gretaWelcomeCard");

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

            await innerDc.Context.SendActivityAsync(activity);
        }
    }
}
