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
    public class RoleService : IRoleService
    {

        private readonly IRoleRepository _repoRole;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        private readonly string SUPPER_ADMIN = "SUPPER_ADMIN";

        public RoleService(IRoleRepository repoRole, IMapper mapper, MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoRole = repoRole;

        }

        public async Task<bool> Add(RoleDto model)
        {
            var artRole = _mapper.Map<Role>(model);
            _repoRole.Add(artRole);
            return await _repoRole.SaveAll();
        }


        public async Task<bool> Delete(object id)
        {
            var ArtRole = _repoRole.FindById(id);
            _repoRole.Remove(ArtRole);
            return await _repoRole.SaveAll();
        }

        public async Task<bool> Update(RoleDto model)
        {
            var artRole = _mapper.Map<Role>(model);
            _repoRole.Update(artRole);
            return await _repoRole.SaveAll();
        }
        public async Task<List<RoleDto>> GetAllAsync()
        {
            // x => x.Code != "SUPPER_ADMIN"
            return await _repoRole.FindAll(x => x.Code != SUPPER_ADMIN).ProjectTo<RoleDto>(_configMapper).OrderBy(x => x.ID).ToListAsync();
        }


        public Task<PagedList<RoleDto>> GetWithPaginations(PaginationParams param)
        {
            throw new NotImplementedException();
        }

        public Task<PagedList<RoleDto>> Search(PaginationParams param, object text)
        {
            throw new NotImplementedException();
        }

        public RoleDto GetById(object id)
        {
            throw new NotImplementedException();
        }
    }
}