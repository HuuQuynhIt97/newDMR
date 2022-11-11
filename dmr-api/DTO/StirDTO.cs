using dmr_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class StirDTO
    {
        public StirDTO()
        {
            this.CreatedTime = DateTime.Now;
        }
        public int ID { get; set; }
        public int MachineID { get; set; }
        public string GlueName { get; set; }
        public int BuildingID { get; set; }
        public int? SettingID { get; set; }
        public int MixingInfoID { get; set; }
        public double TotalMinutes { get; set; }
        public int RPM { get; set; }
        public GlueType GlueType { get; set; }
        public bool Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public int ActualDuration { get; set; }
        public int StandardDuration { get; set; }
        public DateTime StartScanTime { get; set; }
        public DateTime StartStiringTime { get; set; }
        public DateTime FinishStiringTime { get; set; }
    }
}
