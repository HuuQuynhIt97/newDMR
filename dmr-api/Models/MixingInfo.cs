using dmr_api.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class MixingInfo
    {
        public int ID { get; set; }
        public int GlueID { get; set; }
        public string GlueName { get; set; }
        public int BuildingID { get; set; }
        public string Code { get; set; }
        public int MixBy { get; set; }
        public DateTime ExpiredTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool Status { get; set; }
        public DateTime EstimatedTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? PrintTime { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }
        public Glue Glue { get; set; }
        public int GlueNameID { get; set; }
        [ForeignKey("GlueNameID")]
        public GlueName GlueLibrary { get; set; }
        public ICollection<Stir> Stirs { get; set; }
        public ICollection<MixingInfoDetail> MixingInfoDetails { get; set; }
        public ICollection<Dispatch> Dispatches { get; set; }

    }
}
