using CoreBot.Store;
using Microsoft.Bot.Builder;
using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Extensions.Configuration;

namespace CoreBot.Extensions
{
    public static class RecognizerResultExtensions
    {
        public static SingleOrder ToSingleOrder(this RecognizerResult luisResult)
        {
            var singleOrder = new SingleOrder();
            singleOrder.Product = luisResult.Entities["Product"]?.FirstOrDefault()?.ToString();
            var quantity = luisResult.Entities["number"]?.FirstOrDefault();
            var dimension = luisResult.Entities["dimension"]?.FirstOrDefault();

            if (quantity != null)
            {
                singleOrder.Quantity = (int)quantity;
            }

            if (dimension != null)
            {
                singleOrder.Quantity = (int)dimension["number"];
                singleOrder.Dimension = dimension["units"].ToString();
            }

            return singleOrder;
        }


        public static ProductInfo ToProductInfo(this LuisResult luisResult, IConfiguration configuration)
        {
            var product = luisResult.Entities[0].Entity;

            // Parametritzem per evitar Sql injection (tot i que en aquest cas el resultat ve del LUIS i no de l'input de l'usuari).
            string queryString = "SELECT * FROM [dbo].[ProductInfo] WHERE ProductName = @productName";

            // Utilitzem el ConnectionString per evitar donar cap dada pública.
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                // Donem valor al paràmetre de la query
                command.Parameters.AddWithValue("@productName", product.ToLower());
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                // No trobem cap resultat
                if (!reader.HasRows)
                    return null;

                // La query ha retornat algun valor
                else if (reader.Read())
                {
                    // Creem l'objecte que contindrà l'informació
                    var productInfo = new ProductInfo
                    {
                        Name = reader.GetString(0),
                        DisplayText = reader.GetString(1),
                        StoreURL = reader.GetString(2),
                        ImageURL = reader.GetString(3),
                        Title = reader.GetString(4),
                    };

                    reader.Close();
                    connection.Close();

                    return productInfo;
                }

                // Tanquem el reader i la connexió abans de sortir.
                reader.Close();
                connection.Close();
            }

            return null;
        }
    }
}
