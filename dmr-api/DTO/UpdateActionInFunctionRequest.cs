using System.Collections.Generic;
using dmr_api.DTO;

namespace DMR_API.DTO
{
    public class UpdateActionInFunctionRequest
    {
        public List<ActionInFunctionSystemDto> ActionInFunction { get; set; } = new List<ActionInFunctionSystemDto>();
    }
}
