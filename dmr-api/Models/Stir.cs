using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class Stir
    {
        public Stir()
        {
            this.CreatedTime = DateTime.Now;
        }
        public int ID { get; set; }
        public string GlueName { get; set; }
        public int? SettingID { get; set; }
        public Setting Setting { get; set; }
        public int RPM { get; set; }

        public int ActualDuration { get; set; }
        public int StandardDuration { get; set; }

        public bool Status { get; set; }
        public double TotalMinutes { get; set; }
        public int MixingInfoID { get; set; }
        public MixingInfo MixingInfo { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime StartScanTime { get; set; }

        public DateTime StartStiringTime { get; set; }
        public DateTime FinishStiringTime { get; set; }
    }
}
