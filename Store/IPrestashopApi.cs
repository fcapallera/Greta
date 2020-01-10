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

        [Get("/customers?display=[id,id_lang,passwd,lastname,firstname,email,company,date_upd]&filter[id]={id}")]
        Task<CustomerCollection> GetCustomerById(int id);

        [Get("/customers?display=[id,id_lang,passwd,lastname,firstname,email,company,date_upd]&filter[firstname]=%[{firstname}]%&filter[lastname]=%[{lastname}]%")]
        Task<CustomerCollection> GetCustomerByFullName(string firstname, string lastname);

        [Get("/customers?display=[id,id_lang,passwd,lastname,firstname,email,company,date_upd]&filter[firstname]=%[{firstname}]%")]
        Task<CustomerCollection> GetCustomerByFirstName(string firstname);
    }
}
