using System;
using System.Collections.Generic;
using System.Text;

namespace ScalingMachineService.Helpers
{
    public class AppSettings
    {
        public string PortName { get; set; }
        public int MachineID { get; set; }
        public int CycleTime { get; set; }
        public string SignalrUrl { get; set; }
        public string LogPath { get; set; }
    }
}
