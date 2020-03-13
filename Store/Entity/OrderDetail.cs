using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    public class OrderDetail
    {
        public OrderDetail(Product product, Order order)
        {
            _product = product;
            _order = order;
        }

        private Product _product;
        private Order _order;


        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("product_id")]
        public int ProductId
        {
            get { return _product.Id; }
        }

        [XmlIgnore]
        public Product Product
        {
            get { return _product; }
            set { _product = value; }
        }

        [XmlIgnore]
        public Order Order
        {
            get { return _order; }
            set { _order = value; }
        }

        private string _productName { get; set; }

        [XmlElement("product_name")]
        public string ProductName
        {
            get { return _productName ?? _product.GetNameByLanguage(Languages.English); }
            set { _productName = value; }
        }

        private float? _productPrice { get; set; }

        [XmlElement("product_price")]
        public float ProductPrice
        {
            get { return _productPrice ?? _product.Price; }
            set { _productPrice = value; }
        }

        private float? _productWeight { get; set; }

        [XmlElement("product_weight")]
        public float ProductWeight
        {
            get { return _productWeight ?? _product.Weight; }
            set { _productWeight = value; }
        }
    }
}
