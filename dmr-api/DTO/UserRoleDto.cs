using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class UserRoleDto
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public bool IsLock { get; set; }
    }
}
