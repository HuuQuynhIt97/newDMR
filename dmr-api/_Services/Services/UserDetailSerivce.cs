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
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace DMR_API._Services.Services
{
    public class UserDetailService : IUserDetailService
    {
        private readonly IUserDetailRepository _repoUserDetail;
        private readonly IBuildingUserRepository _repoBuildingUser;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IUserRoleRepository _repoUserRole;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public UserDetailService(IUserDetailRepository repoBrand,
            IBuildingUserRepository repoBuildingUser,
            IBuildingRepository buildingRepository,
            IUserRoleRepository repoUserRole,
            IConfiguration configuration,
             IMapper mapper,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _configuration = configuration;
            _repoUserDetail = repoBrand;
            _repoBuildingUser = repoBuildingUser;
            _buildingRepository = buildingRepository;
            _repoUserRole = repoUserRole;
        }

        //Thêm Brand mới vào bảng UserDetail
        public async Task<bool> Add(UserDetailDto model)
        {
            var UserDetail = _mapper.Map<UserDetail>(model);
            _repoUserDetail.Add(UserDetail);
            return await _repoUserDetail.SaveAll();
        }



        //Lấy danh sách Brand và phân trang
        public async Task<PagedList<UserDetailDto>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoUserDetail.FindAll().ProjectTo<UserDetailDto>(_configMapper).OrderByDescending(x => x.ID);
            return await PagedList<UserDetailDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        //public async Task<object> GetIngredientOfUserDetail(int UserDetailid)
        //{
        //    return await _repoUserDetail.GetIngredientOfUserDetail(UserDetailid);

        //    throw new System.NotImplementedException();
        //}
        //Tìm kiếm UserDetail
        //public async Task<PagedList<UserDetailDto>> Search(PaginationParams param, object text)
        //{
        //    var lists = _repoUserDetail.FindAll().ProjectTo<UserDetailDto>(_configMapper)
        //    .Where(x => x.Name.Contains(text.ToString()))
        //    .OrderByDescending(x => x.ID);
        //    return await PagedList<UserDetailDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        //}
        //Xóa Brand
        public async Task<bool> Delete(object id)
        {
            var UserDetail = _repoUserDetail.FindById(id);
            _repoUserDetail.Remove(UserDetail);
            return await _repoUserDetail.SaveAll();
        }

        //Cập nhật Brand
        public async Task<bool> Update(UserDetailDto model)
        {
            var UserDetail = _mapper.Map<UserDetail>(model);
            _repoUserDetail.Update(UserDetail);
            return await _repoUserDetail.SaveAll();
        }

        //Lấy toàn bộ danh sách Brand 
        public async Task<List<UserDetailDto>> GetAllAsync()
        {
            return await _repoUserDetail.FindAll().ProjectTo<UserDetailDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        //Lấy Brand theo Brand_Id
        public UserDetailDto GetById(object id)
        {
            return _mapper.Map<UserDetail, UserDetailDto>(_repoUserDetail.FindById(id));
        }

        public Task<PagedList<UserDetailDto>> Search(PaginationParams param, object text)
        {
            throw new NotImplementedException();
        }

        public Task<List<ModelNoForMapModelDto>> GetModelNos(int modelNameID)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MapUserDetailDto(UserDetailDto mapModel)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(int userId, int lineID)
        {
            throw new NotImplementedException();
        }

        public async Task<object> GetAllUserInfo()
        {
            var appsettings = _configuration.GetSection("AppSettings").Get<Appsettings>();
            var DMRSystemCode = appsettings.SystemCode;
            using var client = new HttpClient();
            var response = await client.GetAsync($"{appsettings.API_AUTH_URL}Users/GetUserBySystemID/{DMRSystemCode}");
            var data = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<UserDto>>(data);
            var userRole = await _repoUserRole.FindAll().Include(x => x.Role).ToListAsync();
            var buildingUser = await _repoBuildingUser.FindAll()
                .Include(x => x.Building)
                .Where(x=> x.Building.Level == 2).ToListAsync();
            var lines = await _repoBuildingUser.FindAll()
              .Include(x => x.Building)
              .Where(x => x.Building.Level == 3).ToListAsync();
            var result = new List<UserDto>();
            foreach (var x in users)
            {
                var userRoleItem = userRole.FirstOrDefault(a => a.UserID == x.ID);
                var buildingUserItem = buildingUser.FirstOrDefault(a => a.UserID == x.ID);
                var line = lines.Where(a => a.UserID == x.ID).Select(a => new { a.Building.Name , a.Building.ID}).ToList();
                var lineTemp = line.Count == 0 ? "#N/A" : string.Join(" , ", line.Select(x => x.Name));
                var building = buildingUser.Where(a=> a.UserID == x.ID).Select(a => new { a.Building.Name, a.Building.ID }).ToList();
                var buildingTemp = building.Count == 0 ? "#N/A" : string.Join(" , ", building.Select(x=> x.Name));
                result.Add(new UserDto
                {
                    ID = x.ID,
                    Username = x.Username,
                    Password = x.Password,
                    EmployeeID = x.EmployeeID,
                    Email = x.Email,
                    PasswordSalt = x.PasswordSalt,
                    PasswordHash = x.PasswordHash,
                    IsLock = userRoleItem != null ? userRoleItem.IsLock : false,
                    SystemID = DMRSystemCode,
                    UserRoleID = userRoleItem != null ? userRoleItem.RoleID : 0,
                    RoleCode = userRoleItem != null ? userRoleItem.Role.Code : "#N/A",
                    BuildingUserID = buildingUserItem != null ? buildingUserItem.BuildingID : 0,
                    Role = userRoleItem != null ? userRoleItem.Role.Name : "#N/A",
                    Building = buildingTemp,
                    Line = lineTemp,
                    Buildings = building.Select(a => a.ID).ToList(),
                    Lines = line.Select(a => a.ID).ToList()
                });
            }

            return result.Where(x => x.RoleCode != "SUPPER_ADMIN").ToList();
        }

        public async Task<object> GetAllUserInfoRoles()
        {
            var appsettings = _configuration.GetSection("AppSettings").Get<Appsettings>();
            var DMRSystemCode = appsettings.SystemCode;
            using var client = new HttpClient();
            var response = await client.GetAsync($"{appsettings.API_AUTH_URL}Users/GetUserBySystemID/{DMRSystemCode}");
            var data = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<UserDto>>(data);
            var userRole = await _repoUserRole.FindAll().Include(x => x.Role)
                .Where(x => x.RoleID == (int)Enums.Role.Worker
            || x.RoleID == (int)Enums.Role.Dispatcher)
                .ToListAsync();
            users = users.Where(x => userRole.Select(a => a.UserID).Contains(x.ID)).ToList();
            var buildingUser = await _repoBuildingUser.FindAll()
                .Include(x => x.Building)
                .Where(x => x.Building.Level == 2).ToListAsync();
            var lines = await _repoBuildingUser.FindAll()
              .Include(x => x.Building)
              .Where(x => x.Building.Level == 3).ToListAsync();

           
            var linesUser = await _repoBuildingUser.FindAll().Include(x => x.Building)
               .Where(x => x.Building.Level == 3).Select(x => x.Building.ParentID.Value).ToListAsync();

           

            var result = new List<UserDto>();
            foreach (var x in users)
            {
                var userRoleItem = userRole.FirstOrDefault(a => a.UserID == x.ID);
                var buildings = await _repoBuildingUser.FindAll().Include(x => x.Building)
              .Where(_ => _.Building.Level == 2 && _.UserID == x.ID).Select(x => x.Building.ID).ToListAsync();

                var buildingsModel =await (from a in _buildingRepository.FindAll(_ => _.Level == 2)
                                           join b in _repoBuildingUser.FindAll(_ => _.UserID == x.ID) on a.ID equals b.BuildingID into ab
                                           from c in ab.DefaultIfEmpty()
                                           select new BuildingDto
                                           {
                                               ID = a.ID,
                                               Level = a.Level,
                                               Status = c == null ? false : true,
                                               Name = a.Name
                                           }).ToListAsync();
                var linesModel =await (from a in _buildingRepository.FindAll(_ => buildings.Contains(_.ParentID.Value))
                                       join b in _repoBuildingUser.FindAll(_ => _.UserID == x.ID) on a.ID equals b.BuildingID into ab
                                       from c in ab.DefaultIfEmpty()
                                       select new BuildingDto
                                       {
                                           ID = a.ID,
                                           Level = a.Level,
                                           Status = c == null ? false : true,
                                           Name = a.Name
                                       }).ToListAsync();


                var line = lines.Where(a => a.UserID == x.ID).Select(a => new { a.Building.Name, a.Building.ID }).ToList();
                var lineTemp = line.Count == 0 ? "#N/A" : string.Join(" , ", line.Select(x => x.Name));
              
                var building = buildingUser.Where(a => a.UserID == x.ID).Select(a => new { a.Building.Name, a.Building.ID }).ToList();
                var buildingTemp = building.Count == 0 ? "#N/A" : string.Join(" , ", building.Select(x => x.Name));
                var item = new BuildingDto
                {
                    ID = -1,
                    Level = 0,
                    Status = false,
                    Name = "#N/A"
                };
                var lineIDList = line.Select(a => a.ID).ToList();
                var buildingIDList = building.Select(a => a.ID).ToList();
                var emptyItem = new List<int> { -1 };
                result.Add(new UserDto
                {
                    ID = x.ID,
                    Username = x.Username,
                    Password = x.Password,
                    EmployeeID = x.EmployeeID,
                    Email = x.Email,
                    PasswordSalt = x.PasswordSalt,
                    PasswordHash = x.PasswordHash,
                    IsLock = userRoleItem != null ? userRoleItem.IsLock : false,
                    SystemID = DMRSystemCode,
                    RoleID = x.RoleID,
                    UserRoleID = userRoleItem != null ? userRoleItem.RoleID : 0,
                    Role = userRoleItem != null ? userRoleItem.Role.Name : "#N/A",
                    Building = buildingTemp,
                    Line = lineTemp,
                    RoleCode = userRoleItem != null ? userRoleItem.Role.Code : "#N/A",
                    BuildingsData = buildingsModel.Prepend(item).OrderBy(x=>x.Level).ToList(),
                    LinesData = linesModel.Prepend(item).OrderBy(x => x.Level).ToList(),
                    Buildings = buildingIDList.Count == 0 ? emptyItem : buildingIDList,
                    Lines = lineIDList.Count == 0 ? emptyItem : lineIDList
                });
            }
            return result.Where(x => x.RoleCode != "SUPPER_ADMIN").ToList();
        }
    }
}