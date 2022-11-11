using System;
using dmr_api.Models;

namespace DMR_API.Models
{
    public class Ingredient
    {
        public Ingredient()
        {
            this.CreatedDate = DateTime.Now.ToString("MMMM dd, yyyy HH:mm:ss tt");
        }
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string CreatedDate { get; set; }
        public string MaterialNO { get; set; }
        public string PartNO { get; set; }

        public int CreatedBy { get; set; }
        public bool isShow { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime ManufacturingDate { get; set; }

        public double Unit { get; set; }
        public double VOC { get; set; }
        public double ExpiredTime { get; set; }
        public double DaysToExpiration { get; set; }
        public double StandardCycle { get; set; }
        public double Real { get; set; }
        public double CBD { get; set; }
        public double ReplacementFrequency { get; set; }

        public int ModifiedBy { get; set; }
        public double PrepareTime { get; set; } 
        public int? GlueTypeID { get; set; }
        public GlueType GlueType { get; set; }
        public int SupplierID { get; set; }
        public Supplier Supplier { get; set; }


    }
}
