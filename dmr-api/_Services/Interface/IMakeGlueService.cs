using System.Threading.Tasks;
using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;

namespace DMR_API._Services.Interface
{
    public interface IMakeGlueService
    {
        //Task<bool> CheckBrandExists(string brandId);
        Task<object> MakeGlue(string code);
        Task<object> GetAllGlues();
        Task<object> GetGlueWithIngredients(int glueid);
        Task<object> GetGlueWithIngredientByGlueCode(string code);
        Task<object> GetGlueWithIngredientByGlueID(int glueid);
        Task<object> GetGlueWithIngredientByGlueName(string glueName);
        object DeliveredHistory();
    }
}