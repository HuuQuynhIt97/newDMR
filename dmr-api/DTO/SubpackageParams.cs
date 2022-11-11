using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class SubpackageParam
    {
       public List<Subpackage> Subpackages { get; set; }
        public string PartNO { get; set; }
        public string BatchNO { get; set; }
        public int GlueID { get; set; }
        public int MixingInfoID { get; set; }
        public string MixingInfoCode { get; set; }
        public Ingredient Ingredient { get; set; }
        
        
        public string GlueName { get; set; }
        public int GlueNameID { get; set; }
        public int BuildingID { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }
    }
}
