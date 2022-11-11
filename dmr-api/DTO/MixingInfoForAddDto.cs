using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class MixingInfoForAddDto
    {
        public int GlueID { get; set; }
        public string GlueName { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int BuildingID { get; set; }
        public int MixBy { get; set; }
        public List<MixingInfoDetailForAddDto> Details { get; set; }
    }

}
