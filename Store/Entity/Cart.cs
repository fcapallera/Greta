using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    public class Cart : IIdentifiable
    {
        public Cart() { }

        public Cart(Customer customer)
        {
            Id = 0;
            DeliveryAddressId = 0;
            AdressInvoiceId = 0;
            CurrencyId = 1;
            Customer = customer;
            GuestId = 0;
            LanguageId = 7;
            ShopGroupId = 1;
            DeliveryOption = string.Empty;
            SecureKey = string.Empty;
            ShopId = 1;
            GiftMessage = string.Empty;
            CarrierId = 0;
            Recyclable = 0;
            Gift = 0;
            MobileTheme = 0;
            AllowSeparatedPackage = 0;
            DateAdd = DateTime.Now;
            DateUpd = DateTime.Now;
        }


        [XmlElement("id")]
        public override int Id { get; set; }

        [XmlElement("id_address_delivery")]
        public int DeliveryAddressId { get; set; }

        [XmlElement("id_address_invoice")]
        public int AdressInvoiceId { get; set; }

        [XmlElement("id_currency")]
        public int CurrencyId { get; set; }

        [XmlIgnore]
        public Customer Customer { get; set; }
        
        private int? _customerId { get; set; }


        [XmlElement("id_customer")]
        public int CustomerId
        {
            get { return _customerId ?? Customer.Id; }
            set { _customerId = value; }
        }

        [XmlElement("id_guest")]
        public int GuestId { get; set; }

        [XmlElement("id_lang")]
        public int LanguageId { get; set; }

        [XmlElement("id_shop_group")]
        public int ShopGroupId { get; set; }

        [XmlElement("id_shop")]
        public int ShopId { get; set; }

        [XmlElement("id_carrier")]
        public int CarrierId { get; set; }

        [XmlElement("recyclable")]
        public int Recyclable { get; set; }

        [XmlElement("gift")]
        public int Gift { get; set; }

        [XmlElement("gift_message")]
        public string GiftMessage { get; set; }

        [XmlElement("mobile_theme")]
        public int MobileTheme { get; set; }

        [XmlElement("delivery_option")]
        public string DeliveryOption { get; set; }
        
        [XmlElement("secure_key")]
        public string SecureKey { get; set; }

        [XmlElement("allow_separated_package")]
        public int AllowSeparatedPackage { get; set; }

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

        public CartRow(int productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
            CombinationId = 1;
            DeliveryAddress = 1;
        }

        [XmlElement("id_product")]
        public int ProductId { get; set; }  

        [XmlElement("id_product_attribute")]
        public int CombinationId { get; set; }

        [XmlElement("id_address_delivery")]
        public int DeliveryAddress { get; set; }

        [XmlElement("quantity")]
        public int Quantity { get; set; }
    }

    [XmlRoot("prestashop")]
    public class CartToPost
    {
        public CartToPost() { }

        [XmlElement("cart")]
        public PostCart Cart { get; set; }

        public static async Task<CartToPost> BuildCartAsync(Models.Cart cart, IPrestashopApi prestashopApi)
        {
            var customer = (await prestashopApi.GetCustomerById(cart.User.PrestashopId.Value)).First();

            var prestaCart = new CartToPost()
            {
                Cart = new PostCart(customer)
            };

            var rowCollection = new CartRowCollection();

            foreach (Models.OrderLine line in cart.OrderLine)
            {
                var row = new CartRow(line.ProductId, line.Amount);
                rowCollection.Rows.Add(row);
            }

            prestaCart.Cart.Rows = rowCollection;

            return await Task.FromResult(prestaCart);
        }
    }

    public class PostCart
    {
        public PostCart() { }

        public PostCart(Customer customer)
        {
            DeliveryAddressId = 0;
            AdressInvoiceId = 0;
            CurrencyId = 1;
            Customer = customer;
            GuestId = 0;
            LanguageId = 7;
            ShopGroupId = 1;
            DeliveryOption = string.Empty;
            SecureKey = string.Empty;
            ShopId = 1;
            GiftMessage = string.Empty;
            CarrierId = 0;
            Recyclable = 0;
            Gift = 0;
            MobileTheme = 0;
            AllowSeparatedPackage = 0;
            DateAdd = DateTime.Now;
            DateUpd = DateTime.Now;
        }

        [XmlElement("id_address_delivery")]
        public int DeliveryAddressId { get; set; }

        [XmlElement("id_address_invoice")]
        public int AdressInvoiceId { get; set; }

        [XmlElement("id_currency")]
        public int CurrencyId { get; set; }

        [XmlIgnore]
        public Customer Customer { get; set; }

        private int? _customerId { get; set; }


        [XmlElement("id_customer")]
        public int CustomerId
        {
            get { return _customerId ?? Customer.Id; }
            set { _customerId = value; }
        }

        [XmlElement("id_guest")]
        public int GuestId { get; set; }

        [XmlElement("id_lang")]
        public int LanguageId { get; set; }

        [XmlElement("id_shop_group")]
        public int ShopGroupId { get; set; }

        [XmlElement("id_shop")]
        public int ShopId { get; set; }

        [XmlElement("id_carrier")]
        public int CarrierId { get; set; }

        [XmlElement("recyclable")]
        public int Recyclable { get; set; }

        [XmlElement("gift")]
        public int Gift { get; set; }

        [XmlElement("gift_message")]
        public string GiftMessage { get; set; }

        [XmlElement("mobile_theme")]
        public int MobileTheme { get; set; }

        [XmlElement("delivery_option")]
        public string DeliveryOption { get; set; }

        [XmlElement("secure_key")]
        public string SecureKey { get; set; }

        [XmlElement("allow_separated_package")]
        public int AllowSeparatedPackage { get; set; }

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

        [XmlElement("associations")]
        public CartRowCollection Rows { get; set; }
    }

    [XmlRoot("prestashop")]
    public class PostedCart
    {
        public PostedCart() { }

        [XmlElement("cart")]
        public Cart Cart { get; set; }
    }
}
