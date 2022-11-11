using DMR_API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class DispatchListDetail
    {
        public int ID { get; set; }
        public int DispatchListID { get; set; }
        public int MixingInfoID { get; set; }
        public DispatchList DispatchList { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
