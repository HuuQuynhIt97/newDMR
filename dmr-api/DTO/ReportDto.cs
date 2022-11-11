using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class ReportBodyDto
    {
        public int Day { get; set; }
        public double CBD { get; set; }
        public double Real { get; set; }
        public string ModelName { get; set; }
        public string ModelNo { get; set; }
        public string ArticleNO { get; set; }
        public string Process { get; set; }
        public int Quantity { get; set; }
        public string Line { get; set; }
        public int LineID { get; set; }
        public DateTime Date { get; set; }
        public List<double> Ingredients { get; set; } = new List<double>();
        public List<IngredientBodyReportDto> Ingredients2 { get; set; } = new List<IngredientBodyReportDto>();
    }
    public class IngredientBodyReportDto
    {
        public double Value { get; set; }
        public string Name { get; set; }
        public string Line { get; set; }
    }
    public class ReportHeaderDto
    {
        public string Day { get; set; } = "Day";
        public string Date { get; set; } = "Date";
        public string ModelName { get; set; } = "Model Name";
        public string ModelNo { get; set; } = "Model NO";
        public string ArticleNO { get; set; } = "Article NO";
        public string Process { get; set; } = "Process";
        public string Quantity { get; set; } = "Qty";
        public string Line { get; set; } = "Line";
        public string CBD { get; set; } = "CBD U$";
        public string Real { get; set; } = "Real U$";
        public List<string> Ingredients { get; set; }
    }
  
    public class IngredientReportDto
    {
        public double Real { get; set; }
        public double CBD { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
    }
    public class GetReportDto
    {
     
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int BuildingID { get; set; }

    }
}
