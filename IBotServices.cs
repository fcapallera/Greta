using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot
{
    public interface IBotServices
    {
        LuisRecognizer Dispatch { get; }

        QnAMaker QnA { get; }
    }
}
