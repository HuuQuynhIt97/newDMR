using DMR_API.DTO;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
    public interface IStirService: IECService<StirDTO>
    {
        Task<List<StirDTO>> GetStirByMixingInfoID(int mixingInfoID);
        Task<Setting> ScanMachine(int buildingID, string scanValue);
        Task<bool> UpdateStartScanTime(int mixingInfoID);
        Task<Stir> UpdateStir(StirDTO model);
    }
}
