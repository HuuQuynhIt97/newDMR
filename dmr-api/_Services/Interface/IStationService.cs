using DMR_API.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
   public interface IStationService : IECService<StationDto>
    {
        Task<List<StationDto>> GetAllByPlanID(int planID);
        Task<bool> UpdateRange(List<StationDto> stationDtos);
        Task<bool> DeleteStation(int id);
        Task<bool> AddRange(List<StationDto> model);
    }
}
