using DMR_API._Services.Interface;
using DMR_API.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
   public interface IUserRoleService : IECService<UserRoleDto>
    {
        Task<object> MappingUserRole(UserRoleDto userRoleDto);
        Task<object> RemoveUserRole(UserRoleDto userRoleDto);
        Task<List<UserRoleDto>> GetUserRoleByUserID(int userID);
        Task<object> GetRoleByUserID(int userid);
        Task<object> MapUserRole(int userID, int roleID);
        Task<bool> Lock(UserRoleDto userRoleDto);
        Task<bool> IsLock(UserRoleDto userRoleDto);
    }
}
