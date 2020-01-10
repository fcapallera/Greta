using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    public class Cart : IIdentifiable
    {
        public override int Id { get; set; }

        //public int DeliveryAddressId { get; set; }

        //public int AdressInvoiceId { get; set; }
        
        public int CurrencyId { get; set; }

        //public int GuestId { get; set; }

        public int LanguageId { get; set; }

        [XmlElement("associations")]
        public CartRowCollection Rows { get; set; }
    }

    public class CartRowCollection
    {
        public CartRowCollection()
        {
            Rows = new List<CartRow>();
        }

        [XmlArray("cart_rows")]
        [XmlArrayItem("cart_row")]
        public List<CartRow> Rows { get; set; }
    }

    public class CartRow
    {
        public CartRow()
        {
        }

        public Product Product;

        private int? _productId { get; set; }

        [XmlElement("id_product")]
        public int ProductId
        {
            get { return _productId ?? Product.Id; }
            set { _productId = value; }
        }

        [XmlElement("id_product_attribute")]
        public int CombinationId { get; set; }

        [XmlElement("quantity")]
        public int Quantity { get; set; }
    }
}
