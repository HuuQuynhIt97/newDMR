using System.Collections.Generic;
using System.Threading.Tasks;
using DMR_API.Helpers;

namespace DMR_API._Services.Interface
{
    public interface IECService<T> where T : class
    {
        Task<bool> Add(T model);
        
        Task<bool> Update(T model);

        Task<bool> Delete(object id);

        Task<List<T>> GetAllAsync();

        Task<PagedList<T>> GetWithPaginations(PaginationParams param);

        Task<PagedList<T>> Search(PaginationParams param, object text);
        T GetById(object id);
    }
}