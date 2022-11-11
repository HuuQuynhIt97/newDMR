using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class ReportParams
    {
        public ReportParams()
        {
            StartDate = DateTime.Now.Date;
            EndDate = DateTime.Now.Date;
        }

        public int BuildingID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
