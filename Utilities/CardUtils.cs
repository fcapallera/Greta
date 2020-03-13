using AdaptiveCards;
using CoreBot.Dialogs;
using CoreBot.Models;
using CoreBot.Store;
using CoreBot.Store.Entity;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoreBot.Utilities
{
    /// <summary>
    /// Collection of static methods for custom Card operations.
    /// </summary>
    public static class CardUtils
    {
        public const string sameCardMsg = "Don't use the same Submit Card twice. If you want to submit new data ask for the Card again.";

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

        public static T GetValueFromAction<T>(string json) where T : IConvertible
        {
            var jobject = JObject.Parse(json);
            return (T)Convert.ChangeType(jobject["action"], typeof(T), CultureInfo.InvariantCulture);
        }

        public static string GetGuidFromResult(string card)
        {
            var jobject = JObject.Parse(card);
            return (string)jobject["id"];
        }

        public static AdaptiveCard RequestedInfoToCard(RequestedInfo requestedInfo)
        {
            var card = new AdaptiveCard("1.0");
            card.Body.Add(new AdaptiveTextBlock
            {
                Text = requestedInfo.Name,
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Large,
                Wrap = true
            });

            card.Body.Add(new AdaptiveTextBlock
            {
                Text = requestedInfo.ProductList,
                Wrap = true
            });

            return card;
        }

        public static List<Attachment> RequestedListToCarousel(List<RequestedInfo> requestedList)
        {
            var guid = Guid.NewGuid();
            var attachmentList = new List<Attachment>();

            foreach(RequestedInfo info in requestedList)
            {
                var card = new AdaptiveCard("1.0");
                card.Body.Add(new AdaptiveTextBlock
                {
                    Text = info.Name,
                    Weight = AdaptiveTextWeight.Bolder,
                    Size = AdaptiveTextSize.Large,
                    Wrap = true
                });

                card.Body.Add(new AdaptiveTextBlock
                {
                    Text = info.ProductList,
                    Wrap = true
                });

                card.Actions.Add(new AdaptiveSubmitAction
                {
                    Title = "SELECT",
                    DataJson = $@"{{ ""id"" : ""{guid.ToString()}"", ""action"" : ""{info.OrderRequestId}""}}"
                });

                var attachment = new Attachment()
                {
                    Content = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(card)),
                    ContentType = "application/vnd.microsoft.card.adaptive"
                };

                attachmentList.Add(attachment);
            }

            return attachmentList;
        }

        static async public Task<Attachment> CreatePriceAssignationCard(Models.Cart cart, IPrestashopApi prestashopApi)
        {
            var card = CreateCardFromJson("prizeAssignationCard");

            var container = card.Body[3] as AdaptiveContainer;

            int index = 0;

            foreach(OrderLine line in cart.OrderLine)
            {
                var product = (await prestashopApi.GetProductById(line.ProductId)).First();

                var productTitle = new AdaptiveTextBlock
                {
                    Text = "**" + product.GetNameByLanguage(7) + "**",
                    Weight = AdaptiveTextWeight.Bolder,
                    Wrap = true
                };

                var columnSet = new AdaptiveColumnSet();

                var column = new AdaptiveColumn
                {
                    Width = AdaptiveColumnWidth.Stretch
                };

                var reference = new AdaptiveTextBlock
                {
                    Text = product.Reference,
                    Wrap = true
                };

                column.Items.Add(reference);

                var columnInput = new AdaptiveColumn
                {
                    Width = AdaptiveColumnWidth.Auto
                };

                var input = new AdaptiveNumberInput
                {
                    Id = "InputCount" + index,
                    Placeholder = "Price"
                };
                column.Items.Add(input);
                columnSet.Columns.Add(column);
                columnSet.Columns.Add(columnInput);
                container.Items.Add(columnSet);
                
                index++;
            }

            return new Attachment()
            {
                Content = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(card)),
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
