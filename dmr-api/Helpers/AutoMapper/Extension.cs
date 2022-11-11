using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
namespace DMR_API.Helpers.AutoMapper
{
    public static class Extension
    {
        public static DateTime ExpriedTime(this MixingInfo mixing)
        {
            if (mixing.Glue.GlueIngredients.Count == 0) return DateTime.MinValue;
            if (mixing.Glue.GlueIngredients.FirstOrDefault(a => a.Position == "A") == null) return DateTime.MinValue;
            var ingredient = mixing.Glue.GlueIngredients.FirstOrDefault(a => a.Position == "A").Ingredient;
            if (ingredient.ExpiredTime == 600)
            {
                return DateTime.MinValue;
            }
            else
            {
                if (ingredient.CreatedDate.IsNullOrEmpty())
                {
                    return DateTime.MinValue;
                }

                var createdTime = mixing.CreatedTime;
                return createdTime.AddMinutes(ingredient.ExpiredTime);
            }
        }
    }
}