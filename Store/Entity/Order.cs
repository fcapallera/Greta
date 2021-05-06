using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    public class Order : IIdentifiable
    {
        public Order() { }

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
            GiftMessage = "";
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
        public double TotalDiscounts { get; set; }

        [XmlElement("total_discounts_tax_incl")]
        public double TotalDiscountsTaxIncluded { get; set; }

        [XmlElement("total_discounts_tax_excl")]
        public double TotalDiscountsTaxExcluded { get; set; }

        [XmlElement("total_paid")]
        public double TotalPaid { get; set; }

        [XmlElement("total_paid_tax_incl")]
        public double TotalPaidTaxIncluded { get; set; }

        [XmlElement("total_paid_tax_excl")]
        public double TotalPaidTaxExcluded { get; set; }

        [XmlElement("total_paid_real")]
        public double TotalPayedReal { get; set; }

        [XmlElement("total_products")]
        public double TotalProducts { get; set; }

        [XmlElement("total_products_wt")]
        public double TotalProductsWithoutTax { get; set; }

        [XmlElement("conversion_rate")]
        public double ConversionRate { get; set; }

        [XmlElement("associations")]
        public OrderRows OrderRows { get; set; }
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
        public OrderRows()
        {
            Rows = new List<OrderRow>();
        }

        [XmlArray("order_rows")]
        [XmlArrayItem("order_row")]
        public List<OrderRow> Rows { get; set; }
    }

    public class OrderRow
    {
        public OrderRow() { }

        public OrderRow(Product product, int quantity)
        {
            _product = product;
            Quantity = quantity;
            AttributeId = 0;
            Isbn = "";
            Upc = "";
            Name = _product.GetNameByLanguage(Languages.English);
            Reference = _product.Reference;
            Ean13 = _product.Reference;
            Price = _product.Price;
            UnitPriceTaxExcl = _product.Price;
            UnitPriceTaxIncl = _product.Price * 1.21;
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

        [XmlElement("product_quantity")]
        public int Quantity { get; set; }

        [XmlElement("product_name")]
        public string Name { get; set; }

        [XmlElement("product_reference")]
        public string Reference { get; set; }

        [XmlElement("product_ean13")]
        public string Ean13 { get; set; }

        [XmlElement("product_isbn")]
        public string Isbn { get; set; }

        [XmlElement("product_upc")]
        public string Upc { get; set; }

        [XmlElement("product_price")]
        public double Price { get; set; }

        [XmlElement("unit_price_tax_incl")]
        public double UnitPriceTaxIncl { get; set; }

        [XmlElement("unit_price_tax_excl")]
        public double UnitPriceTaxExcl { get; set; }
    }

    [XmlRoot(Namespace = "http://www.w3.org/1999/xlink",
        ElementName = "prestashop",
        DataType = "string", IsNullable = true)]
    public class PostedOrder
    {
        public PostedOrder() { }

        public PostedOrder(Order order)
        {
            Order = order;
        }

        [XmlElement("order")]
        public Order Order { get; set; }
    }

    public class PostOrder
    {
        public PostOrder() { }

        public PostOrder(Customer customer, Cart cart, Address customerAddress)
        {
            Customer = customer;
            Cart = cart;
            Language = customer.Language;
            DeliveryAddress = customerAddress;
            InvoiceAddress = customerAddress;
            CarrierId = 1;
            Module = "free_order";
            Payment = "Free order";
            CurrentState = 15;
            InvoiceNumber = 0;
            InvoiceDate = "0000-00-00 00:00:00";
            DeliveryNumber = 0;
            DeliveryDate = "0000-00-00 00:00:00";
            DateAdd = DateTime.Now;
            DateUpd = DateTime.Now;
            CurrencyId = 1;
            ShopGroupId = 1;
            ShopId = 1;
            SecureKey = string.Empty;
            Recyclable = 0;
            Gift = 0;
            GiftMessage = string.Empty;
            MobileTheme = 0;
            ConversionRate = 1;
            Reference = string.Empty;
            OrderRows = new OrderRows();
        }

        [XmlIgnore]
        public Customer Customer { get; set; }

        [XmlIgnore]
        public Cart Cart { get; set; }

        [XmlIgnore]
        public CustomerLanguage Language { get; set; }

        private int? _languageId { get; set; }

        [XmlIgnore]
        public Address DeliveryAddress { get; set; }

        [XmlIgnore]
        public Address InvoiceAddress { get; set; }

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

        [XmlElement("id_lang")]
        public int LanguageId
        {
            get { return _languageId ?? Language.Id; }
            set { _languageId = value; }
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

        [XmlIgnore]
        public int? _shippingNumber { get; set; }

        [XmlElement("shipping_number")]
        public string SNAsText
        {
            get { return (_shippingNumber.HasValue) ? _shippingNumber.ToString() : string.Empty; }
            set { _shippingNumber = !string.IsNullOrEmpty(value) ? int.Parse(value) : default(int?); }
        }

        [XmlElement("id_shop_group")]
        public int ShopGroupId { get; set; }

        [XmlElement("id_shop")]
        public int ShopId { get; set; }

        [XmlElement("secure_key")]
        public string SecureKey { get; set; }

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
        public double TotalDiscounts { get; set; }

        [XmlElement("total_discounts_tax_incl")]
        public double TotalDiscountsTaxIncluded { get; set; }

        [XmlElement("total_discounts_tax_excl")]
        public double TotalDiscountsTaxExcluded { get; set; }

        [XmlElement("total_paid")]
        public double TotalPaid { get; set; }

        [XmlElement("total_paid_tax_incl")]
        public double TotalPaidTaxIncluded { get; set; }

        [XmlElement("total_paid_tax_excl")]
        public double TotalPaidTaxExcluded { get; set; }

        [XmlElement("total_paid_real")]
        public double TotalPayedReal { get; set; }

        [XmlElement("total_products")]
        public double TotalProducts { get; set; }

        [XmlElement("total_products_wt")]
        public double TotalProductsWithTax { get; set; }

        [XmlElement("total_shipping")]
        public double TotalShipping { get; set; }

        [XmlElement("total_shipping_tax_incl")]
        public double TotalShippingTaxIncluded { get; set; }

        [XmlElement("total_shipping_tax_excl")]
        public double TotalShippingTaxExcluded { get; set; }

        [XmlElement("carrier_tax_rate")]
        public double CarrierTaxRate { get; set; }

        [XmlElement("total_wrapping")]
        public double TotalWrapping { get; set; }

        [XmlElement("total_wrapping_tax_incl")]
        public double TotalWrappingTaxIncluded { get; set; }

        [XmlElement("total_wrapping_tax_excl")]
        public double TotalWrappingTaxExcluded { get; set; }

        [XmlElement("round_mode")]
        public int RoundMode { get; set; }

        [XmlElement("round_type")]
        public int RoundType { get; set; }

        [XmlElement("conversion_rate")]
        public double ConversionRate { get; set; }

        [XmlElement("reference")]
        public string Reference { get; set; }

        [XmlElement("associations")]
        public OrderRows OrderRows { get; set; }


        public static async Task<Order> BuildOrderAsync(Models.Cart cart, IPrestashopApi prestashopApi)
        {
            var prestaCart = await CartToPost.BuildCartAsync(cart, prestashopApi);

            var postedCart = await prestashopApi.PostCart(prestaCart);

            var customer = (await prestashopApi.GetCustomerById(cart.User.PrestashopId.Value)).First();

            var customerAddresses = await prestashopApi.GetAddressByCustomer(cart.User.PrestashopId.Value);

            var customerAddress = customerAddresses.First();

            var order = new PostOrder(customer, postedCart.Cart, customerAddress);

            double totalPrice = 0;

            foreach(CartRow row in prestaCart.Cart.Rows.Rows)
            {
                var product = (await prestashopApi.GetProductById(row.ProductId)).First();

                totalPrice += product.Price;

                order.OrderRows.Rows.Add(
                    new OrderRow(product, row.Quantity)
                    );       
            }

            order.TotalProducts = totalPrice;
            order.TotalProductsWithTax = totalPrice * 1.21;
            order.TotalPaidTaxExcluded = totalPrice;
            order.TotalPaidTaxIncluded = totalPrice * 1.21;
            order.TotalPaid = totalPrice * 1.21;

            var prestaOrder = new OrderToPost { Order = order };

            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(OrderToPost));

            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "//SerializationOverview.xml";
            System.IO.FileStream file = System.IO.File.Create(path);

            writer.Serialize(file, prestaOrder);
            file.Close();

            var postedOrder = await prestashopApi.PostOrder(prestaOrder);

            return await Task.FromResult(postedOrder.Order);
        }
    }
    
    [XmlRoot("prestashop")]
    public class OrderToPost
    {
        public OrderToPost() { }

        [XmlElement("order")]
        public PostOrder Order { get; set; }
    }
}
