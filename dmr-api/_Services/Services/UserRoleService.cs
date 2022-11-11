using AutoMapper;
using AutoMapper.QueryableExtensions;
using DMR_API._Repositories.Interface;
using DMR_API._Services.Interface;
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
    public class UserRoleService : IUserRoleService
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public UserRoleService(IUserRoleRepository userRoleRepository,
            IMapper mapper,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _userRoleRepository = userRoleRepository;
        }

        public Task<bool> Add(UserRoleDto model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<UserRoleDto>> GetAllAsync()
        {
            return await _userRoleRepository.FindAll().ProjectTo<UserRoleDto>(_configMapper).ToListAsync();

        }

        public async Task<List<UserRoleDto>> GetUserRoleByUserID(int userID)
        {
            return await _userRoleRepository.FindAll().Where(x => x.UserID == userID).ProjectTo<UserRoleDto>(_configMapper).ToListAsync();
        }

        public UserRoleDto GetById(object id)
        {
            throw new NotImplementedException();
        }

        public Task<PagedList<UserRoleDto>> GetWithPaginations(PaginationParams param)
        {
            throw new NotImplementedException();
        }

        public async Task<object> MapUserRole(int userid, int roleID)
        {
            var item = await _userRoleRepository.FindAll().Where(x => x.UserID == userid).ToListAsync();
            if (item.Count == 0)
            {
                try
                {
                    _userRoleRepository.Add(new UserRole
                    {
                        UserID = userid,
                        RoleID = roleID,
                    });

                    return new
                    {
                        status = await _userRoleRepository.SaveAll(),
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
                    _userRoleRepository.RemoveMultiple(item);
                    await _userRoleRepository.SaveAll();

                    _userRoleRepository.Add(new UserRole
                    {
                        UserID = userid,
                        RoleID = roleID,
                    });

                    
                    return new
                    {
                        status = await _userRoleRepository.SaveAll(),
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

        public async Task<object> MappingUserRole(UserRoleDto userRoleDto)
        {
            var item = await _userRoleRepository.FindAll().FirstOrDefaultAsync(x => x.UserID == userRoleDto.UserID && x.UserID == userRoleDto.UserID);
            if (item == null)
            {
                _userRoleRepository.Add(new UserRole
                {
                    UserID = userRoleDto.UserID,
                    RoleID = userRoleDto.RoleID
                });
                try
                {
                    await _userRoleRepository.SaveAll();
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
                    message = "The User belonged with other user!"
                };
            }
        }

        public async Task<object> RemoveUserRole(UserRoleDto userRoleDto)
        {
            var item = await _userRoleRepository.FindAll().FirstOrDefaultAsync(x => x.UserID == userRoleDto.UserID && x.UserID == userRoleDto.UserID);
            if (item != null)
            {
                _userRoleRepository.Remove(item);
                try
                {
                    await _userRoleRepository.SaveAll();
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

        public Task<PagedList<UserRoleDto>> Search(PaginationParams param, object text)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(UserRoleDto model)
        {
            throw new NotImplementedException();
        }

        public async Task<object> GetRoleByUserID(int userid)
        {
            var model = await _userRoleRepository.FindAll().Include(x => x.Role).FirstOrDefaultAsync(x => x.UserID == userid);

            return model.Role;
        }

        public async Task<bool> Lock(UserRoleDto userRoleDto)
        {
            var model = await _userRoleRepository.FindAll().FirstOrDefaultAsync(x => x.UserID == userRoleDto.UserID && x.RoleID == userRoleDto.RoleID);
            if (model != null)
            {
                model.IsLock = !model.IsLock;
                _userRoleRepository.Update(model);

                return await _userRoleRepository.SaveAll();
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> IsLock(UserRoleDto userRoleDto)
        {
            var model = await _userRoleRepository.FindAll().FirstOrDefaultAsync(x => x.UserID == userRoleDto.UserID && x.RoleID == userRoleDto.RoleID);
            if (model != null)
            {
                return model.IsLock;
            }
            else
            {
                return false;
            }
        }
    }
}
