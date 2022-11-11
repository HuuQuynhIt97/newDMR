using DMR_API._Repositories.Interface;
using DMR_API.Data;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Repositories.Repositories
{
    public class MixingInfoDetailRepository : ECRepository<MixingInfoDetail>, IMixingInfoDetailRepository
    {
        private readonly DataContext _context;

        public MixingInfoDetailRepository(DataContext context) : base(context)
        {
            _context = context;
        }
    }
}
