using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBot;

namespace CoreBot.Utilities
{
    public class NodeConstructor
    {

        public NodeConstructor() { }

        // Mètode que crearà un arbre a partir d'un fitxer i
        // ens tornarà una referència a l'arrel de l'arbre.
        public NodeDecisio BuildTree(string arxiu)
        {
            string line;

            Dictionary<int, NodeDecisio> nodes = new Dictionary<int, NodeDecisio>();
            Dictionary<int, Dictionary<string, int>> connexions = new Dictionary<int, Dictionary<string, int>>();

            System.IO.StreamReader file = new System.IO.StreamReader(arxiu);

            // Llegim el fitxer i emplenem les estructures auxiliars
            while((line = file.ReadLine()) != null)
            {
                // Llegim l'ID del node i el processem
                int nodeId = int.Parse(new String(line.TakeWhile(Char.IsDigit).ToArray()));

                // Llegim el text que es mostrarà quan arribem al node
                line = file.ReadLine();
                var nodeActual = new NodeDecisio(nodeId, line);
                nodes.Add(nodeId, nodeActual);

                System.Console.WriteLine("Node " + nodeId);

                // Processem cada opció amb el respectiu node destí
                line = file.ReadLine();
                Console.WriteLine("Fills:");
                while (line != null && line != "#")
                {
                    string[] splitString = line.Split(" : ");
                    int destiId = int.Parse(splitString[1]);
                    string opcio = splitString[0];
                    System.Console.WriteLine(destiId);
                    if (!connexions.ContainsKey(nodeId))
                    {
                        connexions.Add(nodeId,new Dictionary<string, int>());
                    }
                    connexions[nodeId].Add(opcio, destiId);
                    line = file.ReadLine();
                }
            }

            // Ara recorrem les estructures auxiliars per emplenar els nodes.

            foreach (KeyValuePair<int,Dictionary<string,int>> node in connexions)
            {
                var nodeActual = nodes[node.Key];
                Console.WriteLine(node.Key);

                foreach (KeyValuePair<string,int> connexio in node.Value)
                {
                    var nodeDesti = nodes[connexio.Value];
                    nodeActual.fills.Add(connexio.Key,nodeDesti);
                }
            }

            return nodes[1];

        }

    }
}
