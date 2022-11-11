using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class UserDto
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string EmployeeID { get; set; }
        public string Email { get; set; }
        public int SystemID { get; set; }
        public int UserRoleID { get; set; }
        public string Role { get; set; }
        public string RoleCode { get; set; }
        public int RoleID { get; set; }
        public string Building { get; set; }
        public string Line { get; set; }
        public int BuildingUserID { get; set; }
        public bool IsLock { get; set; }
        public byte[] PasswordHash { get; set; }
        public List<int> Buildings { get; set; }
        public List<int> Lines { get; set; }

        public List<BuildingDto> BuildingsData { get; set; }
        public List<BuildingDto> LinesData { get; set; }

        public byte[] PasswordSalt { get; set; }
    }
}
