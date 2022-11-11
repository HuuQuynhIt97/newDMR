using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class Subpackage
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int GlueNameID { get; set; }
        public int Position { get; set; }
        public int MixingInfoID { get; set; }
        public string GlueName { get; set; }
        public double Amount { get; set; }
        public DateTime? PrintTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public MixingInfo MixingInfo { get; set; }
        public int CreatedBy { get; set; }
    }
}
