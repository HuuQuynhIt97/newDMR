using System.Threading.Tasks;
using DMR_API.Data;
using DMR_API.Models;

namespace DMR_API._Repositories.Interface
{
    public interface IIngredientInfoRepository : IECRepository<IngredientInfo>
    {
        Task<bool> CheckBarCodeExists(string code);
        Task<bool> CheckExists(int id);
        //viet them ham o day neu chua co trong ECRepository
    }
}