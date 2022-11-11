using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class BuildingDto
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string LunchTime { get; set; }
        public bool Status { get; set; }
        public bool IsSTF { get; set; }
        public int Level { get; set; }
        public int? LunchTimeID { get; set; }
        public int? ParentID { get; set; }
        public int? KindID { get; set; }
        public string KindName { get; set; }
        public int? BuildingTypeID { get; set; }
        public string BuildingTypeName { get; set; }
        public BuildingType BuildingType { get; set; }
    }

    public class BuildingTreeDto
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string LunchTime { get; set; }
        public bool Status { get; set; }
        public bool IsSTF { get; set; }
        public int Level { get; set; }
        public int? LunchTimeID { get; set; }
        public int? ParentID { get; set; }
        public int? KindID { get; set; }
        public string KindName { get; set; }
        public int? BuildingTypeID { get; set; }
        public string BuildingTypeName { get; set; }
        public BuildingType BuildingType { get; set; }
    }
}
