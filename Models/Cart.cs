using AdaptiveCards;
using CoreBot.Store;
using CoreBot.Utilities;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreBot.Models
{
    public partial class Cart
    {
        public Cart()
        {
            OrderLine = new HashSet<OrderLine>();
            OrderRequest = new HashSet<OrderRequest>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public bool Active { get; set; }

        public UserProfile User { get; set; }
        public ICollection<OrderLine> OrderLine { get; set; }
        public ICollection<OrderRequest> OrderRequest { get; set; }


        public async Task<Attachment> ToAdaptiveCard(IPrestashopApi prestashopApi)
        {
            var card = CardUtils.CreateCardFromJson("confirmOrderCard");

            var user = (await prestashopApi.GetCustomerById(UserId)).First();

            //Ara hem convertit el JSON a un AdaptiveCard i editarem els fragments que ens interessen.

            //Primer editem el FactSet (informació de l'usuari que sortirà a la fitxa).
            var containerFact = (card.Body[1] as AdaptiveContainer);
            var factSet = (containerFact.Items[1] as AdaptiveFactSet);
            factSet.Facts.Add(new AdaptiveFact("Ordered by:", user.GetFullName()));
            factSet.Facts.Add(new AdaptiveFact("Company:", user.Company));

            //Ara editarem la informació que sortirà dels productes
            var containerProducts = (card.Body[3] as AdaptiveContainer);

            foreach (OrderLine orderLine in OrderLine)
            {
                AdaptiveColumnSet columns = new AdaptiveColumnSet();
                AdaptiveColumn productColumn = new AdaptiveColumn();

                var product = (await prestashopApi.GetProductById(orderLine.ProductId)).First();

                AdaptiveTextBlock productText = new AdaptiveTextBlock(product.GetNameByLanguage(Languages.English));
                productText.Wrap = true;

                productColumn.Width = "stretch";
                productColumn.Items.Add(productText);
                columns.Columns.Add(productColumn);

                AdaptiveColumn amountColumn = new AdaptiveColumn();

                AdaptiveTextBlock amount = new AdaptiveTextBlock(orderLine.Amount.ToString());
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
