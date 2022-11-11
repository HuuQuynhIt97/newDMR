using DMR_API.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
    public interface IKindService : IECService<KindDto>
    {
        Task<object> GetAllKindType();
    }
}
