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
    public class SubpackageCapacityService : ISubpackageCapacityService
    {
        private readonly ISubpackageCapacityRepository _repoSubpackageCapacity;
        private readonly IJWTService _jwtService;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public SubpackageCapacityService(
            ISubpackageCapacityRepository repoSubpackageCapacity,
            IJWTService jwtService,
            IMapper mapper,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoSubpackageCapacity = repoSubpackageCapacity;
            _jwtService = jwtService;
        }

        //Thêm SubpackageCapacity mới vào bảng Line
        public async Task<bool> Add(SubpackageCapacity model)
        {
            var Line = _mapper.Map<SubpackageCapacity>(model);
            _repoSubpackageCapacity.Add(Line);
            return await _repoSubpackageCapacity.SaveAll();
        }

     

        //Lấy danh sách SubpackageCapacity và phân trang
        public async Task<PagedList<SubpackageCapacity>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoSubpackageCapacity.FindAll().OrderByDescending(x => x.ID);
            return await PagedList<SubpackageCapacity>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
      
        //Tìm kiếm Line
        public async Task<PagedList<SubpackageCapacity>> Search(PaginationParams param, object text)
        {
            var lists = _repoSubpackageCapacity.FindAll()
            .OrderByDescending(x => x.ID);
            return await PagedList<SubpackageCapacity>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        //Xóa SubpackageCapacity
        public async Task<bool> Delete(object id)
        {
            var Line = _repoSubpackageCapacity.FindById(id);
            _repoSubpackageCapacity.Remove(Line);
            return await _repoSubpackageCapacity.SaveAll();
        }

        //Cập nhật SubpackageCapacity
        public async Task<bool> Update(SubpackageCapacity model)
        {
            model.UpdatedBy = _jwtService.GetUserID();
            model.UpdatedTime = DateTime.Now;
            _repoSubpackageCapacity.Update(model);
            return await _repoSubpackageCapacity.SaveAll();
        }
      
        //Lấy toàn bộ danh sách SubpackageCapacity 
        public async Task<List<SubpackageCapacity>> GetAllAsync()
        {
            return await _repoSubpackageCapacity.FindAll().OrderByDescending(x => x.ID).ToListAsync();
        }

        //Lấy SubpackageCapacity theo SubpackageCapacity_Id
        public SubpackageCapacity GetById(object id) => _repoSubpackageCapacity.FindById(id);
    }
}