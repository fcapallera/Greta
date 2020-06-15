// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CoreBot.Bots;
using CoreBot;
using CoreBot.Dialogs;
using Refit;
using CoreBot.Store;
using System;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using CoreBot.Models;
using System.Collections.Concurrent;
using Microsoft.Bot.Schema;
using CoreBot.Controllers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.BotBuilderSamples
{
    public class Startup
    {
        private const string BotOpenIdMetadataKey = "BotOpenIdMetadata";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            //Afegim el servei de QnA i LUIS
            services.AddSingleton<IBotServices, BotServices>();

            // Creem un ConversationState que ens ajudarà amb els nodes de conversa del TechSupport.
            services.AddSingleton<ConversationState>();

            services.AddSingleton<TechnicalAssistanceDialog>();

            services.AddSingleton<AskUserInfoDialog>();

            services.AddSingleton<OrderProductDialog>();

            services.AddSingleton<ConfirmOrderDialog>();

            services.AddSingleton<AddProductInfoDialog>();

            services.AddSingleton<UserValidationDialog>();

            services.AddSingleton<UserLoginDialog>();

            services.AddSingleton<CartToOrderDialog>();

            // The Dialog that will be run by the bot.
            services.AddSingleton<MainDialog>();

            services.AddSingleton<NotifyController>();

            services.AddSingleton<UserController>();

            services.AddSingleton<QuestionController>();

            services.AddSingleton<PurchaseController>();

            services.AddSingleton<PermissionDialog>();

            services.AddSingleton<CartToOrderDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, GretaBot<MainDialog>>();

            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            var apiKey = Configuration.GetSection("PrestashopSettings").GetSection("ApiKey").Value;
            var storeUrl = Configuration.GetSection("PrestashopSettings").GetSection("StoreUrl").Value;

            String encoded = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(apiKey));

            // Afegim la API i li donem una configuració.
            services.AddRefitClient<IPrestashopApi>(
                new RefitSettings
                {
                    ContentSerializer = new XmlContentSerializer()
                })
                .ConfigureHttpClient(c => new HttpClient(new UriQueryUnescapingHandler()))
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(storeUrl))
                .ConfigureHttpClient(c => c.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded));

            services.AddDbContext<GretaDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseMvc();
        }
    }


    public class UriQueryUnescapingHandler : DelegatingHandler
    {
        public UriQueryUnescapingHandler()
            : base(new HttpClientHandler()) { }
        public UriQueryUnescapingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        { }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri;
            //You could also simply unescape the whole uri.OriginalString
            //but i don´t recommend that, i.e only fix what´s broken
            var unescapedQuery = Uri.UnescapeDataString(uri.Query);

            var userInfo = string.IsNullOrWhiteSpace(uri.UserInfo) ? "" : $"{uri.UserInfo}@";
            var scheme = string.IsNullOrWhiteSpace(uri.Scheme) ? "" : $"{uri.Scheme}://";

            request.RequestUri = new Uri($"{scheme}{userInfo}{uri.Authority}{uri.AbsolutePath}{unescapedQuery}{uri.Fragment}");
            return base.SendAsync(request, cancellationToken);
        }
    }

}
