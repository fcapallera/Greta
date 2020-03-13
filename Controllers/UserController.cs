using CoreBot.Dialogs;
using CoreBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Controllers
{
    public class UserController
    {
        private readonly IServiceProvider ServiceProvider;

        public UserController(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        //TO_DO TFG EXPLICAR FUNCIONS ASÍNCRONES

        public async Task AddUserAsync(Models.UserProfile userProfile)
        {
            using (var context = ServiceProvider.CreateScope())
            {
                var db = context.ServiceProvider.GetRequiredService<GretaDBContext>();

                db.Add(userProfile);
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<UserProfile>> GetUsersByPermissionLevelAsync(PermissionLevels permissionLevels)
        {
            var permission = (int)permissionLevels;

            using (var context = ServiceProvider.CreateScope())
            {
                var db = context.ServiceProvider.GetRequiredService<GretaDBContext>();

                return await db.UserProfile.Where(u => u.Permission == permission && u.BotUserId != null).ToListAsync();
            }
        }


        public async Task<bool> CheckForUserProfileAsync(int prestaId)
        {
            var user = await GetUserByPrestashopIdAsync(prestaId);
            return await Task.FromResult(user != null);
        }


        public async Task<bool> CheckForUserProfileAsync(string botId)
        {
            var user = await GetUserByBotIdAsync(botId);
            return await Task.FromResult(user != null);
        }


        public async Task<UserProfile> GetUserByPrestashopIdAsync(int prestashopId)
        {
            using (var context = ServiceProvider.CreateScope())
            {
                var db = context.ServiceProvider.GetRequiredService<GretaDBContext>();

                return await db.UserProfile.Where(u => u.PrestashopId == prestashopId).FirstOrDefaultAsync();
            }
        }

        // If no user is found, a null object is returned instead
        public async Task<UserProfile> GetUserByBotIdAsync(string botId)
        {
            using (var context = ServiceProvider.CreateScope())
            {
                var db = context.ServiceProvider.GetRequiredService<GretaDBContext>();

                return await db.UserProfile.Where(u => u.BotUserId == botId).FirstOrDefaultAsync();
            }
        }


        public async Task ValidateUserAsync(int prestashopId, int permission)
        {
            using (var context = ServiceProvider.CreateScope())
            {
                var db = context.ServiceProvider.GetRequiredService<GretaDBContext>();

                var user = await db.UserProfile.Where(u => u.PrestashopId == prestashopId).FirstOrDefaultAsync();

                user.Validated = true;
                user.Permission = permission;

                await db.SaveChangesAsync();
            }
        }
    }
}
