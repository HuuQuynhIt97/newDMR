using AutoMapper;
using AutoMapper.QueryableExtensions;
using DMR_API._Repositories.Interface;
using DMR_API._Services.Interface;
using DMR_API.Constants;
using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Services
{
    public class BuildingUserService : IBuildingUserService
    {
        private readonly IBuildingUserRepository _buildingUserRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public BuildingUserService(IBuildingUserRepository buildingUserRepository,
            IBuildingRepository buildingRepository,
            IUserRoleRepository userRoleRepository,
            IMapper mapper,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _buildingUserRepository = buildingUserRepository;
            _buildingRepository = buildingRepository;
            _userRoleRepository = userRoleRepository;
        }

        public Task<bool> Add(BuildingUserDto model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<BuildingUserDto>> GetAllAsync()
        {
            return await _buildingUserRepository.FindAll().ProjectTo<BuildingUserDto>(_configMapper).ToListAsync();

        }

        //public async Task<object> GetBuildingByUserID(int userid)
        //{
        //    var model = await _buildingUserRepository.FindAll().FirstOrDefaultAsync(x => x.UserID == userid);
        //    if (model == null) return new Building();
        //    return _buildingRepository.FindById(model.BuildingID);
        //}

        public async Task<List<BuildingUserDto>> GetBuildingUserByBuildingID(int buildingID)
        {
            return await _buildingUserRepository.FindAll().Where(x => x.BuildingID == buildingID).ProjectTo<BuildingUserDto>(_configMapper).ToListAsync();
        }

        public BuildingUserDto GetById(object id)
        {
            throw new NotImplementedException();
        }

        public Task<PagedList<BuildingUserDto>> GetWithPaginations(PaginationParams param)
        {
            throw new NotImplementedException();
        }

        public async Task<object> MapBuildingUser(int userid, int buildingid)
        {
            var buildingLevel = 2;
            var item = await _buildingUserRepository.FindAll().Include(x => x.Building).Where(x => x.Building.Level == buildingLevel && x.UserID == userid).ToListAsync();
            if (item.Count == 0)
            {
                try
                {
                    _buildingUserRepository.Add(new BuildingUser
                    {
                        UserID = userid,
                        BuildingID = buildingid,
                        CreatedDate = DateTime.Now
                    });

                    await _buildingUserRepository.SaveAll();
                    return new
                    {
                        status = true,
                        message = "Mapping Successfully!"
                    };
                }
                catch (Exception)
                {
                    return new
                    {
                        status = false,
                        message = "Failed on save!"
                    };
                }
            }
            else
            {

                try
                {
                    _buildingUserRepository.RemoveMultiple(item);
                    await _buildingUserRepository.SaveAll();
                    _buildingUserRepository.Add(new BuildingUser
                    {
                        UserID = userid,
                        BuildingID = buildingid,
                        CreatedDate = DateTime.Now
                    });
                    return new
                    {
                        status = await _buildingUserRepository.SaveAll(),
                        message = "Mapping Successfully!"
                    };
                }
                catch (Exception)
                {
                    return new
                    {
                        status = false,
                        message = "Failed on save!"
                    };
                }
            }
        }


        public async Task<object> MappingUserWithBuilding(BuildingUserDto buildingUserDto)
        {
            var item = await _buildingUserRepository.FindAll().FirstOrDefaultAsync(x => x.UserID == buildingUserDto.UserID && x.BuildingID == buildingUserDto.BuildingID);
            if (item == null)
            {
                _buildingUserRepository.Add(new BuildingUser
                {
                    UserID = buildingUserDto.UserID,
                    BuildingID = buildingUserDto.BuildingID
                });
                try
                {
                    await _buildingUserRepository.SaveAll();
                    return new
                    {
                        status = true,
                        message = "Mapping Successfully!"
                    };
                }
                catch (Exception)
                {
                    return new
                    {
                        status = false,
                        message = "Failed on save!"
                    };
                }
            }
            else
            {

                return new
                {
                    status = false,
                    message = "The User belonged with other building!"
                };
            }
        }


        public async Task<object> RemoveBuildingUser(BuildingUserDto buildingUserDto)
        {
            var item = await _buildingUserRepository.FindAll().FirstOrDefaultAsync(x => x.UserID == buildingUserDto.UserID && x.BuildingID == buildingUserDto.BuildingID);
            if (item != null)
            {
                _buildingUserRepository.Remove(item);
                try
                {
                    await _buildingUserRepository.SaveAll();
                    return new
                    {
                        status = true,
                        message = "Delete Successfully!"
                    };
                }
                catch (Exception)
                {
                    return new
                    {
                        status = false,
                        message = "Failed on delete!"
                    };
                }
            }
            else
            {

                return new
                {
                    status = false,
                    message = ""
                };
            }
        }


        public Task<PagedList<BuildingUserDto>> Search(PaginationParams param, object text)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(BuildingUserDto model)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDetail<object>> RemoveLineUser(BuildingUserForRemoveDto dto)
        {
            var lineLevel = 3;
            var item = await _buildingUserRepository.FindAll()
                .Include(x => x.Building)
                .Where(x => x.Building.Level == lineLevel
                && x.UserID == dto.UserID
                && dto.Buildings.Contains(x.BuildingID)).ToListAsync();
            if (item.Count != 0)
            {
                _buildingUserRepository.RemoveMultiple(item);
                await _buildingUserRepository.SaveAll();
            }
            try
            {
                //var list = new List<BuildingUser>();
                //foreach (var buildingid in dto.Buildings)
                //{
                //    list.Add(new BuildingUser
                //    {
                //        UserID = dto.UserID,
                //        BuildingID = buildingid,
                //        CreatedDate = DateTime.Now
                //    });
                //}
                //_buildingUserRepository.AddRange(list);

                await _buildingUserRepository.SaveAll();
                return new ResponseDetail<object>
                {
                    Status = true,
                    Message = "Mapping Successfully!"
                };
            }
            catch (Exception)
            {
                return new ResponseDetail<object>
                {
                    Status = false,
                    Message = "Failed on save!"
                };
            }
        }
        public async Task<ResponseDetail<object>> MapLineUser(BuildingUserForMapDto dto)
        {
            var lineLevel = 3;
            var item = await _buildingUserRepository.FindAll()
               .Include(x => x.Building)
               .Where(x => x.Building.Level == lineLevel
               && x.UserID == dto.UserID
               && dto.Buildings.Contains(x.BuildingID)).ToListAsync();
            if (item.Count != 0)
            {
                _buildingUserRepository.RemoveMultiple(item);
                await _buildingUserRepository.SaveAll();
            }
            try
            {
                var list = new List<BuildingUser>();
                foreach (var buildingid in dto.Buildings)
                {
                    list.Add(new BuildingUser
                    {
                        UserID = dto.UserID,
                        BuildingID = buildingid,
                        CreatedDate = DateTime.Now
                    });
                }
                _buildingUserRepository.AddRange(list);

                await _buildingUserRepository.SaveAll();
                return new ResponseDetail<object>
                {
                    Status = true,
                    Message = "Mapping Successfully!"
                };
            }
            catch (Exception)
            {
                return new ResponseDetail<object>
                {
                    Status = false,
                    Message = "Failed on save!"
                };
            }

        }

        public async Task<ResponseDetail<List<BuildingDto>>> GetLineByUserID(int userid, int buildingid)
        {
            var buildings = await _buildingUserRepository.FindAll().Include(x => x.Building)
                .Where(x => x.Building.Level == 2 && x.UserID == userid).Select(x => x.Building.ID).ToListAsync();
            if (buildings.Count == 0)
            {
                return new ResponseDetail<List<BuildingDto>>
                {
                    Data = new List<BuildingDto>(),
                    Status = false,
                    Message = "Vui lòng cài đặt tòa nhà cho tài khoản này trước tiên!"
                };
            }
            var model = from a in _buildingRepository.FindAll(x => buildings.Contains(x.ParentID.Value))
                        join b in _buildingUserRepository.FindAll(x => x.UserID == userid) on a.ID equals b.BuildingID into ab
                        from c in ab.DefaultIfEmpty()
                        select new BuildingDto
                        {
                            ID = a.ID,
                            Level = a.Level,
                            Status = c == null ? false : true,
                            Name = a.Name
                        };

            return new ResponseDetail<List<BuildingDto>>
            {
                Data = await model.ToListAsync(),
                Status = true
            };
        }

        public async Task<ResponseDetail<object>> RemoveMultipleBuildingUser(BuildingUserForRemoveDto dto)
        {
            var buildingLevel = 2;
            var item = await _buildingUserRepository.FindAll()
             .Include(x => x.Building)
             .Where(x => x.Building.Level == buildingLevel
             && x.UserID == dto.UserID
             && dto.Buildings.Contains(x.BuildingID)).ToListAsync();
            if (item != null)
            {
                _buildingUserRepository.RemoveMultiple(item);
                try
                {
                    await _buildingUserRepository.SaveAll();
                    return new ResponseDetail<object>()
                    {
                        Status = true,
                        Message = "Delete Successfully!"
                    };
                }
                catch (Exception)
                {
                    return new ResponseDetail<object>()
                    {
                        Status = false,
                        Message = "Failed on delete!"
                    };
                }
            }
            else
            {

                return new ResponseDetail<object>()
                {
                    Status = false,
                    Message = ""
                };
            }
        }
        public async Task<ResponseDetail<object>> MapMultipleBuildingUser(BuildingUserForMapDto dto)
        {
            var buildingLevel = 2;
            var item = await _buildingUserRepository.FindAll()
                .Include(x => x.Building)
                .Where(x => x.Building.Level == buildingLevel
                && x.UserID == dto.UserID
                && dto.Buildings.Contains(x.BuildingID)).ToListAsync();
            if (item.Count != 0)
            {
                _buildingUserRepository.RemoveMultiple(item);
                await _buildingUserRepository.SaveAll();
            }
            try
            {
                var list = new List<BuildingUser>();
                foreach (var buildingid in dto.Buildings)
                {
                    list.Add(new BuildingUser
                    {
                        UserID = dto.UserID,
                        BuildingID = buildingid,
                        CreatedDate = DateTime.Now
                    });
                }
                _buildingUserRepository.AddRange(list);

                await _buildingUserRepository.SaveAll();
                return new ResponseDetail<object>
                {
                    Status = true,
                    Message = "Mapping Successfully!"
                };
            }
            catch (Exception)
            {
                return new ResponseDetail<object>
                {
                    Status = false,
                    Message = "Failed on save!"
                };
            }

        }

        public async Task<ResponseDetail<List<BuildingDto>>> GetBuildingByUserID(int userid)
        {
            var buildingLevel = 2;

            var model = from a in _buildingRepository.FindAll(x => x.Level == buildingLevel)
                        join b in _buildingUserRepository.FindAll(x => x.UserID == userid) on a.ID equals b.BuildingID into ab
                        from c in ab.DefaultIfEmpty()
                        select new BuildingDto
                        {
                            ID = a.ID,
                            Level = a.Level,
                            Status = c == null ? false : true,
                            Name = a.Name
                        };

            return new ResponseDetail<List<BuildingDto>>
            {
                Data = await model.ToListAsync(),
                Status = true
            };
        }

        public async Task<ResponseDetail<List<BuildingDto>>> GetBuildingUserByUserID(int userid)
        {
            var buildingLevel = 2;
            var role = await _userRoleRepository.FindAll(x => x.UserID == userid).FirstOrDefaultAsync();

            // Nếu gán nhóm quyền rồi nhưng chưa gán tòa nhà thì load hết tòa nhà
            var model = await (from b in _buildingRepository.FindAll(x => x.Level == buildingLevel)
                               join l in _buildingRepository.FindAll() on b.ID equals l.ParentID
                               join bu in _buildingUserRepository.FindAll(x => x.UserID == userid) on b.ID equals bu.BuildingID
                               select b
                                 ).Distinct().ProjectTo<BuildingDto>(_configMapper).ToListAsync();
            if (model.Count == 0 ) {
                var allBuildings = await _buildingRepository.FindAll(x => x.Level == buildingLevel)
                               .ProjectTo<BuildingDto>(_configMapper).ToListAsync();
                return new ResponseDetail<List<BuildingDto>>
                {
                    Data = allBuildings,
                    Status = true
                };
            }
            return new ResponseDetail<List<BuildingDto>>
            {
                Data = model,
                Status = true
            };
            
            // // if (role.RoleID == (int)Enums.Role.Worker || role.RoleID == (int)Enums.Role.Dispatcher)
            // // {

            // //     var model = await (from b in _buildingRepository.FindAll(x => x.Level == buildingLevel)
            // //                        join l in _buildingRepository.FindAll() on b.ID equals l.ParentID
            // //                        join bu in _buildingUserRepository.FindAll(x => x.UserID == userid) on b.ID equals bu.BuildingID
            // //                        select b
            // //                        ).Distinct().ProjectTo<BuildingDto>(_configMapper).ToListAsync();
            // //     return new ResponseDetail<List<BuildingDto>>
            // //     {
            // //         Data = model,
            // //         Status = true
            // //     };
            // // }
            // // if (role.RoleID == (int)Enums.Role.Admin || role.ID == (int)Enums.Role.Supervisor || role.ID == (int)Enums.Role.Staff)
            // // {
            // //     var model = await _buildingRepository.FindAll(x => x.Level == buildingLevel)
            // //                     .ProjectTo<BuildingDto>(_configMapper).ToListAsync();
            // //     return new ResponseDetail<List<BuildingDto>>
            // //     {
            // //         Data = model,
            // //         Status = true
            // //     };
            // // }

            // return new ResponseDetail<List<BuildingDto>>
            // {
            //     Data = null,
            //     Status = false
            // };
        }
    }
}
