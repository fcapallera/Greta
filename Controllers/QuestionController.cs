using CoreBot.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Controllers
{
    public class QuestionController
    {
        private readonly UserController UserController;
        private readonly IServiceProvider ServiceProvider;

        public QuestionController(IServiceProvider serviceProvider, UserController userController)
        {
            UserController = userController;
            ServiceProvider = serviceProvider;
        }

        public async Task AddQuestionAsync(string question, string botId)
        {
            var user = await UserController.GetUserByBotIdAsync(botId);

            using (var scope = ServiceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<GretaDBContext>();

                user.Naquestions.Add(new Naquestions()
                {
                    QuestionText = question
                });

                await db.SaveChangesAsync();
            }
        }
    }
}
