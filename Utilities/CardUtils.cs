using AdaptiveCards;
using CoreBot.Store;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.IO;

namespace CoreBot.Utilities
{
    public static class CardUtils
    {
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

        public static Attachment CreateCardFromOrder(UserProfile userProfile)
        {
            string[] paths = { ".", "Cards", "confirmOrderCard.json" };
            var cardJson = File.ReadAllText(Path.Combine(paths));
            var card = AdaptiveCard.FromJson(cardJson).Card;

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
    }
}
