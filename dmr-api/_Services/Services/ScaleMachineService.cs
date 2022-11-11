using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMR_API.Helpers;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DMR_API._Repositories.Interface;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Models;
using Microsoft.EntityFrameworkCore;

namespace DMR_API._Services.Services
{
    public class ScaleMachineService : IScaleMachineService
    {
        private readonly IKindRepository _repoLine;
        private readonly IScaleMachineRepository _repoScale;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public ScaleMachineService(IScaleMachineRepository repoScale ,IKindRepository repoBrand, IMapper mapper, MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoLine = repoBrand;
            _repoScale = repoScale;

        }

        public Task<bool> Add(ScaleMachineDto model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ScaleMachineDto>> GetAllAsync()
        {
            return await _repoScale.FindAll().ProjectTo<ScaleMachineDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        public ScaleMachineDto GetById(object id)
        {
            throw new NotImplementedException();
        }

        public Task<PagedList<ScaleMachineDto>> GetWithPaginations(PaginationParams param)
        {
            throw new NotImplementedException();
        }

        public Task<PagedList<ScaleMachineDto>> Search(PaginationParams param, object text)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(ScaleMachineDto model)
        {
            throw new NotImplementedException();
        }
    }
}