using DMR_API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class DispatchDetailDto 
    {
        public int ID { get; set; }
        public double Amount { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public string LineName { get; set; }
        public int LineID { get; set; }
    }
    public class DispatchListDto
    {
        public int ID { get; set; }
        public int PlanID { get; set; }
        public int MixingInfoID { get; set; }
        public int GlueID { get; set; }
        public int GlueNameID { get; set; }
        public int BuildingID { get; set; }

        public int LineID { get; set; }
        public int BPFCID { get; set; }
        public string LineName { get; set; }
        public ColorCode ColorCode { get; set; }
        public List<string> LineNames { get; set; }
        public string GlueName { get; set; }
        public string Supplier { get; set; }
        public bool Status { get; set; }
        public bool IsEVA_UV { get; set; }

        public DateTime? StartDispatchingTime { get; set; }
        public DateTime? FinishDispatchingTime { get; set; }

        public DateTime StartTimeOfPeriod { get; set; }
        public DateTime FinishTimeOfPeriod { get; set; }

        public DateTime? PrintTime { get; set; }
    
        public double DeliveredAmount { get; set; }

        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }

        public bool IsDelete { get; set; }
        public DateTime DeleteTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public int DeleteBy { get; set; }
        public int CreatedBy { get; set; }
    }
    public class DispatchListForReturnDto
    {
        public DispatchListForReturnDto()
        {
        }
        public void DispatcherDetail(List<DispatchListDto> data, int doneTotal = 0, int todoTotal = 0, int delayTotal = 0, int total = 0)
        {
            Data = data;
            DispatchTotal = total;
            TodoDispatchTotal = todoTotal;
            DoneDispatchTotal = doneTotal;
            DelayDispatchTotal = delayTotal;
            var val = Math.Round(((double)doneTotal / total) * 100, 0);
            PercentageOfDoneDispatch = Double.IsNaN(val) ? 0 : val;
        }
        public void TodoDetail(List<DispatchListDto> data, int doneTotal = 0, int todoTotal = 0, int delayTotal = 0, int total = 0)
        {
            Data = data;
            DoneTotal = doneTotal;
            Total = total;
            TodoTotal = todoTotal;
            DelayTotal = delayTotal;
            var val = Math.Round(((double)doneTotal / total) * 100, 0);
            PercentageOfDone = Double.IsNaN(val) ? 0 : val;
        }
        public DispatchListForReturnDto(List<DispatchListDto> data, int doneTotal = 0, int todoTotal = 0, int delayTotal = 0, int total = 0)
        {
            Data = data;
            DoneDispatchTotal = doneTotal;
            DispatchTotal = total;
            TodoDispatchTotal = todoTotal;
            DelayDispatchTotal = delayTotal;
            var val = Math.Round(((double)doneTotal / total) * 100, 0);
            PercentageOfDoneDispatch = Double.IsNaN(val) ? 0 : val;
        }
        public List<DispatchListDto> Data { get; set; }
        public double Total { get; set; }
        public double DoneTotal { get; set; }
        public double TodoTotal { get; set; }
        public double DelayTotal { get; set; }
        public double PercentageOfDone { get; set; }

        public double DispatchTotal { get; set; }
        public double TodoDispatchTotal { get; set; }
        public double DoneDispatchTotal { get; set; }
        public double DelayDispatchTotal { get; set; }
        public double PercentageOfDoneDispatch { get; set; }
    }

}
