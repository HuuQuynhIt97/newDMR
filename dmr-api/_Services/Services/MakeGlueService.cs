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
    public class MakeGlueService : IMakeGlueService
    {
        private readonly IGlueIngredientRepository _repoGlueIngredient;
        private readonly IMakeGlueRepository _repoMakeGlue;
        private readonly IGlueRepository _repoGlue;
        private readonly IBuildingRepository _repoBuilding;
        private readonly IBPFCEstablishRepository _repoBPFC;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public MakeGlueService(
            IGlueIngredientRepository repoGlueIngredient,
            IGlueRepository repoGlue, 
            IBPFCEstablishRepository repoBPFC,
            IMakeGlueRepository repoMakeGlue,
            IMapper mapper,
            IBuildingRepository repoBuilding,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoGlue = repoGlue;
            _repoBPFC = repoBPFC;
            _repoMakeGlue = repoMakeGlue;
            _repoGlueIngredient = repoGlueIngredient;
            _repoBuilding = repoBuilding;

        }

        public async Task<object> GetAllGlues()
        {
            return await _repoGlue.FindAll().Where(x=>x.isShow == true).ProjectTo<GlueCreateDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        public async Task<object> GetGlueWithIngredientByGlueCode(string code)
        {
            return await _repoMakeGlue.GetGlueWithIngredientByGlueCode(code);
        }
        public async Task<object> GetGlueWithIngredientByGlueName(string glueName)
        {
            return await _repoMakeGlue.GetGlueWithIngredientByGlueName(glueName);
        }
       
        public async Task<object> GetGlueWithIngredientByGlueID(int glueID)
        {
            return await _repoMakeGlue.GetGlueWithIngredientByGlueID(glueID);
        }
        public async Task<object> GetGlueWithIngredients(int glueid)
        {
            return await _repoGlueIngredient.GetIngredientOfGlue(glueid);
            throw new NotImplementedException();
        }

        public async Task<object> MakeGlue(string code)
        {
            return await _repoMakeGlue.MakeGlue(code);
        }

        public object DeliveredHistory()
        {
            throw new NotImplementedException();
        }
    }
}