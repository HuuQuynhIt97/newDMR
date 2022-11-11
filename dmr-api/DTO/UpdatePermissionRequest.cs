using System.Collections.Generic;
using dmr_api.DTO;

namespace DMR_API.DTO
{
    public class UpdatePermissionRequest
    {
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }
}
