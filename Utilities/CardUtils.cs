using AdaptiveCards;
using CoreBot.Store;
using CoreBot.Store.Entity;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CoreBot.Utilities
{
    public static class CardUtils
    {
        public const string sameCardMsg = "Don't use the same Submit Card twice. If you want to submit new data ask for the Card again.";
        public const int ENGLISH = 7;

        public static Attachment CreateCardFromProductInfo(ProductInfo productInfo)
        {
            var processedDisplayText = productInfo.DisplayText.Replace("\\n", Environment.NewLine);
            var card = new AdaptiveCard("1.0");
            if (productInfo.ImageURL != null)
                card.Body.Add(new AdaptiveImage(url: productInfo.ImageURL));

            card.Body.Add(new AdaptiveTextBlock() { Text = productInfo.Title, Weight = AdaptiveTextWeight.Bolder });
            card.Body.Add(new AdaptiveTextBlock() { Text = processedDisplayText });
            if (productInfo.StoreURL != null)
                card.Actions.Add(new AdaptiveOpenUrlAction() { Title = "More information", Url = new Uri(productInfo.StoreURL) });
            var resposta = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
            return resposta;
        }


        public static List<Attachment> CarouselFromProducts(ProductCollection collection, IConfiguration configuration)
        {
            List<Product> products = collection.Products.ToList();
            List<Attachment> attachments = new List<Attachment>();
            var apiKey = configuration.GetSection("PrestashopSettings").GetSection("ApiKey").Value;
            Guid guid = Guid.NewGuid();
            int index = 0;


            foreach(Product product in products)
            {
                var heroCard = new HeroCard
                {
                    Title = $"<b>{product.GetNameByLanguage(ENGLISH)}</b>",
                    Text = product.GetDescriptionByLanguage(ENGLISH),
                    Images = new List<CardImage> { new CardImage(product.Image.Url+"?ws_key="+apiKey) },
                    Buttons = new List<CardAction> { new CardAction {
                        Type = ActionTypes.PostBack,
                        Title = "Choose", 
                        Value = $@"{{ ""id"" : ""{guid.ToString()}"", ""action"" : ""{index}""}}"
                        }
                    }
                };

                attachments.Add(heroCard.ToAttachment());

                index++;
            }

            return attachments;
        }

        public static Attachment CreateCardFromOrder(UserProfile userProfile)
        {
            var card = CreateCardFromJson("confirmOrderCard");

            //Ara hem convertit el JSON a un AdaptiveCard i editarem els fragments que ens interessen.

            //Primer editem el FactSet (informació de l'usuari que sortirà a la fitxa).
            var containerFact = (card.Body[1] as AdaptiveContainer);
            var factSet = (containerFact.Items[1] as AdaptiveFactSet);
            factSet.Facts.Add(new AdaptiveFact("Ordered by:", userProfile.Name));
            factSet.Facts.Add(new AdaptiveFact("Company:", userProfile.Company));

            //Ara editarem la informació que sortirà dels productes
            var containerProducts = (card.Body[3] as AdaptiveContainer);

            userProfile.ProductCart.Products.RemoveAll(item => item == null);

            foreach (SingleOrder order in userProfile.ProductCart.Products)
            {
                AdaptiveColumnSet columns = new AdaptiveColumnSet();
                AdaptiveColumn productColumn = new AdaptiveColumn();

                AdaptiveTextBlock product = new AdaptiveTextBlock(order.Product);
                product.Wrap = true;

                productColumn.Width = "stretch";
                productColumn.Items.Add(product);
                columns.Columns.Add(productColumn);

                AdaptiveColumn amountColumn = new AdaptiveColumn();

                AdaptiveTextBlock amount = new AdaptiveTextBlock(order.AmountToString());
                amount.Wrap = true;

                amountColumn.Width = "auto";
                amountColumn.Items.Add(amount);
                columns.Columns.Add(amountColumn);

                containerProducts.Items.Add(columns);
            }

            var attachment = new Attachment()
            {
                Content = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(card)),
                ContentType = "application/vnd.microsoft.card.adaptive"
            };

            return attachment;
        }

        static public AdaptiveCard CreateCardFromJson(string json)
        {
            string[] paths = { ".", "Cards", json+".json" };
            var cardJson = File.ReadAllText(Path.Combine(paths));
            var processedJson = AddGuidToJson(cardJson);
            return AdaptiveCard.FromJson(processedJson).Card;
        }

        static public Attachment AdaptiveCardToAttachment(AdaptiveCard card)
        {
            return new Attachment
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive"
            };
        }

        static public string AddGuidToJson(string json)
        {
            string regex = @"submitButton[0-9]";
            Regex r = new Regex(regex);
            Guid g = Guid.NewGuid();
            var replaced = r.Replace(json, g.ToString());

            return replaced;
        }
    }
}
