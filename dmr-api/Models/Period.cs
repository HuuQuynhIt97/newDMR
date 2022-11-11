using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class Period
    {
        public int ID { get; set; }
        public int LunchTimeID { get; set; }
        public int Sequence { get; set; }
        public bool IsOvertime { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }

        public int IsDelete { get; set; }

        public int CreatedBy { get; set; }
        public int DeletedBy { get; set; }
        public int UpdatedBy { get; set; }
        public List<PeriodDispatch> PeriodDispatches { get; set; }
    }
}
