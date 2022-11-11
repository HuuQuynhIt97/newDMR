using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class LunchTimeDto
    {
        public int ID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Content { get; set; }

        public int CreatedBy { get; set; }
        public int DeletedBy { get; set; }
        public int UpdatedBy { get; set; }

        public int IsDelete { get; set; }

        public DateTime CreatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
