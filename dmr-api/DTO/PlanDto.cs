using DMR_API.Helpers;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class PlanDto
    {
        public int ID { get; set; }
        public int BuildingID { get; set; }
        public string BuildingName { get; set; }
        public string ModelName { get; set; }
        public string ModelNoName { get; set; }
        public string BPFCName { get; set; }

        public string LineKind { get; set; }
        public string ArticleName { get; set; }
        public string ProcessName { get; set; }
        public List<string> Glues { get; set; }
        public List<string> Kinds { get; set; }
        public int BPFCEstablishID { get; set; }
        public int HourlyOutput { get; set; }
        public int WorkingHour { get; set; }
        public int ModelNameID { get; set; }
        public int ModelNoID { get; set; }
        public int Quantity { get; set; }
        public int ArticleNoID { get; set; }
        public int ArtProcessID { get; set; }
        public bool IsGenerate { get; set; }
        public bool IsChangeBPFC { get; set; }
        public bool IsOvertime { get; set; }
        public bool IsOffline { get; set; }// v102

        public TimeDto StartTime { get; set; }
        public TimeDto EndTime { get; set; }

        public int UpdatedBy { get; set; }

        public int UpdatedOfflineBy { get; set; }// v102
        public int UpdatedOnlineBy { get; set; }// v102

        public int UpdatedOvertimeBy { get; set; }// v102
        public int UpdatedNoOvertimeBy { get; set; }// v102

        public DateTime? UpdatedOffline { get; set; }// v102
        public DateTime? UpdatedOnline { get; set; }// v102

        public DateTime? UpdatedOvertime { get; set; }// v102
        public DateTime? UpdatedNoOvertime { get; set; }// v102

        public DateTime? UpdatedTime { get; set; }// v102


        public DateTime DueDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime StartWorkingTime { get; set; }
        public DateTime FinishWorkingTime { get; set; }
    }
    public class ExportExcelPlanDto
    {
        public string Building { get; set; }
        public string Line { get; set; }
        public string ModelName { get; set; }
        public string ModelNo { get; set; }
        public string BPFCName { get; set; }
        public string ArticleNO { get; set; }

        public string ArticleName { get; set; }
        public string ProcessName { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class ConsumtionDto
    {
        public int ID { get; set; }
        public string ModelName { get; set; }
        public string ModelNo { get; set; }
        public string ArticleNo { get; set; }
        public string Process { get; set; }
        public string Glue { get; set; }
        public float Std { get; set; }
        public int Qty { get; set; }
        public string Line { get; set; }
        public int LineID { get; set; }
        public float TotalConsumption { get; set; }
        public float RealConsumption { get; set; }
        public float Diff { get; set; }
        public float Percentage { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime MixingDate { get; set; }

    }
    public class TimeDto
    {
        public TimeDto(DateTime dt)
        {
            var dateTime = dt;
            Hour = dateTime.ToString("HH").ToInt();
            Minute = dateTime.ToString("mm").ToInt();
        }

        public int Hour { get; set; }
        public int Minute { get; set; }
    }
    public class AchievementDto
    {
        public string Building { get; set; }
        public double UpdateOnTime { get; set; }
        public double Total { get; set; }
        public double AchievementRate { get; set; }
      

    }
    public class TodolistDto
    {
        public int ID { get; set; }
        public int PlanID { get; set; }
        public int GlueID { get; set; }
        public int LineID { get; set; }
        public string LineName { get; set; }
        public int BuildingID { get; set; }
        public int PrepareTime { get; set; }
        public int MixingInfoID { get; set; }
        public double StandardConsumption { get; set; }
        public List<string> Lines { get; set; }
        public List<string> BPFCs { get; set; }
        public string Glue { get; set; }
        public List<MixingInfoTodolistDto> MixingInfoTodolistDtos { get; set; }
        public string Supplier { get; set; }
        public string DeliveredActual { get; set; }
        public bool Status { get; set; }
        public bool AbnormalStatus { get; set; }
        public DateTime EstimatedTime { get; set; }
        public List<DateTime> EstimatedTimes { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }

        public double MixedConsumption { get; set; }
        public double DeliveredConsumption { get; set; }

    }
    public class DispatchParams {
        public string Glue { get; set; }
        public int ID { get; set; }
        public int MixingInfoID { get; set; }
        public DateTime EstimatedTime { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }

        public DateTime StartDispatchingTime { get; set; }
        public DateTime FinishDispatchingTime { get; set; }
        public List<string> Lines { get; set; }

    }
    public class MixingInfoTodolistDto
    {
    public int ID { get; set; }
    public string Glue { get; set; }
    public bool Status { get; set; }
    public DateTime EstimatedStartTime { get; set; }
    public DateTime EstimatedFinishTime { get; set; }

}
    public class GlueTodolistDto
    {
        public int ID { get; set; }
        public Glue Glue { get; set; }
        public int HourlyOutput { get; set; }
        public int WorkingHour { get; set; }
        public Building Building { get; set; }
        public string DeliverActual { get; set; }
        public bool Status { get; set; }
        public DateTime EstimatedTime { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }

    }
    public class DispatchTodolistDto
    {
        public int ID { get; set; }
        public int StationID { get; set; }
        public int DeleteBy { get; set; }
        public int CreateBy { get; set; }
        public double StandardAmount { get; set; }
        public double MixedConsumption { get; set; }
        public string Line { get; set; }
        public string Glue { get; set; }
        public int LineID { get; set; }
        public bool IsDelete { get; set; }
        public int MixingInfoID { get; set; }
        public DateTime EstimatedTime { get; set; }
        public double Real { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? DeliveryTime { get; set; }


    }
    public class ExcelExportDto
    {
        public int buildingID { get; set; }
        public List<int> Plans { get; set; }
    }
}
