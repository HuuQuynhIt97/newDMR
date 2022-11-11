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
using Version = DMR_API.Models.Version;

namespace DMR_API._Services.Services
{
    public class VersionService : IVersionService
    {

        private readonly IVersionRepository _repoVersion;
        private readonly IPlanRepository _repoPlan;
        private readonly IBuildingRepository _repoBuilding;
        private readonly IIngredientRepository _repoIngredient;
        private readonly IMixingInfoRepository _repoMixingInfo;
        private readonly IIngredientInfoRepository _repoIngredientInfo;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public VersionService(
            IVersionRepository repoVersion,
            IMapper mapper,
            IPlanRepository repoPlan,
             IIngredientRepository repoIngredient,
            IIngredientInfoRepository repoIngredientInfo,
            IBuildingRepository repoBuilding,
            IMixingInfoRepository repoMixingInfo,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoVersion = repoVersion;
            _repoIngredientInfo = repoIngredientInfo;
            _repoPlan = repoPlan;
            _repoIngredient = repoIngredient;
            _repoBuilding = repoBuilding;
            _repoMixingInfo = repoMixingInfo;
        }

        public async Task<bool> Add(Version model)
        {
            model.CreatedTime  = DateTime.Now;
            _repoVersion.Add(model);
            return await _repoVersion.SaveAll();
        }

        public async Task<PagedList<Version>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoVersion.FindAll().OrderByDescending(x => x.ID);
            return await PagedList<Version>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        public async Task<PagedList<Version>> Search(PaginationParams param, object text)
        {
            var lists = _repoVersion.FindAll()
            .Where(x => x.Name.Contains(text.ToString()))
            .OrderByDescending(x => x.ID);
            return await PagedList<Version>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        public async Task<bool> Delete(object id)
        {
            var Version = _repoVersion.FindById(id);
            _repoVersion.Remove(Version);
            return await _repoVersion.SaveAll();
        }

        public async Task<bool> Update(Version model)
        {
            model.UpatedTime = DateTime.Now;
            _repoVersion.Update(model);
            return await _repoVersion.SaveAll();
        }

        public async Task<List<Version>> GetAllAsync() => await _repoVersion.FindAll().OrderByDescending(x => x.ID).ToListAsync();

        public Version GetById(object id) => _repoVersion.FindById(id);

    }
}