using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class BuildingUserDto
    {
        public BuildingUserDto()
        {
            CreatedDate = DateTime.Now;
        }

        public int UserID { get; set; }
        public int BuildingID { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class BuildingUserForRemoveDto
    {
        public int UserID { get; set; }
        public List<int> Buildings { get; set; }
    }
    public class BuildingUserForMapDto
    {
        public int UserID { get; set; }
        public List<int> Buildings { get; set; }
    }
}
