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
using DMR_API._Repositories;
using Microsoft.AspNetCore.Http;

namespace DMR_API._Services.Services
{
    public class StationService : IStationService
    {
        private readonly IStationRepository _repoStation;
        private readonly IPlanRepository _repoPlan;
        private readonly IToDoListRepository _repoToDoList;
        private readonly IDispatchRepository _repoDispatch;
        private readonly IHttpContextAccessor _accessor;
        private readonly IJWTService _jwtService;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public StationService(
            IStationRepository repoStation,
            IPlanRepository repoPlan,
            IToDoListRepository repoToDoList,
            IDispatchRepository repoDispatch,
            IHttpContextAccessor accessor,
            IJWTService jwtService,
            IMapper mapper,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoStation = repoStation;
            _repoPlan = repoPlan;
            _repoToDoList = repoToDoList;
            _repoDispatch = repoDispatch;
            _accessor = accessor;
            _jwtService = jwtService;
        }

        public async Task<bool> Add(StationDto model)
        {
            var item = _mapper.Map<Station>(model);

            _repoStation.Add(item);
            return await _repoStation.SaveAll();
        }

        public async Task<bool> AddRange(List<StationDto> model)
        {
            var station = _mapper.Map<List<Station>>(model);
            _repoStation.AddRange(station);
            return await _repoStation.SaveAll();
        }
        public async Task<PagedList<StationDto>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoStation.FindAll().ProjectTo<StationDto>(_configMapper).OrderByDescending(x => x.ID);
            return await PagedList<StationDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }

        public async Task<bool> Delete(object id)
        {
            var station = _repoStation.FindById(id);
            _repoStation.Remove(station);
            return await _repoStation.SaveAll();
        }

        // khong su dung
        public Task<bool> Update(StationDto model)
        {
            //string token = _accessor.HttpContext.Request.Headers["Authorization"];
            //var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            //using var transaction = new TransactionScopeAsync().Create();
            //{
            //    try
            //    {
            //        var item = _mapper.Map<Station>(model);
            //        if (item.Amount > 0 && item.ModifyTime is null || item.Amount > 0 && item.ModifyTime != null)
            //        {
            //            item.ModifyTime = DateTime.Now.ToLocalTime();
            //        }
            //        _repoStation.Update(item);
            //        await _repoStation.SaveAll();
            //        // update station thi xoa het dispatch // tao lai dispatch
            //        var dispatchList = await _repoDispatch.FindAll(x => !x.IsDelete ).ToListAsync();
            //        dispatchList.ForEach(x =>
            //        {
            //            x.IsDelete = true;
            //            x.DeleteBy = userID;
            //            x.DeleteTime = DateTime.Now.ToLocalTime();
            //        });
            //        _repoDispatch.UpdateRange(dispatchList);
            //        await _repoDispatch.SaveAll();
            //        if (dispatchList.Count > 0)
            //        {
            //            var dispatchTemp = dispatchList.First();
            //            var dispatchModel = new List<DispatchTodolistDto>();

            //            int stationAmount = item.Amount;
            //            if (stationAmount == 0)
            //            {
            //                dispatchModel.Add(new DispatchTodolistDto
            //                {
            //                    ID = 0,
            //                    LineID = dispatchTemp.LineID,
            //                    MixingInfoID = dispatchTemp.MixingInfoID,
            //                    CreatedTime = DateTime.Now.ToLocalTime(),
            //                    StationID = item.ID,
            //                    CreateBy = userID,
            //                });
            //            }
            //            else
            //            {
            //                for (int i = 1; i <= stationAmount; i++)
            //                {
            //                    dispatchModel.Add(new DispatchTodolistDto
            //                    {
            //                        ID = 0,
            //                        LineID = dispatchTemp.LineID,
            //                        MixingInfoID = dispatchTemp.MixingInfoID,
            //                        CreatedTime = DateTime.Now.ToLocalTime(),
            //                        StationID = item.ID,
            //                        CreateBy = userID,
            //                    });
            //                }
            //            }

            //            var dispatch = _mapper.Map<List<Dispatch>>(dispatchModel);
            //            _repoDispatch.AddRange(dispatch);
            //            await _repoDispatch.SaveAll();
            //        }

            //        transaction.Complete();
            //        return true;
            //    }
            //    catch 
            //    {
            //        transaction.Dispose();
            //        throw;
            //    }
            //}
            throw new NotImplementedException();

        }

        public async Task<List<StationDto>> GetAllAsync()
        {
            return await _repoStation.FindAll().ProjectTo<StationDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        public StationDto GetById(object id)
        {
            return _mapper.Map<Station, StationDto>(_repoStation.FindById(id));
        }

        public Task<PagedList<StationDto>> Search(PaginationParams param, object text)
        {
            throw new NotImplementedException();
        }

        public async Task<List<StationDto>> GetAllByPlanID(int planID)
        {
            var stationModel = await _repoStation.FindAll(x => x.PlanID == planID).ProjectTo<StationDto>(_configMapper).OrderByDescending(x => x.CreateTime).ToListAsync();

            var plansModel = await _repoPlan.FindAll(x => x.ID == planID)
                                .Include(x => x.BPFCEstablish)
                                    .ThenInclude(x => x.Glues)
                                    .ThenInclude(x => x.GlueName)
                                .SelectMany(x => x.BPFCEstablish.Glues, (plan, glue) => new
                                {
                                    ID = plan.ID,
                                    GlueName = glue.GlueName.Name,
                                    GlueID = glue.ID,
                                    IsShow = glue.isShow
                                }).Where(x => x.IsShow == true).ToListAsync();


            var result = (from plan in plansModel
                          from station in stationModel.Where(x => x.PlanID == plan.ID && x.GlueID == plan.GlueID)
                         .DefaultIfEmpty()
                          select new StationDto
                          {
                              PlanID = plan.ID,
                              GlueID = plan.GlueID,
                              GlueName = plan.GlueName,
                              ID = station == null ? 0 : station.ID,
                              CreateTime = station == null ? DateTime.MinValue : station.CreateTime,
                              IsDelete = station == null ? false : station.IsDelete,
                              Amount = station == null ? 1 : station.Amount,
                          }).ToList();

           return result.Where(x=> !x.IsDelete).ToList();

        }

        public async Task<bool> DeleteStation(int id)
        {
            var item = _repoStation.FindById(id);
            item.IsDelete = true;
            item.DeleteBy = _jwtService.GetUserID();
            item.DeleteTime = DateTime.Now;
            _repoStation.Update(item);
            return await _repoStation.SaveAll();
        }

        public async Task<bool> UpdateRange(List<StationDto> stationDtos)
        {
             var flag = new List<bool>();
            foreach (var station in stationDtos)
            {
                //var item = _mapper.Map<Station>(station);
                var item = _repoStation.FindById(station.ID);
                if (item != null)
                {
                    if (item.Amount > 0 && item.ModifyTime is null || item.Amount > 0 && item.ModifyTime != null)
                    {
                        item.ModifyTime = DateTime.Now.ToLocalTime();
                    }
                    item.Amount = station.Amount;
                    _repoStation.Update(item);
                    flag.Add(await _repoStation.SaveAll());
                }
               
            }
            return flag.All(x => x == true);
        }
    }
}