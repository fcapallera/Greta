using CoreBot.Store.Entity;
using Refit;
using System.Threading.Tasks;

namespace CoreBot.Store
{
    public interface IPrestashopApi
    {
        [Get("/products?display=[id,name,description,id_default_image]&filter[name]=%[{product}]%")]
        Task<ProductCollection> GetProductByName(string product);

        [Get("/products?display=[id,name,description,id_default_image]&filter[id]={id}%")]
        Task<ProductCollection> GetProductById(int id);

        [Get("/products?display=full")]
        Task<ProductCollection> GetAllProducts();

        [Get("/products?display=full{wordParameters}")]
        Task<ProductCollection> GetProductsByKeyWords(string wordParameters);

        [Get("/customers?display=[id,id_lang,passwd,lastname,firstname,email,company,date_upd]&filter[id]={id}")]
        Task<CustomerCollection> GetCustomerById(int id);

        [Get("/customers?display=[id,id_lang,passwd,lastname,firstname,email,company,date_upd]&filter[firstname]=%[{firstname}]%&filter[lastname]=%[{lastname}]%")]
        Task<CustomerCollection> GetCustomerByFullName(string firstname, string lastname);

        [Get("/customers?display=[id,id_lang,passwd,lastname,firstname,email,company,date_upd]&filter[firstname]=%[{firstname}]%")]
        Task<CustomerCollection> GetCustomerByFirstName(string firstname);

        [Get("/customers?display=[id,id_lang,passwd,lastname,firstname,email,company,date_upd]&filter[email]=%[{email}]%")]
        Task<CustomerCollection> GetCustomerByEmail(string email);

        [Get("/customers?display=full&filter[name]={words}")]
        Task<CustomerCollection> GetCustomerByWords(string words);

        [Get("/carts?filter[id_customer]={customerId}&display=full")]
        Task<CartCollection> GetCartsByCustomer(int customerId);

        [Get("/carts?filter[id_customer]={customerId}&display=full")]
        Task<OrderCollection> GetOrdersByCustomer(int customerId);

        [Get("/addresses?filter[id_customer]={customerId}&display=full")]
        Task<Address> GetAddressByCustomer(int customerId);

        [Get("/carts?schema=blank")]
        Task<PrestashopCart> GetBlankCart();

        [Get("/orders?schema=blank")]
        Task<PrestashopOrder> GetBlankOrder();

        [Post("/carts")]
        Task<Cart> PostCart([Body]PrestashopCart prestaCart);

        [Post("/orders")]
        Task<Cart> PostOrder([Body]PrestashopOrder order);


        //Rudimentary solution to the parsing parameters problem
        [Get("/products?display=full&filter[name]=%[{product1}]%")]
        Task<ProductCollection> GetProductByOneParam(string product1);

        [Get("/products?display=full&filter[name]=%[{product1}]%&filter[name]=%[{product2}]%")]
        Task<ProductCollection> GetProductByTwoParam(string product1, string product2);

        [Get("/products?display=full&filter[name]=%[{product1}]%&filter[name]=%[{product2}]%&filter[name]=%[{product3}]%")]
        Task<ProductCollection> GetProductByThreeParam(string product1, string product2, string product3);
    }

    public enum Languages : int
    {
        Spanish = 1,
        Catalan = 2,
        French = 6,
        English = 7
    }
}
