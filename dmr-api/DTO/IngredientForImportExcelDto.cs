using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class IngredientForImportExcelDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string SupplierName { get; set; }
        public DateTime CreatedDate { get; set; }= DateTime.Now;
        public DateTime ManufacturingDate { get; set; } = DateTime.MinValue;
        public string MaterialNO { get; set; }
        public string Unit { get; set; }
        public int SupplierID { get; set; }
        public string VOC { get; set; }
        public int CreatedBy { get; set; }
        public int ExpiredTime { get; set; }
        public int DaysToExpiration { get; set; }
        public bool isShow { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }= DateTime.MinValue;
        public double Real { get; set; }
        public double CBD { get; set; }
    }
}
