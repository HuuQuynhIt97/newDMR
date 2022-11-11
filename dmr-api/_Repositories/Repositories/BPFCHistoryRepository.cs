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
    public class BPFCHistoryRepository : ECRepository<BPFCHistory>, IBPFCHistoryRepository
    {
        private readonly DataContext _context;
        public BPFCHistoryRepository(DataContext context) : base(context) => _context = context;

        public async Task<bool> CheckGlueID(int code)
        {
            return await _context.BPFCHistories.AnyAsync(x => x.GlueID.Equals(code));
        }



        //Login khi them repo
    }
}