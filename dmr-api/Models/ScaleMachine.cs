using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class ScaleMachine
    {
        public int ID { get; set; }
        public string MachineType { get; set; }
        public string Unit { get; set; }
        public int BuildingID { get; set; }
        public int MachineID { get; set; }
    }
}
