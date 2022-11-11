using dmr_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class ToDoListForUpdateDto
    {
        public string GlueName { get; set; }
        public double Amount { get; set; }
        public int LineID { get; set; }
        public int BuildingID { get; set; }
        public int MixingInfoID { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }

        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }
        public List<Dispatch> Dispatches { get; set; }
    }
}
