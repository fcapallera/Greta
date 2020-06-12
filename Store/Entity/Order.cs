using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    public class Order : IIdentifiable
    {
        public Order(Customer customer, Cart cart, Address customerAddress)
        {
            Customer = customer;
            Cart = cart;
            DeliveryAddress = customerAddress;
            InvoiceAddress = customerAddress;
            Module = "free_order";
            CurrentState = 15;
            InvoiceNumber = 0;
            InvoiceDate = "0000-00-00 00:00:00";
            DeliveryNumber = 0;
            DeliveryDate = "0000-00-00 00:00:00";
            DateAdd = cart.DateAdd;
            DateUpd = cart.DateUpd;
            CurrencyId = 1;
            ShopGroupId = 1;
            ShopId = 1;
            CarrierId = 0;
            Recyclable = 0;
            Gift = 0;
            MobileTheme = 0;
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
        public int InvoiceAddressId
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

        [XmlElement("id_currency")]
        public int CurrencyId { get; set; }

        private int? _customerId { get; set; }

        [XmlElement("id_customer")]
        public int CustomerId
        {
            get { return _customerId ?? Customer.Id; }
            set { _customerId = value; }
        }

        [XmlElement("id_carrier")]
        public int CarrierId { get; set; }

        [XmlElement("current_state")]
        public int CurrentState { get; set; }

        [XmlElement("module")]
        public string Module { get; set; }

        [XmlElement("invoice_number")]
        public int InvoiceNumber { get; set; }

        [XmlElement("invoice_date")]
        public string InvoiceDate { get; set; }

        [XmlElement("delivery_number")]
        public int DeliveryNumber { get; set; }

        [XmlElement("delivery_date")]
        public string DeliveryDate { get; set; }

        [XmlElement("valid")]
        public int Valid { get; set; }

        [XmlIgnore]
        public DateTime DateAdd { get; set; }

        [XmlElement("date_add")]
        public string DateAddString
        {
            get { return this.DateAdd.ToString("yyyy-MM-dd HH:mm:ss"); }
            set { this.DateAdd = DateTime.Parse(value); }
        }

        [XmlIgnore]
        public DateTime DateUpd { get; set; }

        [XmlElement("date_upd")]
        public string DateUpdString
        {
            get { return this.DateUpd.ToString("yyyy-MM-dd HH:mm:ss"); }
            set { this.DateUpd = DateTime.Parse(value); }
        }

        [XmlElement("id_shop_group")]
        public int ShopGroupId { get; set; }

        [XmlElement("id_shop")]
        public int ShopId { get; set; }

        [XmlElement("payment")]
        public string Payment { get; set; }

        [XmlElement("recyclable")]
        public int Recyclable { get; set; }

        [XmlElement("gift")]
        public int Gift { get; set; }

        [XmlElement("gift_message")]
        public string GiftMessage { get; set; }

        [XmlElement("mobile_theme")]
        public int MobileTheme { get; set; }

        [XmlElement("total_discounts")]
        public float TotalDiscounts { get; set; }

        [XmlElement("total_discounts_tax_incl")]
        public float TotalDiscountsTaxIncluded { get; set; }

        [XmlElement("total_discounts_tax_excl")]
        public float TotalDiscountsTaxExcluded { get; set; }

        [XmlElement("total_paid")]
        public float TotalPaid { get; set; }

        [XmlElement("total_paid_tax_incl")]
        public float TotalPaidTaxIncluded { get; set; }

        [XmlElement("total_paid_tax_excl")]
        public float TotalPaidTaxExcluded { get; set; }

        [XmlElement("total_paid_real")]
        public float TotalPayedReal { get; set; }

        [XmlElement("total_products")]
        public float TotalProducts { get; set; }

        [XmlElement("total_products_wt")]
        public float TotalProductsWithoutTax { get; set; }

        [XmlElement("conversion_rate")]
        public float ConversionRate { get; set; }

        [XmlElement("associations")]
        public OrderRows OrderRows { get; set; }

        /*public static async Task<PrestashopOrder> BuildOrderAsync(Models.Cart cart, IPrestashopApi prestashopApi)
        {
            var prestaCart = await Cart.BuildCartAsync(cart, prestashopApi);

            var postedCart = await prestashopApi.PostCart(prestaCart);

            var customer = (await prestashopApi.GetCustomerById(cart.User.PrestashopId.Value)).First();

            var customerAddress = await prestashopApi.GetAddressByCustomer(cart.User.PrestashopId.Value);

            var order = new Order(customer, postedCart, customerAddress);

            return await Task.FromResult(order);
        }*/
    }

    [XmlRoot("prestashop")]
    public class OrderCollection
    {
        [XmlArray("orders")]
        [XmlArrayItem("order")]
        public List<Order> Orders { get; set; }

        public Order Last()
        {
            if (Orders.Count > 0)
                return Orders[Orders.Count - 1];

            else return null;
        }
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

    [XmlRoot(Namespace = "http://www.w3.org/1999/xlink",
        ElementName = "prestashop",
        DataType = "string", IsNullable = true)]
    public class PrestashopOrder
    {
        [XmlElement("order")]
        public Order Order { get; set; }
    }
}
