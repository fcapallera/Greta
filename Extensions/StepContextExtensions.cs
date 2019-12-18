using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Extensions
{
    public static class StepContextExtensions
    {
        public static T GetValue<T>(this WaterfallStepContext stepContext, string index)
        {
            if (stepContext.Values.TryGetValue(index, out object value))
                return (T)value;

            else throw new KeyNotFoundException();
        }
    }
}
