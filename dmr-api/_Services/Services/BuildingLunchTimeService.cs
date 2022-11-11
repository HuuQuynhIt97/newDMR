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
    public class BuildingLunchTimeService : IBuildingLunchTimeService
    {
        private readonly IPeriodRepository _repoPeriod;
        private readonly ILunchTimeRepository _repoLunchTime;
        private readonly IPeriodMixingRepository _repoPeriodMixing;
        private readonly IPeriodDispatchRepository _repoPeriodDispatch;
        private readonly IBuildingRepository _repoBuilding;
        private readonly IJWTService _jWTService;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public BuildingLunchTimeService(IPeriodRepository repoPeriod,
            ILunchTimeRepository repoLunchTime,
            IPeriodMixingRepository repoPeriodMixing,
            IPeriodDispatchRepository repoPeriodDispatch,
            IBuildingRepository repoBuilding,
            IJWTService jWTService,
            IMapper mapper,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoPeriod = repoPeriod;
            _repoLunchTime = repoLunchTime;
            _repoPeriodMixing = repoPeriodMixing;
            _repoPeriodDispatch = repoPeriodDispatch;
            _repoBuilding = repoBuilding;
            _jWTService = jWTService;
        }

        public async Task<bool> Add(Period model)
        {
            var Line = _mapper.Map<Period>(model);
            _repoPeriod.Add(Line);
            return await _repoPeriod.SaveAll();
        }
        public async Task<bool> AddRangePeriod(Period model)
        {
            var Line = _mapper.Map<Period>(model);
            _repoPeriod.Add(Line);
            return await _repoPeriod.SaveAll();
        }

        //Cập nhật Period
        public async Task<ResponseDetail<object>> UpdatePeriodMixing(PeriodMixing model)
        {
            var userID = _jWTService.GetUserID();
            var period = model;
            var periodItem = await _repoPeriodMixing.FindAll(x => x.ID == model.ID 
                                         && x.StartTime.TimeOfDay == model.StartTime.TimeOfDay
                                         && x.EndTime.TimeOfDay == model.EndTime.TimeOfDay
                                ).AsNoTracking()
                .FirstOrDefaultAsync();
            if (periodItem == null)
            {
                var periodMixing = await _repoPeriodMixing.FindAll(x => x.BuildingID == model.BuildingID && x.IsDelete == false)
                    .AsNoTracking()
                  .ToListAsync();
                periodMixing = periodMixing.Where(x => x.ID != model.ID).ToList();
                var building = await _repoBuilding.FindAll(x => x.ID == model.BuildingID)
                    .Include(x => x.LunchTime)
                    .FirstOrDefaultAsync();

                if (periodMixing.Count > 0)
                {
                    foreach (var item in periodMixing)
                    {
                        //(StartA < EndB) && (EndA > StartB) 
                        if (
                                 period.StartTime.TimeOfDay < item.EndTime.TimeOfDay
                                && period.EndTime.TimeOfDay > item.StartTime.TimeOfDay
                           )
                        {
                            return new ResponseDetail<object>()
                            {
                                Status = false,
                                Message = $"Khoảng thời gian này đã giao với 1 thời gian khác trong hệ thống!" +
                                $"This time period overlaps another time period in the system!"
                            };
                        }
                    }
                }
                var startLunchTime = building.LunchTime.StartTime.TimeOfDay;
                var endLunchTime = building.LunchTime.EndTime.TimeOfDay;
                // lunchTime 12:30-13:30
                if (period.StartTime.TimeOfDay >= startLunchTime && period.EndTime.TimeOfDay <= endLunchTime)
                {
                    return new ResponseDetail<object>() { Status = false, Message = "Thời gian bắt đầu và thời gian kết thúc không được giao với giờ ăn trưa!" };
                }
                if (period.StartTime.TimeOfDay > period.EndTime.TimeOfDay)
                {
                    return new ResponseDetail<object>() { Status = false, Message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc!" };
                }
                if (period.StartTime.TimeOfDay == period.EndTime.TimeOfDay)
                {
                    return new ResponseDetail<object>() { Status = false, Message = "Thời gian bắt đầu phải khác thời gian kết thúc!" };
                }
            }
        
            period.UpdatedBy = userID;
            period.UpdatedTime = DateTime.Now;
            try
            {
                _repoPeriodMixing.Update(period);
                var status = await _repoPeriodMixing.SaveAll();
                return new ResponseDetail<object>() { Status = status };
            }
            catch (Exception ex)
            {
                return new ResponseDetail<object>() { Status = false, Message = ex.Message };
            }
        }
        public async Task<ResponseDetail<object>> AddPeriodMixing(PeriodMixing model)
        {
            var userID = _jWTService.GetUserID();
            var period = model;
            var periodMixing = await _repoPeriodMixing.FindAll(x => x.BuildingID == model.BuildingID && x.IsDelete == false)
                .ToListAsync();

            var building = await _repoBuilding.FindAll(x => x.ID == model.BuildingID)
                .Include(x => x.LunchTime)
                .FirstOrDefaultAsync();

            if (periodMixing.Count > 0)
            {
                foreach (var item in periodMixing)
                {
                    if (
                             period.StartTime.TimeOfDay < item.EndTime.TimeOfDay
                            && period.EndTime.TimeOfDay > item.StartTime.TimeOfDay
                       )
                    {
                        return new ResponseDetail<object>()
                        {
                            Status = false,
                            Message = $"Khoảng thời gian này đã giao với 1 thời gian khác trong hệ thống!" +
                            $"This time period overlaps another time period in the system!"
                        };
                    }
                }
            }
            var startLunchTime = building.LunchTime.StartTime.TimeOfDay;
            var endLunchTime = building.LunchTime.EndTime.TimeOfDay;
            // lunchTime 12:30-13:30
            // 12:30 >= 12:30 and 13:00 <= 13:30
            if (period.StartTime.TimeOfDay >= startLunchTime && period.EndTime.TimeOfDay <= endLunchTime)
            {
                return new ResponseDetail<object>() { Status = false, Message = "Thời gian bắt đầu và thời gian kết thúc không được giao với giờ ăn trưa!" };
            }
            if (period.StartTime.TimeOfDay > period.EndTime.TimeOfDay)
            {
                return new ResponseDetail<object>() { Status = false, Message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc!" };
            }
            if (period.StartTime.TimeOfDay == period.EndTime.TimeOfDay)
            {
                return new ResponseDetail<object>() { Status = false, Message = "Thời gian bắt đầu phải khác thời gian kết thúc!" };
            }
            period.CreatedBy = userID;
            period.CreatedTime = DateTime.Now;
            try
            {
                _repoPeriodMixing.Add(period);
                var status = await _repoPeriodMixing.SaveAll();
                return new ResponseDetail<object>() { Status = status };
            }
            catch (Exception ex)
            {
                return new ResponseDetail<object>() { Status = false, Message = ex.Message };
            }
        }
        public async Task<ResponseDetail<object>> DeletePeriodMixing(int id)
        {
            var userID = _jWTService.GetUserID();
            var periodMixing = await _repoPeriodMixing.FindAll(x => x.ID == id).FirstOrDefaultAsync();

            periodMixing.DeletedBy = userID;
            periodMixing.IsDelete = true;
            periodMixing.DeletedTime = DateTime.Now;
            try
            {
                _repoPeriodMixing.Update(periodMixing);
                var status = await _repoPeriodMixing.SaveAll();
                return new ResponseDetail<object>() { Status = status };
            }
            catch (Exception ex)
            {
                return new ResponseDetail<object>() { Status = false, Message = ex.Message };
            }

        }
        public async Task<List<PeriodMixing>> GetPeriodMixingByBuildingID(int buildingID)
       => await _repoPeriodMixing.FindAll(x => x.BuildingID == buildingID && x.IsDelete == false).OrderBy(x=> x.StartTime).ToListAsync();


        //Cập nhật Dispatch period
        public async Task<ResponseDetail<object>> UpdatePeriodDispatch(PeriodDispatch model)
        {
            var userID = _jWTService.GetUserID();
            var period = model;
            var periodDispatch = await _repoPeriodDispatch.FindAll(x => x.PeriodMixingID == model.PeriodMixingID && x.IsDelete == false)
                    .AsNoTracking()
                  .ToListAsync();
            periodDispatch = periodDispatch.Where(x => x.ID != model.ID).ToList();
            var periodMixing = await _repoPeriodMixing.FindAll(x => x.ID == model.PeriodMixingID)
                .FirstOrDefaultAsync();

            //if (periodDispatch.Count > 0)
            //{
            //    foreach (var item in periodDispatch)
            //    {
            //        if (
            //                 period.StartTime.TimeOfDay < item.EndTime.TimeOfDay
            //                && period.EndTime.TimeOfDay > item.StartTime.TimeOfDay
            //           )
            //        {
            //            return new ResponseDetail<object>()
            //            {
            //                Status = false,
            //                Message = $"Khoảng thời gian này đã giao với 1 thời gian khác trong hệ thống!" +
            //                $"This time period overlaps another time period in the system!"
            //            };
            //        }
            //    }
            //}
            var startPeriodMixing = periodMixing.StartTime.TimeOfDay;
            var endPeriodMixing = periodMixing.EndTime.TimeOfDay;
            // lunchTime 12:30-13:30
            // 12:30 >= 12:30 and 13:00 <= 13:30
            //if (period.StartTime.TimeOfDay >= startPeriodMixing && period.EndTime.TimeOfDay <= endPeriodMixing)
            //{
            //    return new ResponseDetail<object>() { Status = false, Message = "Thời gian bắt đầu và thời gian kết thúc không được giao với khoảng thời gian của pha keo!" };
            //}
            if (period.StartTime.TimeOfDay > period.EndTime.TimeOfDay)
            {
                return new ResponseDetail<object>() { Status = false, Message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc!" };
            }
            if (period.StartTime.TimeOfDay == period.EndTime.TimeOfDay)
            {
                return new ResponseDetail<object>() { Status = false, Message = "Thời gian bắt đầu phải khác thời gian kết thúc!" };
            }
            period.UpdatedBy = userID;
            period.UpdatedTime = DateTime.Now;
            try
            {
                _repoPeriodDispatch.Update(period);
                var status = await _repoPeriodDispatch.SaveAll();
                return new ResponseDetail<object>() { Status = status };
            }
            catch (Exception ex)
            {
                return new ResponseDetail<object>() { Status = false, Message = ex.Message };
            }
        }
        public async Task<ResponseDetail<object>> AddPeriodDispatch(PeriodDispatch model)
        {
            var userID = _jWTService.GetUserID();
            var period = model;
            var periodDispatch = await _repoPeriodDispatch.FindAll(x => x.PeriodMixingID == model.PeriodMixingID && x.IsDelete == false)
                .ToListAsync();

            var periodMixing = await _repoPeriodMixing.FindAll(x => x.ID == model.PeriodMixingID)
                .FirstOrDefaultAsync();

            //if (periodDispatch.Count > 0)
            //{
            //    foreach (var item in periodDispatch)
            //    {
            //        if (
            //                  period.StartTime.TimeOfDay < item.EndTime.TimeOfDay
            //                && period.EndTime.TimeOfDay > item.StartTime.TimeOfDay
            //           )
            //        {
            //            return new ResponseDetail<object>()
            //            {
            //                Status = false,
            //                Message = $"Khoảng thời gian này đã giao với 1 thời gian khác trong hệ thống!" +
            //                $"This time period overlaps another time period in the system!"
            //            };
            //        }
            //    }
            //}
            var startPeriodMixing = periodMixing.StartTime.TimeOfDay;
            var endPeriodMixing = periodMixing.EndTime.TimeOfDay;
           
            //if ( // 8 > 9 & 9:30 > 9 || 
            //        period.StartTime.TimeOfDay < endPeriodMixing 
            //    && period.EndTime.TimeOfDay > endPeriodMixing
            //    ||
            //     period.StartTime.TimeOfDay < startPeriodMixing
            //    && period.EndTime.TimeOfDay > startPeriodMixing
            //    )
            //{
            //    return new ResponseDetail<object>() { Status = false, Message = "Thời gian bắt đầu và thời gian kết thúc không được giao với khoảng thời gian của pha keo!" };
            //}
            if (period.StartTime.TimeOfDay > period.EndTime.TimeOfDay)
            {
                return new ResponseDetail<object>() { Status = false, Message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc!" };
            }
            if (period.StartTime.TimeOfDay == period.EndTime.TimeOfDay)
            {
                return new ResponseDetail<object>() { Status = false, Message = "Thời gian bắt đầu phải khác thời gian kết thúc!" };
            }
            period.CreatedBy = userID;
            period.CreatedTime = DateTime.Now;
            try
            {
                _repoPeriodDispatch.Add(period);
                var status = await _repoPeriodDispatch.SaveAll();
                return new ResponseDetail<object>() { Status = status };
            }
            catch (Exception ex)
            {
                return new ResponseDetail<object>() { Status = false, Message = ex.Message };
            }
        }
        public async Task<ResponseDetail<object>> DeletePeriodDispatch(int id)
        {
            var userID = _jWTService.GetUserID();
            var periodDispatch = await _repoPeriodDispatch.FindAll(x => x.ID == id).FirstOrDefaultAsync();

            periodDispatch.DeletedBy = userID;
            periodDispatch.IsDelete = true;
            periodDispatch.DeletedTime = DateTime.Now;
            try
            {
                _repoPeriodDispatch.Update(periodDispatch);
                var status = await _repoPeriodDispatch.SaveAll();
                return new ResponseDetail<object>() { Status = status };
            }
            catch (Exception ex)
            {
                return new ResponseDetail<object>() { Status = false, Message = ex.Message };
            }

        }
        public async Task<List<PeriodDispatch>> GetPeriodDispatchByPeriodMixingID(int periodMixingID)
       => await _repoPeriodDispatch.FindAll(x => x.PeriodMixingID == periodMixingID && x.IsDelete == false).OrderBy(x => x.StartTime).ToListAsync();

        public async Task<ResponseDetail<object>> AddLunchTimeBuilding(Building building)
        {
            var item = await _repoBuilding.FindAll(x => x.ID == building.ID).FirstOrDefaultAsync();
            try
            {
                item.LunchTimeID = building.LunchTimeID;
                _repoBuilding.Update(item);
                var status = await _repoBuilding.SaveAll();
                return new ResponseDetail<object>() { Status = status };
            }
            catch (Exception ex)
            {
                return new ResponseDetail<object>() { Status = false, Message = ex.Message };
            }
        }
    }
}