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

        [Get("/products?display=full&filter[name]={wordParameters}")]
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
    }

    public enum Languages : int
    {
        Spanish = 1,
        Catalan = 2,
        French = 6,
        English = 7
    }
}
