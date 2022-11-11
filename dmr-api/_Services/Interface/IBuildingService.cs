using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
   public interface IBuildingService : IECService<BuildingDto>
    {
        Task<IEnumerable<HierarchyNode<BuildingTreeDto>>> GetAllAsTreeView();
        Task<List<BuildingDto>> GetBuildings();
        Task<object> GetBuildingsForSetting();
        Task<object> CreateMainBuilding(BuildingDto buildingDto);
        Task<object> CreateSubBuilding(BuildingDto buildingDto);
        Task<bool> AddOrUpdateLunchTime(LunchTimeDto lunchTime);

        Task<object> GetAllBuildingType();

        Task<bool> CheckRoot();


    }
}
