using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class StationDto
    {

        public int ID { get; set; }
        public int Amount { get; set; }

        public int GlueID { get; set; }
        public string GlueName { get; set; }
        public int PlanID { get; set; }

        public bool IsDelete { get; set; }
        public int CreateBy { get; set; }
        public int DeleteBy { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? DeleteTime { get; set; }
        public DateTime? ModifyTime { get; set; }
    }
}
