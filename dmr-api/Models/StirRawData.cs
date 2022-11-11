using System;

namespace DMR_API.Models
{
    public class StirRawData
    {
        public StirRawData(int machineID, double rPM, string building, int duration, DateTime createdTime)
        {
            MachineID = machineID;
            RPM = Math.Round(rPM);
            Building = building;
            Duration = duration;
            CreatedTime = createdTime;
        }

        public int ID { get; set; }
        public int BuildingID { get; set; }
        public int MachineID { get; set; }
        public double RPM { get; set; }
        public string Building { get; set; }
        public int Duration { get; set; }
        public int Sequence { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}