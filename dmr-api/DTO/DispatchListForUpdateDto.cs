using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class DispatchListForUpdateDto
    {
        public int ID { get; set; }
        public int GlueNameID { get; set; }
        public double Amount { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }
    }
}
