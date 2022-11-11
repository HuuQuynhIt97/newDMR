using System.Threading.Tasks;
using DMR_API._Repositories.Interface;
using DMR_API.Data;
using DMR_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using DMR_API.DTO;
using System.Collections.Generic;

namespace DMR_API._Repositories.Repositories
{
    public class IngredientInfoRepository : ECRepository<IngredientInfo>, IIngredientInfoRepository
    {
        private readonly DataContext _context;
        public IngredientInfoRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> CheckExists(int id)
        {
            return await _context.Ingredients.AnyAsync(x => x.ID == id);
        }
        public async Task<bool> CheckBarCodeExists(string code)
        {
            return await _context.IngredientsInfos.AnyAsync(x => x.Code.Equals(code));
        }

    }
}