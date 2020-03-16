using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    public class Cart : IIdentifiable
    {
        [XmlElement("id")]
        public override int Id { get; set; }

        [XmlIgnore]
        public Customer Customer { get; set; }
        
        private int? _customerId { get; set; }

        [XmlElement("id_customer")]
        public int CustomerId
        {
            get { return _customerId ?? Customer.Id; }
            set { _customerId = value; }
        }

        //public int DeliveryAddressId { get; set; }

        //public int AdressInvoiceId { get; set; }
        
        public int CurrencyId { get; set; }

        //public int GuestId { get; set; }

        public int LanguageId { get; set; }

        [XmlElement("associations")]
        public CartRowCollection Rows { get; set; }
    }

    [XmlRoot("prestashop")]
    public class CartCollection
    {
        [XmlArray("carts")]
        [XmlArrayItem("cart")]
        public List<Cart> Carts { get; set; }

        public Cart Last()
        {
            if (Carts.Count > 0)
                return Carts[Carts.Count - 1];

            else return null;
        }
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

        public CartRow(Product product, int quantity)
        {
            Product = product;
            Quantity = quantity;
        }

        [XmlIgnore]
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
