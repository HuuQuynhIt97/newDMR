using DMR_API.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
   public interface IServiceBase<T, TDto> 
    {
        Task<bool> Add(TDto model);

        Task<bool> Update(TDto model);

        Task<bool> Delete(object id);

        Task<List<TDto>> GetAllAsync();

        Task<PagedList<TDto>> GetWithPaginations(PaginationParams param);

        Task<PagedList<TDto>> Search(PaginationParams param, object text);
        TDto GetById(object id);
    }
}
