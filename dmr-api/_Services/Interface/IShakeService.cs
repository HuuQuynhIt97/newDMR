using dmr_api.Models;
using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
   public interface IShakeService : IECService<Shake>
    {
        Task<ResponseDetail<object>> GenerateShakes(int mixingInfoID);
        Task<ResponseDetail<object>> GetShakesByMixingInfoID(int mixingInfoID);

    }
}
