using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot
{
    public class ConversationData
    {
        public NodeDecisio CurrentNode { get; set; }

        public bool StartedTechSupport { get; set; } = false;

        public NodeDecisio RootNode { get; set; }

        public Dictionary<string, DateTime> DisabledCards { get; } = new Dictionary<string,DateTime>();
    }
}
