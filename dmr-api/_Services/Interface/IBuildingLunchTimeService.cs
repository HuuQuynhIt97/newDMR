using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
    public interface IBuildingLunchTimeService 
    {
        Task<List<PeriodMixing>> GetPeriodMixingByBuildingID( int buildingID);
        Task<ResponseDetail<object>> UpdatePeriodMixing(PeriodMixing period);
        Task<ResponseDetail<object>> AddPeriodMixing(PeriodMixing period);
        Task<ResponseDetail<object>> DeletePeriodMixing(int id);

        Task<ResponseDetail<object>> AddLunchTimeBuilding(Building building);

        Task<List<PeriodDispatch>> GetPeriodDispatchByPeriodMixingID(int periodMixingID);
        Task<ResponseDetail<object>> UpdatePeriodDispatch(PeriodDispatch period);
        Task<ResponseDetail<object>> AddPeriodDispatch(PeriodDispatch period);
        Task<ResponseDetail<object>> DeletePeriodDispatch(int id);

    }
}
