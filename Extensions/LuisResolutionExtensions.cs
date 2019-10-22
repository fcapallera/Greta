using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreBot.Extensions
{
    public static class LuisResolutionExtensions
    {
        public static string ProcessProduct(this EntityModel entity)
        {
            if(entity.AdditionalProperties.TryGetValue("resolution", out dynamic resolution))
            {
                var resolutionValues = (IEnumerable<dynamic>)resolution.values;
                return resolutionValues.Select(product => product).FirstOrDefault();
            }

            throw new Exception("ProcessProduct");
        }
    }
}
