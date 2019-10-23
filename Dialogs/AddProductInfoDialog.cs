using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class AddProductInfoDialog : CancelAndHelpDialog
    {
        public AddProductInfoDialog(UserState userState) : base(nameof(AddProductInfoDialog), userState)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CheckPermissionStepAsync,
            }));

            PermissionLevel = 1;
        }
    }
}
