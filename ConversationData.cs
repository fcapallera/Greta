using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot
{
    public class ConversationData
    {
        public NodeDecisio currentNode { get; set; }

        public bool startedTechSupport { get; set; } = false;

        public NodeDecisio rootNode { get; set; }
    }
}
