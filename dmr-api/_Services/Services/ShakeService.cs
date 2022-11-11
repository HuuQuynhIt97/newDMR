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
using dmr_api.Models;

namespace DMR_API._Services.Services
{
    public class ShakeService : IShakeService
    {

        private readonly IMapper _mapper;
        private readonly IMixingInfoRepository _repoMixingInfo;
        private readonly IShakeRepository _repoShake;
        private readonly MapperConfiguration _configMapper;
        public ShakeService(
            IMapper mapper,
           IMixingInfoRepository repoMixingInfo,
           IShakeRepository repoShake,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoMixingInfo = repoMixingInfo;
            _repoShake = repoShake;
        }

        public async Task<bool> Add(Shake model)
        {
            _repoShake.Add(model);
            return await _repoShake.SaveAll();
        }

        public async Task<PagedList<Shake>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoShake.FindAll().OrderByDescending(x => x.ID);
            return await PagedList<Shake>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        public async Task<PagedList<Shake>> Search(PaginationParams param, object text)
        {
            var lists = _repoShake.FindAll()
            .Where(x => x.ChemicalType.Contains(text.ToString()))
            .OrderByDescending(x => x.ID);
            return await PagedList<Shake>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        public async Task<bool> Delete(object id)
        {
            var Shake = _repoShake.FindById(id);
            _repoShake.Remove(Shake);
            return await _repoShake.SaveAll();
        }

        public async Task<bool> Update(Shake model)
        {
            var Shake = _mapper.Map<Shake>(model);
            _repoShake.Update(Shake);
            return await _repoShake.SaveAll();
        }

        public async Task<List<Shake>> GetAllAsync() => await _repoShake.FindAll().OrderByDescending(x => x.ID).ToListAsync();

        public Shake GetById(object id) => _repoShake.FindById(id);

        public async Task<object> AddRange(List<Shake> Shakes)
        {
            _repoShake.AddRange(Shakes);
            return await _repoShake.SaveAll();
        }

        public async Task<ResponseDetail<object>> GenerateShakes(int mixingInfoID)
        {
            if (mixingInfoID == 0)
                return new ResponseDetail<object>(null, false, "Vui lòng trộn keo trước!");

            var item = await _repoMixingInfo.FindAll(x => x.ID == mixingInfoID)
                                             .Include(x => x.MixingInfoDetails)
                                             .ThenInclude(x => x.Ingredient)
                                             .ThenInclude(x => x.GlueType)
                                             .FirstOrDefaultAsync();
            if (item == null)
                return new ResponseDetail<object>(null, false, "Không tìm thấy keo này trong hệ thống!");

            var details = item.MixingInfoDetails;

            if (details == null)
                return new ResponseDetail<object>(null, false, $"Keo {item.GlueName} đã bị lỗi vui lòng liên hệ Lab-Team!");

            var chemicalA = details.FirstOrDefault(x => x.Position == "A");

            if (chemicalA == null)
                return new ResponseDetail<object>(null, false, $"Keo {item.GlueName} đã bị lỗi vui lòng liên hệ Lab-Team!");

            var amountTotal = details.Sum(x => x.Amount);
            var length = Math.Ceiling(amountTotal / 2); // theo cong thuc cua khach hang

            var shakeList = new List<Shake>();
            for (int i = 0; i <= length - 1; i++)
            {
                var chemicalType = chemicalA.Ingredient.GlueType;

                if (chemicalType == null)
                    return new ResponseDetail<object>(null, false, $"Hóa chất {chemicalA.Ingredient.Name} chưa được gán loại keo (glue type)!");

                if (chemicalType.Method != "Shaking")
                    return new ResponseDetail<object>(null, false, $"Hóa chất {chemicalA.Ingredient.Name} chưa được gán phương thức khuấy! Vui lòng cài đặt lại!");

                shakeList.Add(new Shake
                {
                   
                    MixingInfoID = mixingInfoID,
                    ChemicalType = chemicalType.Title,
                    StandardCycle = chemicalType.RPM,
                });
            }
            _repoShake.AddRange(shakeList);
            try
            {
                await _repoShake.SaveAll();
                return new ResponseDetail<object>(shakeList, true, "Thành Công!");
            }
            catch (Exception)
            {
                return new ResponseDetail<object>(null, false, "");
            }

        }

        public async Task<ResponseDetail<object>> GetShakesByMixingInfoID(int mixingInfoID)
        {
            var data = await _repoShake.FindAll(x => x.MixingInfoID == mixingInfoID).ToListAsync();

            if (data.Count == 0)
            {
                var result =  await this.GenerateShakes(mixingInfoID);
                return result;
            }
            return new ResponseDetail<object>(data, true, "");
        }
    }
}