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
using DMR_API._Repositories;
using dmr_api.Models;
using System.Transactions;

namespace DMR_API._Services.Services
{
    public class DispatchService : IDispatchService
    {
        private readonly IDispatchRepository _repoDispatch;
        private readonly IToDoListService _toDoListService;
        private readonly IPlanService _planService;
        private readonly IMixingInfoRepository _repoMixingInfo;
        private readonly IStationRepository _repoStation;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public DispatchService(
            IDispatchRepository repoDispatch,
            IToDoListService toDoListService,
            IPlanService planService,
            IMixingInfoRepository repoMixingInfo,
            IStationRepository repoStation,
            IMapper mapper,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoDispatch = repoDispatch;
            _toDoListService = toDoListService;
            _planService = planService;
            _repoMixingInfo = repoMixingInfo;
            _repoStation = repoStation;
        }

        public async Task<bool> Add(Dispatch model)
        {
            model.CreatedTime = DateTime.Now.ToLocalTime();
            _repoDispatch.Add(model);
            return await _repoDispatch.SaveAll();
        }

        public bool AddDispatching(Dispatch model)
        {
            try
            {
                var mixing = _repoMixingInfo.FindById(model.MixingInfoID);
                if (mixing == null) return false;
                using (TransactionScope scope = new TransactionScope())
                {
                    _repoDispatch.Add(model);
                    _repoDispatch.Save();

                    var item = new ToDoListForUpdateDto()
                    {
                        GlueName = mixing.GlueName,
                        EstimatedFinishTime = mixing.EstimatedFinishTime,
                        EstimatedStartTime = mixing.EstimatedStartTime,
                    };
                    _toDoListService.UpdateStiringTimeRange(item);
                    scope.Complete();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            throw new NotImplementedException();
        }
        public async Task<bool> Update(Dispatch model)
        {
            var dispatch = _mapper.Map<Dispatch>(model);
            _repoDispatch.Update(dispatch);
            return await _repoDispatch.SaveAll();
        }
        public async Task<bool> Delete(object id)
        {
            var dispatch = _repoDispatch.FindById(id);
            _repoDispatch.Remove(dispatch);
            return await _repoDispatch.SaveAll();
        }

        public async Task<List<Dispatch>> GetAllAsync()
        {
            return await _repoDispatch.FindAll().OrderBy(x => x.ID).ToListAsync();
        }

        public Task<PagedList<Dispatch>> GetWithPaginations(PaginationParams param)
        {
            throw new NotImplementedException();
        }

        public Task<PagedList<Dispatch>> Search(PaginationParams param, object text)
        {
            throw new NotImplementedException();
        }

        public Dispatch GetById(object id)
        {
            throw new NotImplementedException();
        }

        public object AddDispatchingRange(List<Dispatch> dispatch)
        {

            if (dispatch.Count == 0) return new
            {
                status = false,
                message = ""
            };
            if (dispatch.All(x => x.ID == 0) == true) return new
            {
                status = false,
                message = ""
            };

            foreach (var item in dispatch)
            {
                // xoa bo
                var station = _repoStation.FindById(1);
                if (station != null && station.Amount == 0)
                {
                    var mixingModel = _repoMixingInfo.FindAll(x => x.ID == item.MixingInfoID).Include(x => x.Glue).ThenInclude(x => x.GlueName).FirstOrDefault();
                    var glueName = mixingModel.Glue.GlueName.Name;
                    var plan = _planService.FindByID(station.PlanID);
                    return new
                    {
                        status = false,
                        message = $"Vui lòng thêm station của tên keo '{glueName}' cho BPFC '{plan.BPFCName}' ! Please add station"
                    };
                }
                if (station == null)
                {
                    var plan = _planService.FindByID(station.PlanID);

                    return new
                    {
                        status = false,
                        message = $"Vui lòng thêm station cho BPFC là ({plan.BPFCName}) ! Please add station"
                    };
                }
            }
            var dispatchList = _repoDispatch.FindAll(x => dispatch.Select(a => a.ID).Contains(x.ID)).ToList();
            var firstDispatch = dispatchList.OrderBy(x => x.ID).FirstOrDefault();
            var lastDispatch = dispatchList.OrderBy(x => x.ID).LastOrDefault();
            var mixing = _repoMixingInfo.FindById(dispatch.FirstOrDefault().MixingInfoID);
            if (mixing == null) return new
            {
                status = false,
                message = "Vui lòng trộn keo! Please do mixing process!"
            };

            var flags = new List<bool>();

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    _repoDispatch.Update(lastDispatch);
                    _repoDispatch.Save();
                    var item = new ToDoListForUpdateDto()
                    {
                        GlueName = mixing.GlueName,
                        EstimatedFinishTime = mixing.EstimatedFinishTime,
                        EstimatedStartTime = mixing.EstimatedStartTime,
                        MixingInfoID = firstDispatch.MixingInfoID,
                        Amount = firstDispatch.Amount,
                        LineID = firstDispatch.LineID,
                        BuildingID = mixing.BuildingID,
                        Dispatches = dispatchList
                    };
                    _toDoListService.UpdateDispatchTimeRange(item);
                    scope.Complete();
                    flags.Add(true);
                }
                catch
                {
                    scope.Dispose();
                    flags.Add(false);
                }
            }

            return new
            {
                status = flags.All(x => x == true),
                message = flags.All(x => x == true) is true ? "" : "Vui lòng thử lại!"
            };
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateAmount(int id, double amount)
        {
            var item = _repoDispatch.FindById(id);
            if (item is null) return false;
            item.Amount = amount;
            item.DeliveryTime = DateTime.Now.ToLocalTime();
            //item.StartDispatchingTime = item.StartDispatchingTime is null ? DateTime.Now.ToLocalTime() : item.StartDispatchingTime;
            _repoDispatch.Update(item);
            return await _repoDispatch.SaveAll();
        }

        public bool UpdateStartDispatchingTime(int id)
        {
            var item = _repoDispatch.FindById(id);
            if (item is null) return false;
            try
            {
                _repoDispatch.Update(item);
                _repoDispatch.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}