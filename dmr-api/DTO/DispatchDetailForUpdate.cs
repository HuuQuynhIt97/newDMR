using DMR_API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class DispatchDetailForUpdateDto 
    {
        public int ID { get; set; }
        public double Amount { get; set; }
        public int  GlueNameID { get; set; }
        public int  LineID { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }
        public DateTime StartTimeOfPeriod { get; set; }
        public DateTime FinishTimeOfPeriod { get; set; }
    }
   

}
