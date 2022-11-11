using DMR_API._Repositories.Interface;
using DMR_API.Data;
using AutoMapper;
using DMR_API.Data.MongoModels;

namespace DMR_API._Repositories.Repositories
{
    public class RawDataRepository : IoTRepository<RawData>, IRawDataRepository
    {
        private readonly IoTContext _context;
        private readonly IMapper _mapper;

        public RawDataRepository(IoTContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }
    }
}