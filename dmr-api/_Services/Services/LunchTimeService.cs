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
    public class LunchTimeService : ILunchTimeService
    {
        private readonly ILunchTimeRepository _repoLine;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public LunchTimeService(ILunchTimeRepository repoBrand, IMapper mapper, MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoLine = repoBrand;

        }

        //Thêm Brand mới vào bảng Line
        public async Task<bool> Add(LunchTimeDto model)
        {
            model.StartTime = model.StartTime.ToLocalTime();
            model.EndTime = model.EndTime.ToLocalTime();
            var Line = _mapper.Map<LunchTime>(model);
            _repoLine.Add(Line);
            return await _repoLine.SaveAll();
        }

     

        //Lấy danh sách Brand và phân trang
        public async Task<PagedList<LunchTimeDto>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoLine.FindAll().ProjectTo<LunchTimeDto>(_configMapper).OrderByDescending(x => x.ID);
            return await PagedList<LunchTimeDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
      
        //Tìm kiếm Line
        public async Task<PagedList<LunchTimeDto>> Search(PaginationParams param, object text)
        {
            var lists = _repoLine.FindAll().ProjectTo<LunchTimeDto>(_configMapper)
            .OrderByDescending(x => x.ID);
            return await PagedList<LunchTimeDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        //Xóa Brand
        public async Task<bool> Delete(object id)
        {
            var Line = _repoLine.FindById(id);
            _repoLine.Remove(Line);
            return await _repoLine.SaveAll();
        }

        //Cập nhật Brand
        public async Task<bool> Update(LunchTimeDto model)
        {
            model.StartTime = model.StartTime.ToLocalTime();
            model.EndTime = model.EndTime.ToLocalTime();
            var Line = _mapper.Map<LunchTime>(model);
            _repoLine.Update(Line);
            return await _repoLine.SaveAll();
        }
      
        //Lấy toàn bộ danh sách Brand 
        public async Task<List<LunchTimeDto>> GetAllAsync()
        {
            return await _repoLine.FindAll().ProjectTo<LunchTimeDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        //Lấy Brand theo Brand_Id
        public LunchTimeDto GetById(object id)
        {
            return  _mapper.Map<LunchTime, LunchTimeDto>(_repoLine.FindById(id));
        }

    }
}