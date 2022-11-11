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
    public class KindService : IKindService
    {
        private readonly IKindRepository _repoLine;
        private readonly IKindTypeRepository _repoKindType;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public KindService(
            IKindRepository repoKind, 
            IKindTypeRepository repoKindType,
            IMapper mapper,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoLine = repoKind;
            _repoKindType = repoKindType;
        }

        //Thêm Kind mới vào bảng Line
        public async Task<bool> Add(KindDto model)
        {
            var Line = _mapper.Map<Kind>(model);
            _repoLine.Add(Line);
            return await _repoLine.SaveAll();
        }

     

        //Lấy danh sách Kind và phân trang
        public async Task<PagedList<KindDto>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoLine.FindAll().ProjectTo<KindDto>(_configMapper).OrderByDescending(x => x.ID);
            return await PagedList<KindDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
      
        //Tìm kiếm Line
        public async Task<PagedList<KindDto>> Search(PaginationParams param, object text)
        {
            var lists = _repoLine.FindAll().ProjectTo<KindDto>(_configMapper)
            .Where(x => x.Name.Contains(text.ToString()))
            .OrderByDescending(x => x.ID);
            return await PagedList<KindDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        //Xóa Kind
        public async Task<bool> Delete(object id)
        {
            var Line = _repoLine.FindById(id);
            _repoLine.Remove(Line);
            return await _repoLine.SaveAll();
        }

        //Cập nhật Kind
        public async Task<bool> Update(KindDto model)
        {
            var Line = _mapper.Map<Kind>(model);
            _repoLine.Update(Line);
            return await _repoLine.SaveAll();
        }
      
        //Lấy toàn bộ danh sách Kind 
        public async Task<List<KindDto>> GetAllAsync()
        {
            return await _repoLine.FindAll().ProjectTo<KindDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        //Lấy Kind theo Kind_Id
        public KindDto GetById(object id)
        {
            return  _mapper.Map<Kind, KindDto>(_repoLine.FindById(id));
        }
        public async Task<object> GetAllKindType()
       => await _repoKindType.FindAll().ToListAsync();
    }
}