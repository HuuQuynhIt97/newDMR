using dmr_api.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class Setting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Name { get; set; }
        public string MachineType { get; set; }
        public int? GlueTypeID { get; set; }
        [ForeignKey("GlueTypeID")]
        public GlueType GlueType { get; set; }
        public string MachineCode { get; set; }
        public int MinRPM { get; set; }
        public int MaxRPM { get; set; }
        public int BuildingID { get; set; }
        [ForeignKey("BuildingID")]
        public Building Building { get; set; }
    }
}
