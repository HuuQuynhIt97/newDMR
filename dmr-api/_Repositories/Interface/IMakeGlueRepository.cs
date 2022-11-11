using System.Threading.Tasks;
using DMR_API.Data;
using DMR_API.Models;

namespace DMR_API._Repositories.Interface
{
    public interface IMakeGlueRepository
    {
        Task<object> MakeGlue(int glueid);
        Task<object> MakeGlue(string code);
        Task<object> GetGlueWithIngredientByGlueCode(string code);
        Task<object> GetGlueWithIngredientByGlueID(int glueid);
        Task<object> GetGlueWithIngredientByGlueName(string glueName);
        //viet them ham o day neu chua co trong ECRepository
    }
}