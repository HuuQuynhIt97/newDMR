using dmr_api.Models;
using DMR_API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class DispatchList
    {
        public int ID { get; set; }
        public int PlanID { get; set; }
        public Plan Plan { get; set; }
        public GlueName GlueLibrary { get; set; }
        public int MixingInfoID { get; set; }
        public int GlueID { get; set; } 
        public int GlueNameID { get; set; }
        public int BuildingID { get; set; } 

        public int LineID { get; set; }
        public int BPFCID { get; set; }
        public string LineName { get; set; }
        public string GlueName { get; set; }
        public string Supplier { get; set; }
        public bool Status { get; set; }
        public bool AbnormalStatus { get; set; }
        public ColorCode ColorCode { get; set; }

        public DateTime? StartDispatchingTime { get; set; }
        public DateTime? FinishDispatchingTime { get; set; }

        public DateTime? PrintTime { get; set; }

        public double DeliveredAmount { get; set; } 

        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }
        public DateTime StartTimeOfPeriod { get; set; }
        public DateTime FinishTimeOfPeriod { get; set; }
        public bool IsDelete { get; set; }
        public DateTime DeleteTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public int DeleteBy { get; set; }
        public int CreatedBy { get; set; }
        public ICollection<DispatchListDetail> DispatchListDetails { get; set; }
        public ICollection<Dispatch> Dispatches { get; set; }
    }
}
