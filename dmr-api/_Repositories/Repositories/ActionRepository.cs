using System.Threading.Tasks;
using DMR_API._Repositories.Interface;
using DMR_API.Data;
using DMR_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using DMR_API.DTO;
using System.Collections.Generic;
using AutoMapper;

namespace DMR_API._Repositories.Repositories
{
    public class ActionRepository : ECRepository<Action>, IActionRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public ActionRepository(DataContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }
    }
}