using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DMR_API.Models;

namespace dmr_api.Models
{
    public class Dispatch
    {
        public int ID { get; set; }
        public double Amount { get; set; }
        public double? RemainingAmount { get; set; }
        public string Unit { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public DateTime? DeleteTime { get; set; }
        public DateTime CreatedTime { get; set; }

        public int GlueNameID { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }

        public int CreateBy { get; set; }
        public bool IsDelete { get; set; }
        public int DeleteBy { get; set; }
        public int? DispatchListID { get; set; }
        public int CreatedBy { get; set; }
        public int LineID { get; set; }
        [ForeignKey("LineID")]
        public Building Building { get; set; }
        public int MixingInfoID { get; set; }
        [ForeignKey("MixingInfoID")]
        public MixingInfo MixingInfo { get; set; }

    }
}