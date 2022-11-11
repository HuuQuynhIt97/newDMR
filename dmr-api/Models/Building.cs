using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class Building
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public LunchTime LunchTime { get; set; }
        public int? LunchTimeID { get; set; }
        public int Level { get; set; }
        public int? ParentID { get; set; }
        public int? KindID { get; set; }
        public int? BuildingTypeID { get; set; }
        public Kind Kind { get; set; }
        public BuildingType BuildingType { get; set; }
        public ICollection<Plan> Plans { get; set; }
        public ICollection<Setting> Settings { get; set; }
        public ICollection<PeriodMixing> PeriodMixingList { get; set; }
    }
}
