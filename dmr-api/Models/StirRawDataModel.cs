using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class StirRawDataModel
    {
        public StirRawDataModel(int machineID, double rPM, string building, int duration, DateTime createdTime)
        {
            MachineID = machineID;
            RPM = rPM;
            Building = building;
            Duration = duration;
            CreatedTime = createdTime;
        }

        [JsonProperty("m")]
        public int MachineID { get; set; }
        [JsonProperty("r")]
        public double RPM { get; set; }
        [JsonProperty("b")]
        public string Building { get; set; }
        [JsonProperty("d")]
        public int Duration { get; set; }
        [JsonProperty("t")]
        public DateTime CreatedTime { get; set; }
    }
}
