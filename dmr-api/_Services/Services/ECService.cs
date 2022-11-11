using AutoMapper;
using AutoMapper.QueryableExtensions;
using DMR_API._Services.Interface;
using DMR_API.Data;
using DMR_API.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Services
{
    public class ECService<T, TDto> : IServiceBase<T, TDto> where T : class
    {
        private readonly IMapper _mapper;
        private readonly IECRepository<T> _repoEC;
        private readonly MapperConfiguration _configMapper;

        public ECService(
            IMapper mapper,
            IECRepository<T> repoEC,
            MapperConfiguration configMapper
            )
        {
            _mapper = mapper;
            _repoEC = repoEC;
            _configMapper = configMapper;
        }

        public async Task<bool> Add(TDto model)
        {
            var item = _mapper.Map<T>(model);
            _repoEC.Add(item);
            return await _repoEC.SaveAll();
        }
        public async Task<bool> AddRange(List<TDto> model)
        {
            var item = _mapper.Map<List<T>>(model);
            _repoEC.AddRange(item);
            return await _repoEC.SaveAll();
        }

        public async Task<bool> Delete(object id)
        {
            var item = _repoEC.FindById(id);
            _repoEC.Remove(item);
            return await _repoEC.SaveAll();
        }

        public async Task<List<TDto>> GetAllAsync()
        {
            return await _repoEC.FindAll().ProjectTo<TDto>(_configMapper).ToListAsync();

        }

        public TDto GetById(object id)
        {
            return _mapper.Map<T, TDto>(_repoEC.FindById(id));
        }

        public async Task<PagedList<TDto>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoEC.FindAll().ProjectTo<TDto>(_configMapper).OrderByDescending(x => x.GetType().GetProperty("ID").GetValue(x));
            return await PagedList<TDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }

        public async Task<PagedList<TDto>> Search(PaginationParams param, object text)
        {
            var lists = _repoEC.FindAll().ProjectTo<TDto>(_configMapper)
          .Where(x => x.GetType().GetProperty("Name").GetValue(x) == text )
          .OrderByDescending(x => x.GetType().GetProperty("ID"));
            return await PagedList<TDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }

        public async Task<bool> Update(TDto model)
        {
            var item = _mapper.Map<T>(model);
            _repoEC.Update(item);
            return await _repoEC.SaveAll();
        }
    }
}
