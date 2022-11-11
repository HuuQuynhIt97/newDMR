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
    public class BuildingService : IBuildingService
    {

        private readonly IBuildingRepository _repoBuilding;
        private readonly IBuildingTypeRepository _repoBuildingType;
        private readonly IPeriodRepository _repoPeriod;
        private readonly IJWTService _jwtService;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IBuildingUserRepository _buildingUserRepository;
        private readonly ILunchTimeRepository _repoLunchTime;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        private readonly int ROOT_LEVEL = 1;
        public BuildingService(
            IBuildingRepository repoBuilding,
            IBuildingTypeRepository repoBuildingType,
            IPeriodRepository repoPeriod,
            IJWTService jwtService,
            IUserRoleRepository userRoleRepository,
            IBuildingUserRepository buildingUserRepository,
            ILunchTimeRepository repoLunchTime,
            IMapper mapper, MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoBuilding = repoBuilding;
            _repoBuildingType = repoBuildingType;
            _repoPeriod = repoPeriod;
            _jwtService = jwtService;
            _userRoleRepository = userRoleRepository;
            _buildingUserRepository = buildingUserRepository;
            _repoLunchTime = repoLunchTime;
        }

        public async Task<bool> Add(BuildingDto model)
        {
            var building = _mapper.Map<Building>(model);
            _repoBuilding.Add(building);
            return await _repoBuilding.SaveAll();
        }

        public async Task<PagedList<BuildingDto>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoBuilding.FindAll().ProjectTo<BuildingDto>(_configMapper).OrderByDescending(x => x.ID);
            return await PagedList<BuildingDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        public async Task<PagedList<BuildingDto>> Search(PaginationParams param, object text)
        {
            var lists = _repoBuilding.FindAll().ProjectTo<BuildingDto>(_configMapper)
            .Where(x => x.Name.Contains(text.ToString()))
            .OrderByDescending(x => x.ID);
            return await PagedList<BuildingDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        public async Task<bool> Delete(object id)
        {
            var Building = _repoBuilding.FindById(id);
            var data = _repoBuilding.FindAll().ToList().AsHierarchy(x => x.ID, y => y.ParentID, id).ToList();
            var da = data.Flatten2(x => x.ChildNodes).ToList();
            var list = new List<Building>();
            foreach (var item in da)
            {
                list.Add(item.Entity);
            }
            _repoBuilding.RemoveMultiple(list);
            //_repoBuilding.Remove(Building);
            return await _repoBuilding.SaveAll();
        }

        public async Task<bool> Update(BuildingDto model)
        {
            try
            {
                var building = _mapper.Map<Building>(model);
                _repoBuilding.Update(building);

                return await _repoBuilding.SaveAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<List<BuildingDto>> GetAllAsync()
        {
            return await _repoBuilding.FindAll()
                .Include(x => x.LunchTime)
                .Include(x => x.Kind)
                .ProjectTo<BuildingDto>(_configMapper)
                .OrderByDescending(x => x.ID).ToListAsync();
        }

        public BuildingDto GetById(object id)
        {
            return _mapper.Map<Building, BuildingDto>(_repoBuilding.FindById(id));
        }

        public async Task<IEnumerable<HierarchyNode<BuildingTreeDto>>> GetAllAsTreeView()
        {
            var data = await _repoBuilding.FindAll()
                .Include(x => x.LunchTime)
                .Include(x => x.Kind)
                .ProjectTo<BuildingTreeDto>(_configMapper)
                .OrderByDescending(x => x.ID).ToListAsync();
            var lists = data.OrderBy(x => x.Name).AsHierarchy(x => x.ID, y => y.ParentID);
            return lists;
        }

        public async Task<List<BuildingDto>> GetBuildings()
        {
            var userid = _jwtService.GetUserID();
            var role = await _userRoleRepository.FindAll(x => x.UserID == userid).AsNoTracking().FirstOrDefaultAsync();
           var model = await _buildingUserRepository.FindAll(x => x.UserID == userid)
                        .Include(x => x.Building).ThenInclude(x => x.Kind).Select(x => new BuildingDto
                        {
                            ID = x.Building.ID,
                            Level = x.Building.Level,
                            ParentID = x.Building.ParentID,
                            Name = x.Building.Name,
                            IsSTF = x.Building.Kind == null ? false : true,
                        }).ToListAsync();
            if (model.Count == 0) {
                return await _repoBuilding.FindAll().Include(x => x.LunchTime).Include(x => x.Kind).Where(x => x.Level != 5).ProjectTo<BuildingDto>(_configMapper).OrderBy(x => x.Level).ToListAsync();
            }
            return model;
            // switch (role.RoleID)
            // {
            //     case (int)Enums.Role.Admin:
            //     case (int)Enums.Role.Supervisor:
            //     case (int)Enums.Role.Staff:
            //         return await _repoBuilding.FindAll().Include(x => x.LunchTime).Include(x => x.Kind).Where(x => x.Level != 5).ProjectTo<BuildingDto>(_configMapper).OrderBy(x => x.Level).ToListAsync();
            //     case (int)Enums.Role.Worker:
            //     case (int)Enums.Role.Dispatcher:
            //         return await _buildingUserRepository.FindAll(x => x.UserID == userid)
            //             .Include(x => x.Building).ThenInclude(x => x.Kind).Select(x => new BuildingDto
            //             {
            //                 ID = x.Building.ID,
            //                 Level = x.Building.Level,
            //                 ParentID = x.Building.ParentID,
            //                 Name = x.Building.Name,
            //                 IsSTF = x.Building.Kind == null ? false : true,
            //             }).ToListAsync();
            //     default:
            //         return new List<BuildingDto>();
            // }

        }

        public async Task<object> CreateMainBuilding(BuildingDto buildingDto)
        {
            if (buildingDto.ID == 0)
            {
                var item = _mapper.Map<Building>(buildingDto);
                item.Level = 1;
                //item.ParentID = null;
                _repoBuilding.Add(item);
                try
                {
                    return new { status = await _repoBuilding.SaveAll(), building = item };
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new { status = false };
                }

            }
            else
            {
                var item = _repoBuilding.FindById(buildingDto.ID);
                item.Name = buildingDto.Name;
                _repoBuilding.Update(item);
                try
                {
                    return new { status = await _repoBuilding.SaveAll(), building = item };
                }
                catch (Exception)
                {
                    return new { status = false };
                }

            }


        }

        public async Task<object> CreateSubBuilding(BuildingDto buildingDto)
        {
            var item = _mapper.Map<Building>(buildingDto);

            //Level cha tang len 1 va gan parentid cho subtask
            var itemParent = _repoBuilding.FindById(buildingDto.ParentID);
            item.Level = itemParent.Level + 1;
            item.ParentID = buildingDto.ParentID;
            _repoBuilding.Add(item);

            try
            {
                return new { status = await _repoBuilding.SaveAll(), building = item };
            }
            catch (Exception)
            {
                return new { status = false };
            }
        }

        public async Task<object> GetBuildingsForSetting() => await _repoBuilding.FindAll().Where(x => x.Level == 2).Select(x => new { x.ID, x.Name }).OrderBy(x => x.Name).ToListAsync();

        public Task<bool> AddOrUpdateLunchTime(LunchTimeDto lunchTimeDto)
        {
            throw new NotImplementedException();


        }

        public async Task<object> GetAllBuildingType()
        => await _repoBuildingType.FindAll().ToListAsync();

        public async Task<bool> CheckRoot()
         => await _repoBuilding.FindAll(x => ROOT_LEVEL).AnyAsync();
    }
}