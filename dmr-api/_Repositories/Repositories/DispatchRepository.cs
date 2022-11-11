using System.Threading.Tasks;
using DMR_API._Repositories.Interface;
using DMR_API.Data;
using DMR_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using DMR_API.DTO;
using System.Collections.Generic;
using dmr_api.Models;
using DMR_API._Repositories;

namespace DMR_API._Repositories.Repositories
{
    public class DispatchRepository : ECRepository<Dispatch>, IDispatchRepository
    {
        private readonly DataContext _context;
        public DispatchRepository(DataContext context) : base(context)
        {
            _context = context;
        }
    }
}