using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    public class Order : IIdentifiable
    {
        public Order(Customer customer, Cart cart, Address deliveryAddress, Address invoiceAddress)
        {
            Customer = customer;
            Cart = cart;
            DeliveryAddress = deliveryAddress;
            InvoiceAddress = invoiceAddress;
        }

        [XmlIgnore]
        public Customer Customer { get; set; }

        [XmlIgnore]
        public Cart Cart { get; set; }

        [XmlIgnore]
        public Address DeliveryAddress { get; set; }

        [XmlIgnore]
        public Address InvoiceAddress { get; set; }

        [XmlElement("id")]
        public override int Id { get; set; }

        private int? _deliveryAddressId { get; set; }

        [XmlElement("id_address_delivery")]
        public int DeliveryAddressId
        {
            get { return _deliveryAddressId ?? DeliveryAddress.Id; }
            set { _deliveryAddressId = value; }
        }

        private int? _invoiceAddressId { get; set; }

        [XmlElement("id_address_invoice")]
        public int? InvoiceAddressId
        {
            get { return _invoiceAddressId ?? InvoiceAddress.Id; }
            set { _invoiceAddressId = value; }
        }

        private int? _cartId { get; set; }

        [XmlElement("id_cart")]
        public int CartId
        {
            get { return _cartId ?? Cart.Id; }
            set { _cartId = value; }
        }

        private int? _customerId { get; set; }

        [XmlElement("id_customer")]
        public int CustomerId
        {
            get { return _customerId ?? Customer.Id; }
            set { _customerId = value; }
        }

        [XmlElement("payment")]
        public string Payment { get; set; }

        [XmlElement("total_paid_real")]
        public float TotalPayedReal { get; set; }

        [XmlElement("total_products")]
        public float TotalProducts { get; set; }

        [XmlElement("total_products_wt")]
        public float TotalProductsWithoutTax { get; set; }

        [XmlElement("conversion_rate")]
        public float ConversionRate { get; set; }

        /*[XmlElement("associations")]
        public OrderRows OrderRows { get; set; }*/
    }

    public class OrderRows
    {
        [XmlArray("order_rows")]
        [XmlArrayItem("order_row")]
        public List<OrderRow> Rows { get; set; }
    }

    public class OrderRow
    {
        public OrderRow(Product product, int quantity)
        {
            _product = product;
            Quantity = quantity;
            AttributeId = 0;
        }

        private Product _product;

        private int? _productId { get; set; }

        [XmlElement("product_id")]
        public int ProductId
        {
            get { return _productId ?? _product.Id; }
            set { _productId = value; }
        }

        [XmlElement("product_attribute_id")]
        public int AttributeId { get; set; }

        [XmlElement("quantity")]
        public int Quantity { get; set; }
    }
}
