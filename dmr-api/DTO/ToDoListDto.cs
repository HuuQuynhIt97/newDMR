using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class ToDoListDto
    {
        public ToDoListDto()
        {
        }

        public ToDoListDto(int glueID, int glueNameID,  string glueName, int planID, int buildingID,
            int lineID, string lineName, int bpfcID, int kindID, bool isEVA_UV, 
            string supplier, double stdCon , DateTime est, DateTime eft)
        {

            GlueID = glueID;
            GlueNameID = glueNameID;
            GlueName = glueName;
            PlanID = planID;
            BuildingID = buildingID;
            LineID = lineID;
            LineName = lineName;
            BPFCID = bpfcID;
            KindID = kindID;
            IsEVA_UV = isEVA_UV;
            Supplier = supplier;
            StandardConsumption = stdCon;
            EstimatedStartTime = est;
            EstimatedFinishTime = eft;
        }

        public int ID { get; set; }
        public int PlanID { get; set; }
        public int JobType { get; set; }
        public int MixingInfoID { get; set; }
        public int GlueID { get; set; }
        public int BuildingID { get; set; }
        public int LineID { get; set; }
        public int BPFCID { get; set; }
        public int GlueNameID { get; set; }
        public string LineName { get; set; }
        public bool IsEVA_UV { get; set; }
        public string GlueName { get; set; }
        public string Supplier { get; set; }
        public bool Status { get; set; }
        public bool AbnormalStatus { get; set; }
        public bool IsDelete { get; set; }
        public int KindID { get; set; }
        public List<string> LineNames { get; set; }
        public DateTime? StartMixingTime { get; set; }
        public DateTime? FinishMixingTime { get; set; }

        public DateTime? StartStirTime { get; set; }
        public DateTime? FinishStirTime { get; set; }

        public DateTime? StartDispatchingTime { get; set; }
        public DateTime? FinishDispatchingTime { get; set; }

        public DateTime? PrintTime { get; set; }
        public DateTime? DispatchTime { get; set; }

        public double StandardConsumption { get; set; }
        public double MixedConsumption { get; set; }
        public double DeliveredConsumption { get; set; }
        public double DeliveredAmount { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }
    }
    public class GlueForGenerateToDoListDto
    {
        public int WorkingHour { get; set; }
        public int HourlyOutput { get; set; }
        public DateTime FinishWorkingTime { get; set; }
        public DateTime StartWorkingTime { get; set; }
        public DateTime DueDate { get; set; }
        public Building Building { get; set; }
        public int PlanID { get; set; }
        public int BPFCID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Consumption { get; set; }
        public int GlueID { get; set; }
        public int BuildingKindID { get; set; }
        public string KindTypeCode { get; set; }
        public int KindID { get; set; }
        public int GlueKindID { get; set; }
        public int? GlueNameID { get; set; }
        public string GlueName { get; set; }
        public Ingredient ChemicalA { get; set; }
    }
    public class ToDoListForReturnDto
    {
        public ToDoListForReturnDto()
        {
        }
        public void DispatcherDetail(List<ToDoListDto> data, int doneTotal = 0, int todoTotal = 0, int delayTotal = 0, int total = 0)
        {
            Data = data;
            DispatchTotal = total;
            TodoDispatchTotal = todoTotal;
            DelayDispatchTotal = delayTotal;
            DoneDispatchTotal = doneTotal;
            DoneTotal = doneTotal;
            var val = Math.Round(((double)doneTotal / total) * 100, 0);
            PercentageOfDoneDispatch = Double.IsNaN(val) ? 0 : val;
        }
        public void DispatcherDetail(List<ToDoListDto> data, int doneTotal = 0, int todoTotal = 0, int delayTotal = 0, int EVA_UVTotal = 0, int total = 0)
        {
            Data = data;
            DispatchTotal = total;
            TodoDispatchTotal = todoTotal;
            DelayDispatchTotal = delayTotal;
            DoneDispatchTotal = doneTotal;
            DoneTotal = doneTotal;
            this.EVA_UVTotal = EVA_UVTotal;
            var val = Math.Round(((double)doneTotal / total) * 100, 0);
            PercentageOfDoneDispatch = Double.IsNaN(val) ? 0 : val;
        }

        public void TodoDetail(List<ToDoListDto> data, int doneTotal = 0, int todoTotal = 0, int delayTotal = 0, int total = 0)
        {
            Data = data;
            DoneTotal = doneTotal;
            Total = total;
            TodoTotal = todoTotal;
            DelayTotal = delayTotal;
            var val = Math.Round(((double)doneTotal / total) * 100, 0);
            PercentageOfDone = Double.IsNaN(val) ? 0 : val;
        }
        public void TodoDetail(List<ToDoListDto> data, int doneTotal = 0, int todoTotal = 0, int delayTotal = 0, int EVA_UVTotal = 0, int total = 0)
        {
            Data = data;
            DoneTotal = doneTotal;
            Total = total;
            TodoTotal = todoTotal;
            DelayTotal = delayTotal;
            this.EVA_UVTotal = EVA_UVTotal;
            var val = Math.Round(((double)doneTotal / total) * 100, 0);
            PercentageOfDone = Double.IsNaN(val) ? 0 : val;
        }
        public ToDoListForReturnDto(List<ToDoListDto> data, int doneTotal = 0, int todoTotal = 0, int delayTotal = 0, int total = 0)
        {
            Data = data;
            DoneTotal = doneTotal;
            Total = total;
            TodoTotal = todoTotal;
            DelayTotal = delayTotal;
            var val = Math.Round(((double)doneTotal / total) * 100, 0);
            PercentageOfDone = Double.IsNaN(val) ? 0 : val;
        }
        public ToDoListForReturnDto(List<ToDoListDto> data, int doneTotal = 0, int todoTotal = 0, int delayTotal = 0, int EVA_UVTotal = 0, int total = 0)
        {
            Data = data;
            DoneTotal = doneTotal;
            Total = total;
            TodoTotal = todoTotal;
            DelayTotal = delayTotal;
            this.EVA_UVTotal = EVA_UVTotal;
            var val = Math.Round(((double)doneTotal / total) * 100, 0);
            PercentageOfDone = Double.IsNaN(val) ? 0 : val;
        }
        public List<ToDoListDto> Data { get; set; }
        public double Total { get; set; }
        public double DoneTotal { get; set; }
        public double TodoTotal { get; set; }
        public double DelayTotal { get; set; }
        public double PercentageOfDone { get; set; }

        public double DispatchTotal { get; set; }
        public double TodoDispatchTotal { get; set; }
        public double DelayDispatchTotal { get; set; }
        public double DoneDispatchTotal { get; set; }
        public double PercentageOfDoneDispatch { get; set; }

        public int EVA_UVTotal { get; set; }
    }
}
