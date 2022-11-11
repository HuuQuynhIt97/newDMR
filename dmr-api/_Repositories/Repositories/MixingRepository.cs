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
    public class MixingRepository : IoTRepository<Mixing>, IMixingRepository
    {
        private readonly IoTContext _context;

        public MixingRepository(IoTContext context) : base(context)
        {
            _context = context;
        }
    }
}