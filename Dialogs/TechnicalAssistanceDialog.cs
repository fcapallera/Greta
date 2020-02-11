using CoreBot.Controllers;
using CoreBot.Extensions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class TechnicalAssistanceDialog : CancelAndHelpDialog
    {
        //private readonly IStatePropertyAccessor<ConversationData> _conversationDataAccessor;
        private const string _insightMessage = "If your problem still persists, would you like to give us a description about your problem? If we already solved your problem chose \"No\"";
        private const string _promptMessage = "Please, tell us more about your problem.";

        private readonly IStatePropertyAccessor<ConversationData> _conversationDataAccessor;
        private readonly QuestionController QuestionController;

        public TechnicalAssistanceDialog(ConversationState conversationState, UserController userController,
            QuestionController questionController)
            : base(nameof(TechnicalAssistanceDialog),userController)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CheckPermissionStepAsync,
                PromptOptionsAsync,
                OptionalStepAsync,
                FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
            PermissionLevel = PermissionLevels.Customer;
            QuestionController = questionController;
            _conversationDataAccessor = conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
        }

        private async Task<DialogTurnResult> PromptOptionsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var conversationData = await _conversationDataAccessor.GetAsync(stepContext.Context, () => new ConversationData());

            var nodeActual = (NodeDecisio)stepContext.Options;
            stepContext.Values["Node"] = nodeActual;

            Console.WriteLine(nodeActual.nodeId);
            if(nodeActual.fills.Count == 0)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(nodeActual.pregunta), cancellationToken);

                var promptMessage = MessageFactory.Text(_insightMessage, _insightMessage);
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);

            }
            var promptOptions = new PromptOptions
            {   
                Prompt = MessageFactory.Text(nodeActual.pregunta),
                RetryPrompt = MessageFactory.Text("Please choose an option from the list."),
                Choices = ChoiceFactory.ToChoices(nodeActual.obtenirRespostes()),
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> OptionalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var nodeActual = stepContext.GetValue<NodeDecisio>("Node");
            if(nodeActual.fills.Count == 0)
            {
                var result = (bool)stepContext.Result;
                if (result)
                {
                    var promptMessage = MessageFactory.Text(_promptMessage, _promptMessage, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage}, cancellationToken);
                }
                else
                {
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }

            return await stepContext.NextAsync(stepContext.Result, cancellationToken);
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choice = (FoundChoice)stepContext.Result;
            var nodeActual = stepContext.GetValue<NodeDecisio>("Node");

            if(nodeActual.fills.Count == 0)
            {
                var question = (string)stepContext.Result;

                await QuestionController.AddQuestionAsync(question, stepContext.Context.Activity.From.Id);

                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.ReplaceDialogAsync(nameof(TechnicalAssistanceDialog), nodeActual.ObtenirNode(choice.Value), cancellationToken);
            }
        }
    }
}
