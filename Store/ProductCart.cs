using System.Collections.Generic;

namespace CoreBot.Store
{
    public class ProductCart
    {
        public List<SingleOrder> Products { get; }

        public ProductCart(SingleOrder singleOrder)
        {
            Products = new List<SingleOrder>
            {
                singleOrder
            };
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
