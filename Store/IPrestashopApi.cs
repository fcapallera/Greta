using CoreBot.Store.Entity;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Store
{
    public interface IPrestashopApi
    {
        [Get("/products?display=[id,name,description,id_default_image]&filter[name]=%[{product}]%")]
        Task<ProductCollection> GetProductByName(string product);
    }
}
