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
    public class GlueRepository : ECRepository<Glue>, IGlueRepository
    {
        private readonly DataContext _context;
        public GlueRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> CheckBarCodeExists(string code)
        {
            return await _context.Glues.AnyAsync(x => x.Code.Equals(code));

        }

        public async Task<bool> CheckExists(int id)
        {
            return await _context.Glues.AnyAsync(x => x.ID == id);
        }

        //Login khi them repo
    }
}