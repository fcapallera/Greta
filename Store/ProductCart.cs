using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Store
{
    public class ProductCart
    {
        public List<SingleOrder> Products { get; } = new List<SingleOrder>();

        public ProductCart(SingleOrder singleOrder)
        {
            Products.Add(singleOrder);
        }

        public void AddOrder(SingleOrder singleOrder)
        {
            Products.Add(singleOrder);
        }
        
        public bool IsEmpty()
        {
            return Products.Count == 0;
        }
    }
}
