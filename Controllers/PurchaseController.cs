using CoreBot.Models;
using CoreBot.Store;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Controllers
{
    public class PurchaseController
    {
        private readonly UserController UserController;
        private readonly IPrestashopApi PrestashopApi;
        private readonly IServiceProvider ServiceProvider;

        public PurchaseController(UserController userController, IPrestashopApi prestashopApi,
            IServiceProvider serviceProvider)
        {
            UserController = userController;
            PrestashopApi = prestashopApi;
            ServiceProvider = serviceProvider;
        }

        public async Task<Cart> GetActiveCartFromUser(string botId)
        {
            var user = await UserController.GetUserByBotIdAsync(botId);

            return user.Cart.Where(c => c.Active).SingleOrDefault();
        }

        public async Task<bool> HasCart(string botId)
        {
            var user = await UserController.GetUserByBotIdAsync(botId);

            if (user == null) throw new ArgumentNullException("User doesn't exist");

            var carts = await PrestashopApi.GetCartsByCustomer(user.PrestashopId.Value);

            return await Task.FromResult(carts.Carts.Count > 0);
        }

        public async Task AddOrderLineToUser(string botId, OrderLine orderLine)
        {
            var user = await UserController.GetUserByBotIdAsync(botId);

            using (var context = ServiceProvider.CreateScope())
            {
                var db = context.ServiceProvider.GetRequiredService<GretaDBContext>();

                var latestCart = user.Cart.OrderByDescending(c => c.Id).FirstOrDefault();

                if (latestCart.Active)
                {
                    latestCart.OrderLine.Add(orderLine);
                }
                else
                {
                    var newCart = new Cart()
                    {
                        Active = true
                    };
                    newCart.OrderLine.Add(orderLine);
                    user.Cart.Add(newCart);
                }

                await db.SaveChangesAsync();
            }
        }

        public async Task<List<OrderRequest>> GetOrderRequests()
        {
            using (var context = ServiceProvider.CreateScope())
            {
                var db = context.ServiceProvider.GetRequiredService<GretaDBContext>();

                var pending = db.OrderRequest.Where(o => o.Confirmed == false);

                return await Task.FromResult(pending.ToList());
            }
        }

        public async Task InactivateCartFromUser(string botId)
        {
            var lastCart = await GetActiveCartFromUser(botId);

            using(var scope = ServiceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<GretaDBContext>();

                lastCart.Active = false;

                await db.SaveChangesAsync();
            }
        }

        public async Task<List<Cart>> GetCartFromUserList(List<Store.Entity.Customer> customerList)
        {
            var ids = customerList.Select(c => c.Id);

            using(var scope = ServiceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<GretaDBContext>();

                IEnumerable<Cart> carts = db.Cart.Where(cart => ids.Any(id => id.Equals(cart.UserId)));

                return carts.ToList();
            }
        }

        public async Task<Cart> GetCartById(int id)
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<GretaDBContext>();

                return await Task.FromResult(db.Cart.Where(c => c.Id == id).SingleOrDefault());
            }
        }

    }
}
