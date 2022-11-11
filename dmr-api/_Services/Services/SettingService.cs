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
using System.Transactions;
using dmr_api.Models;

namespace DMR_API._Services.Services
{
    public class SettingService : ISettingService
    {
        private readonly IMixingRepository _repoMixing;
        private readonly IMixingInfoRepository _repoMixingInfo;
        private readonly ISettingRepository _repoSetting;
        private readonly IScaleMachineRepository _repoScale;
        private readonly IStirRepository _repoStir;
        private readonly IGlueTypeRepository _repoGlueType;
        private readonly IToDoListService _toDoListService;
        private readonly IMapper _mapper;

        public SettingService(IToDoListService toDoListService,
            IScaleMachineRepository repoScale,
            IMixingInfoRepository repoMixingInfo,
            IStirRepository repoStir,
            IGlueTypeRepository repoGlueType,
            IMixingRepository repoMixing,
            IMapper mapper,
            ISettingRepository repoSetting)
        {
            _repoMixing = repoMixing;
            _repoSetting = repoSetting;
            _repoMixingInfo = repoMixingInfo;
            _repoScale = repoScale;
            _repoStir = repoStir;
            _repoGlueType = repoGlueType;
            _mapper = mapper;
            _toDoListService = toDoListService;
        }

        public async Task<object> GetAllAsync()
        {
            return await _repoSetting.FindAll().ToListAsync();

        }
        public async Task<object> GetSettingByBuilding(int buildingID)
        {
            return await _repoSetting.FindAll(x => x.BuildingID == buildingID).Include(x => x.GlueType).ToListAsync();
        }

        public async Task<object> GetMachineByBuilding(int buildingID)
        {
            return await _repoScale.FindAll().Where(x => x.BuildingID == buildingID).ToListAsync();
        }
        public bool Add(StirDTO model)
        {
            try
            {
                var stir = _mapper.Map<Stir>(model);
                stir.RPM = 306;
                stir.TotalMinutes = 3.34;
                stir.Status = true;
                _repoStir.Add(stir);
                _repoStir.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> Update(StirDTO model)
        {
            try
            {
                var item = await _repoStir.FindAll().FirstOrDefaultAsync(x => x.ID == model.ID);
                if (item == null)
                    return false;
                item.RPM = model.RPM;
                item.Status = model.Status;
                item.TotalMinutes = model.TotalMinutes;
                _repoStir.Update(item);
                return await _repoStir.SaveAll();

            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateSetting(SettingDTO model)
        {
            try
            {
                var setting = await _repoSetting.FindAll().FirstOrDefaultAsync(x => x.ID == model.ID);
                setting.GlueTypeID = model.GlueTypeID;
                setting.MachineCode = model.MachineCode;
                _repoSetting.Update(setting);
                return await _repoSetting.SaveAll();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<bool> AddSetting(SettingDTO model)
        {
            try
            {
                var setting = _mapper.Map<Setting>(model);
                _repoSetting.Add(setting);
                return await _repoSetting.SaveAll();

            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddMachine(ScaleMachineDto model)
        {
            try
            {
                var scale = _mapper.Map<ScaleMachine>(model);
                _repoScale.Add(scale);
                return await _repoScale.SaveAll();

            }
            catch (System.Exception)
            {

                throw;
            }
        }


        public async Task<bool> UpdateMachine(ScaleMachineDto model)
        {
            try
            {
                var scale = _mapper.Map<ScaleMachine>(model);
                _repoScale.Update(scale);
                return await _repoScale.SaveAll();
            }
            catch (System.Exception)
            {
                throw;
            }
        }
        public async Task<bool> DeleteMachine(int id)
        {
            try
            {
                var item = await _repoScale.FindAll().FirstOrDefaultAsync(x => x.ID == id);
                if (item == null)
                    return false;
                _repoScale.Remove(item);
                return await _repoScale.SaveAll();

            }
            catch (System.Exception)
            {
                throw;
            }
        }
        public async Task<bool> DeleteSetting(int id)
        {
            try
            {
                var item = await _repoSetting.FindAll().FirstOrDefaultAsync(x => x.ID == id);
                if (item == null)
                    return false;
                _repoSetting.Remove(item);
                return await _repoSetting.SaveAll();

            }
            catch
            {
                return false;
            }
        }

        public async Task<List<GlueType>> GetAllGlueTypeAsync()
        {
            return await _repoGlueType.FindAll().ToListAsync();
        }

        public async Task<Setting> CheckMachine(int buildingID, string MachineCode)
        {
            return await _repoSetting.FindAll(x=> x.BuildingID == buildingID && x.MachineCode == MachineCode).FirstOrDefaultAsync();
        }
    }
}