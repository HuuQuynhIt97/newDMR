using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class Station
    {
        public int ID { get; set; }
        public int Amount { get; set; }

        public int GlueID { get; set; }
        public int PlanID { get; set; }

        public bool IsDelete { get; set; }
        public int CreateBy { get; set; }
        public int DeleteBy { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? DeleteTime { get; set; }
        public DateTime? ModifyTime { get; set; }
        public Plan Plan { get; set; }
        public Glue Glue { get; set; }

    }
}
