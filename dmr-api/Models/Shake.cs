using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DMR_API.Models;

namespace dmr_api.Models
{
    public class Shake
    {
        public int ID { get; set; }
        public string ChemicalType { get; set; }
        public double StandardCycle { get; set; }
        public string ActualCycle { get; set; }
        public bool Status{ get; set; }
        public DateTime? StartTime  { get; set; }
        public DateTime? EndTime { get; set; }

        public int MixingInfoID { get; set; }
        public MixingInfo MixingInfo { get; set; }
    }
}