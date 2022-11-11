using AutoMapper;
using DMR_API._Repositories.Interface;
using DMR_API.Data;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Repositories.Repositories
{
    public class BuildingUserRepository : ECRepository<BuildingUser>, IBuildingUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public BuildingUserRepository(DataContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

    }
}
