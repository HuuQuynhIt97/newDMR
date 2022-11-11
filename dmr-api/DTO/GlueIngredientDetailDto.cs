using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class GlueIngredientDetailDto
    {
        public int ID { get; set; }
        public int GlueID { get; set; }
        public int IngredientID { get; set; }
        public string GlueName { get; set; }
        public string IngredientName { get; set; }
        public string Position { get; set; }
        public double Percentage { get; set; }
        public double Allow { get; set; }
        public double ExpiredTime { get; set; }
        public string CreatedDate { get; set; }
    }
}
