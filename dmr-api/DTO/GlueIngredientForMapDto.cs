using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class GlueIngredientForMapDto
    {
        public int IngredientID { get; set; }
        public int GlueID { get; set; }
        public string IngredientName { get; set; }
        public string Code { get; set; }
        public double Percentage { get; set; }
        public string Position { get; set; }
        public double Allow { get; set; }
        public int ExpiredTime { get; set; }
        public DateTime  CreatedDate { get; set; }
    }

    public class GlueIngredientParams
    {
        public string GlueName { get; set; }
        public int GlueID { get; set; }
        public List<GlueIngredientForMapDto> GlueIngredientForMapDto { get; set; }
    }

}
