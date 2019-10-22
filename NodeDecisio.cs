using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot
{
    public class NodeDecisio
    {
        public NodeDecisio() {
            fills = new Dictionary<string, NodeDecisio>();
        }

        public NodeDecisio(int _nodeId, string _pregunta)
        {
            fills = new Dictionary<string, NodeDecisio>();
            nodeId = _nodeId;
            pregunta = _pregunta;
        }

        public int nodeId { get; set; }

        public string pregunta { get; set; }
        
        public Dictionary<string,NodeDecisio> fills { get; }

        public List<string> obtenirRespostes()
        {
            return new List<string>(this.fills.Keys);
        }

        public NodeDecisio ObtenirNode(string resposta)
        {
            return fills[resposta];
        }
    }
}
