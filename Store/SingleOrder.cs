using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Store
{
    public class SingleOrder
    {
        public string Product { get; set; }

        public int Quantity { get; set; } = 0;

        public string Dimension { get; set; }

        public override string ToString()
        {
            var res = Quantity.ToString() + " ";

            if (Dimension != null)
            {
                res += Dimension + ((Quantity > 1) ? "s" : "") + " of ";
            }

            res += Product;

            return res;
        }

        public string AmountToString()
        {
            string dimension;
            switch (Dimension)
            {
                case "Kilogram":
                    dimension = "Kg";
                    break;

                case "Liter":
                    dimension = "L";
                    break;

                default:
                    dimension = "";
                    break;
            }

            return Quantity.ToString() + dimension;
        }
    }
}
