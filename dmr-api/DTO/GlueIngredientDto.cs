using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class GlueIngredientDto
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public double Percentage { get; set; }
        public string Position { get; set; }
        public double Allow { get; set; }
        public Ingredient Ingredient { get; set; }
    }
   
}
