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
    public class StirRawDataService : IStirRawDataService
    {

        private readonly IStirRawDataRepository _repoStirRawData;
        private readonly IPlanRepository _repoPlan;
        private readonly IBuildingRepository _repoBuilding;
        private readonly IIngredientRepository _repoIngredient;
        private readonly IMixingInfoRepository _repoMixingInfo;
        private readonly IIngredientInfoRepository _repoIngredientInfo;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public StirRawDataService(
            IStirRawDataRepository repoStirRawData,
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
            _repoStirRawData = repoStirRawData;
            _repoIngredientInfo = repoIngredientInfo;
            _repoPlan = repoPlan;
            _repoIngredient = repoIngredient;
            _repoBuilding = repoBuilding;
            _repoMixingInfo = repoMixingInfo;
        }

        public async Task<bool> Add(StirRawData model)
        {
            var StirRawData = _mapper.Map<StirRawData>(model);

            _repoStirRawData.Add(StirRawData);
            return await _repoStirRawData.SaveAll();
        }

     
        public async Task<bool> Delete(object id)
        {
            var StirRawData = _repoStirRawData.FindById(id);
            _repoStirRawData.Remove(StirRawData);
            return await _repoStirRawData.SaveAll();
        }

        public async Task<bool> Update(StirRawData model)
        {
            var StirRawData = _mapper.Map<StirRawData>(model);
            _repoStirRawData.Update(StirRawData);
            return await _repoStirRawData.SaveAll();
        }

        public async Task<List<StirRawData>> GetAllAsync() => await _repoStirRawData.FindAll().OrderByDescending(x => x.ID).ToListAsync();

        public StirRawData GetById(object id) => _repoStirRawData.FindById(id);

        public Task<PagedList<StirRawData>> GetWithPaginations(PaginationParams param)
        {
            throw new NotImplementedException();
        }

        public Task<PagedList<StirRawData>> Search(PaginationParams param, object text)
        {
            throw new NotImplementedException();
        }
    }
}