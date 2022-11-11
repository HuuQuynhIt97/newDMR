using AutoMapper;
using DMR_API._Repositories.Interface;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using DMR_API._Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Globalization;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Style;
using System.Drawing;
using DMR_API.Data;
using DMR_API.Enums;
using AutoMapper.QueryableExtensions;
using dmr_api.Models;
using DMR_API.Constants;

namespace DMR_API._Services.Services
{
    public class ToDoListService : IToDoListService
    {
        #region Constructor
        private readonly IToDoListRepository _repoToDoList;
        private readonly IGlueRepository _repoGlue;
        private readonly IMixingInfoRepository _repoMixingInfo;
        private readonly IMixingInfoDetailRepository _repoMixingInfoDetail;
        private readonly IIngredientRepository _repoIngredient;
        private readonly IBuildingRepository _repoBuilding;
        private readonly IBuildingUserRepository _repoBuildingUser;
        private readonly IPlanRepository _repoPlan;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMailExtension _emailService;
        private readonly IMongoRepository<Data.MongoModels.RawData> _repoRawData;
        private readonly IStirRepository _repoStir;
        private readonly IDispatchRepository _repoDispatch;
        private readonly IDispatchListRepository _repoDispatchList;
        private readonly IDispatchListDetailRepository _repoDispatchListDetail;
        private readonly IJWTService _jwtService;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public ToDoListService(
            IToDoListRepository repoToDoList,
            IGlueRepository repoGlue,
            IIngredientRepository repoIngredient,
            IMixingInfoRepository repoMixingInfo,
            IMixingInfoDetailRepository repoMixingInfoDetail,
            IBuildingRepository repoBuilding,
            IBuildingUserRepository repoBuildingUser,
            IPlanRepository repoPlan,
            IUserRoleRepository userRoleRepository,
            IHttpContextAccessor accessor,
            IMailExtension emailService,

            IMongoRepository<DMR_API.Data.MongoModels.RawData> repoRawData,
            IStirRepository repoStir,
            IDispatchRepository repoDispatch,
            IDispatchListRepository repoDispatchList,
            IDispatchListDetailRepository repoDispatchListDetail,
            IJWTService jwtService,
            IMapper mapper,
            MapperConfiguration configMapper
            )
        {
            _mapper = mapper;
            _configMapper = configMapper;
            _repoToDoList = repoToDoList;
            _repoMixingInfoDetail = repoMixingInfoDetail;
            _repoIngredient = repoIngredient;
            _repoGlue = repoGlue;
            _repoBuilding = repoBuilding;
            _repoBuildingUser = repoBuildingUser;
            _repoPlan = repoPlan;
            _userRoleRepository = userRoleRepository;
            _accessor = accessor;
            _emailService = emailService;
            _repoRawData = repoRawData;
            _repoStir = repoStir;
            _repoMixingInfo = repoMixingInfo;
            _repoDispatch = repoDispatch;
            _repoDispatchList = repoDispatchList;
            _repoDispatchListDetail = repoDispatchListDetail;
            _jwtService = jwtService;
        }
        #endregion

        #region LoadData
        // Helper for donelist
        List<ToDoListDto> MapDispatchToToDoListDto(List<DispatchList> model)
        {
            var dispatchlist = new List<ToDoListDto>();

            var groupByDispatchListModel = model.GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime, x.GlueNameID });
            foreach (var todo in groupByDispatchListModel)
            {
                var item = todo.FirstOrDefault();
                var lineList = todo.Select(x => x.LineName).OrderBy(x => x).Distinct().ToList();
                var deliveredAmount = todo.Sum(x => x.DeliveredAmount);

                var itemDispatch = new ToDoListDto();
                itemDispatch.ID = item.ID;
                itemDispatch.JobType = 2;
                itemDispatch.PlanID = item.PlanID;
                itemDispatch.MixingInfoID = item.MixingInfoID;
                itemDispatch.GlueID = item.GlueID;
                itemDispatch.LineID = item.LineID;
                itemDispatch.LineName = item.LineName;
                itemDispatch.GlueName = item.GlueName;
                itemDispatch.GlueNameID = item.GlueNameID;

                itemDispatch.Supplier = item.Supplier;
                itemDispatch.Status = item.FinishDispatchingTime.HasValue == false ? false : item.FinishDispatchingTime.Value.ToRemoveSecond() <= item.EstimatedFinishTime;

                itemDispatch.StartMixingTime = null;
                itemDispatch.FinishMixingTime = null;

                itemDispatch.StartStirTime = null;
                itemDispatch.FinishStirTime = null;

                itemDispatch.StartDispatchingTime = item.StartDispatchingTime;
                itemDispatch.FinishDispatchingTime = item.FinishDispatchingTime;

                itemDispatch.PrintTime = item.PrintTime;

                itemDispatch.MixedConsumption = 0;
                itemDispatch.DeliveredConsumption = deliveredAmount;
                itemDispatch.StandardConsumption = 0;
                itemDispatch.DeliveredAmount = Math.Round(deliveredAmount, 3);

                itemDispatch.DispatchTime = item.CreatedTime;
                itemDispatch.EstimatedStartTime = item.EstimatedStartTime;
                itemDispatch.EstimatedFinishTime = item.EstimatedFinishTime;

                itemDispatch.AbnormalStatus = item.AbnormalStatus;
                itemDispatch.LineNames = lineList;
                itemDispatch.BuildingID = item.BuildingID;
                dispatchlist.Add(itemDispatch);
            }
            var modelTempDispatchList = dispatchlist.OrderBy(x => x.EstimatedStartTime)
                                                     .GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime }).ToList();
            var dispatchListResult = new List<ToDoListDto>();

            foreach (var item in modelTempDispatchList)
            {
                dispatchListResult.AddRange(item.OrderByDescending(x => x.GlueName));
            }
            return dispatchListResult;
        }

        public async Task<ToDoListForReturnDto> Done(int buildingID)
        {
            var response = new ToDoListForReturnDto();
            var userID = _jwtService.GetUserID();
            var role = await _userRoleRepository.FindAll(x => x.UserID == userID).FirstOrDefaultAsync();
            var currentTime = DateTime.Now.ToLocalTime();
            var currentDate = currentTime.Date;
            var value = currentTime.ToString("tt", CultureInfo.InvariantCulture);
            var morning = "AM";
            var start = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 00, 00);
            var model = await _repoToDoList.FindAll(x =>
                  x.IsDelete == false
                  && x.EstimatedStartTime.Date == currentDate
                  && x.EstimatedFinishTime.Date == currentDate
                  && x.GlueName.Contains(" + ")
                  && x.BuildingID == buildingID)
               .ToListAsync();

            var result = MapToTodolistDto(model);
            // filter by middle of the day
            if (value == morning)
                result = result.Where(x => x.EstimatedStartTime.TimeOfDay <= start.TimeOfDay).ToList();

            // dispatch
            // map to dto
            var dispatchListModel = await _repoDispatchList.FindAll(x =>
              x.IsDelete == false
              && x.EstimatedStartTime.Date == currentDate
              && x.EstimatedFinishTime.Date == currentDate
              && x.BuildingID == buildingID)
           .ToListAsync();

            var lines = await _repoBuildingUser.FindAll().Include(x => x.Building).Where(x => x.Building.ParentID == buildingID).Select(x => x.BuildingID).ToListAsync();
            if (lines.Count > 0)
            {
                dispatchListModel = dispatchListModel.Where(x => lines.Contains(x.LineID)).ToList();
            }
            else
            {
                var allLines = await _repoBuilding.FindAll().Where(x => x.ParentID == buildingID).Select(x => x.ID).ToListAsync();
                dispatchListModel = dispatchListModel.Where(x => allLines.Contains(x.LineID)).ToList();
            }
            // map to dto
            var dispatchListResult = MapDispatchToToDoListDto(dispatchListModel);

            // filter by middle of the day
            if (value == morning)
                dispatchListResult = dispatchListResult.Where(x => x.EstimatedStartTime.TimeOfDay <= start.TimeOfDay).ToList();

            // decentralzation
            var EVA_UVTotal = result.Where(x => x.IsEVA_UV).Count();
            var dispatchTotal = dispatchListResult.Count();
            var dispatchListDoneResult = dispatchListResult.Where(x => x.FinishDispatchingTime != null).ToList();
            var delayDispatchTotal = dispatchListResult.Where(x => x.FinishDispatchingTime == null && x.EstimatedFinishTime.TimeOfDay < currentTime.TimeOfDay).Count();
            var todoDispatchTotal = dispatchListResult.Where(x => x.FinishDispatchingTime == null && x.EstimatedStartTime.TimeOfDay <= currentTime.TimeOfDay && x.EstimatedFinishTime.TimeOfDay >= currentTime.TimeOfDay).Count();
            var doneDispatchTotal = dispatchListDoneResult.Count();

            var doneList = result.ToList();

            // tinh tong ca 2 danh sach hoan thanh
            var total = doneList.Count();

            var todoTotal = result.Where(x => x.PrintTime is null && x.EstimatedFinishTime.TimeOfDay >= currentTime.TimeOfDay).Count();
            var delayTotal = result.Where(x => x.PrintTime is null && x.EstimatedFinishTime.TimeOfDay < currentTime.TimeOfDay).Count();
            var doneTotal = result.Where(x => x.PrintTime != null).Count();

            int jobTypeOfTodo = 1, jobTypeOfDispatch = 2;
            var data = doneList.Concat(dispatchListResult).Where(x => x.PrintTime != null && x.JobType == jobTypeOfTodo || x.FinishDispatchingTime != null && x.JobType == jobTypeOfDispatch).ToList();
            var recaculatetotal = todoTotal + delayTotal + doneTotal + todoDispatchTotal + delayDispatchTotal + doneDispatchTotal;
            response.TodoDetail(data, doneTotal + doneDispatchTotal, todoTotal, delayTotal, recaculatetotal);
            response.DispatcherDetail(data, 0, todoDispatchTotal, delayDispatchTotal, dispatchTotal);

            return response;

        }

        // Helper for delay, todo
        List<ToDoListDto> MapToTodolistDto(List<ToDoList> model)
        {
            var groupBy = model.GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime, x.GlueNameID, x.PrintTime });
            var todolist = new List<ToDoListDto>();
            foreach (var todo in groupBy)
            {
                var item = todo.FirstOrDefault();
                var lineList = todo.Select(x => x.LineName).DistinctBy(x => x).OrderBy(x => x).ToList();
                var stdTotal = todo.Select(x => x.StandardConsumption).Sum();
                var stddeliver = todo.Select(x => x.DeliveredConsumption).Sum();

                var itemTodolist = new ToDoListDto();
                itemTodolist.ID = item.ID;
                itemTodolist.JobType = 1;
                itemTodolist.PlanID = item.PlanID;
                itemTodolist.MixingInfoID = item.MixingInfoID;
                itemTodolist.GlueID = item.GlueID;
                itemTodolist.LineID = item.LineID;
                itemTodolist.LineName = item.LineName;
                itemTodolist.GlueName = item.GlueName;
                itemTodolist.GlueNameID = item.GlueNameID;
                itemTodolist.Supplier = item.Supplier;
                itemTodolist.Status = item.Status;

                var glue = _repoGlue.FindById(item.GlueID);
                itemTodolist.KindID = glue != null && glue.KindID != null ? glue.KindID.Value : 0;

                itemTodolist.StartMixingTime = item.StartMixingTime;
                itemTodolist.FinishMixingTime = item.FinishMixingTime;

                itemTodolist.StartStirTime = item.StartStirTime;
                itemTodolist.FinishStirTime = item.FinishStirTime;

                itemTodolist.StartDispatchingTime = item.StartDispatchingTime;
                itemTodolist.FinishDispatchingTime = item.FinishDispatchingTime;

                itemTodolist.PrintTime = item.PrintTime;

                itemTodolist.MixedConsumption = Math.Round(item.MixedConsumption, 2);
                itemTodolist.DeliveredConsumption = Math.Round(stddeliver, 2);
                itemTodolist.StandardConsumption = Math.Round(stdTotal, 2);

                itemTodolist.EstimatedStartTime = item.EstimatedStartTime;
                itemTodolist.EstimatedFinishTime = item.EstimatedFinishTime;

                itemTodolist.AbnormalStatus = item.AbnormalStatus;
                itemTodolist.IsDelete = item.IsDelete;

                itemTodolist.LineNames = lineList;
                itemTodolist.BuildingID = item.BuildingID;
                itemTodolist.IsEVA_UV = item.IsEVA_UV;
                todolist.Add(itemTodolist);
            }

            // GroupBy period and then by glueName
            var modelTemp = todolist.OrderBy(x => x.EstimatedStartTime).GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime }).ToList();
            var result = new List<ToDoListDto>();
            foreach (var item in modelTemp)
            {
                result.AddRange(item.OrderByDescending(x => x.GlueName));
            }

            return result;
        }

        public async Task<ToDoListForReturnDto> Delay(int buildingID)
        {
            var userID = _jwtService.GetUserID();
            var role = await _userRoleRepository.FindAll(x => x.UserID == userID).FirstOrDefaultAsync();
            var currentTime = DateTime.Now.ToLocalTime();
            currentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, 0);
            var currentDate = currentTime.Date;
            var value = currentTime.ToString("tt", CultureInfo.InvariantCulture);
            var morning = "AM";
            var start = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 00, 00);
            var model = await _repoToDoList.FindAll(x =>
                   x.IsDelete == false
                   && x.EstimatedStartTime.Date == currentDate
                   && x.EstimatedFinishTime.Date == currentDate
                   && x.GlueName.Contains(" + ")
                   && x.BuildingID == buildingID)
               .ToListAsync();

            var lines = await _repoBuildingUser.FindAll().Include(x => x.Building).Where(x => x.Building.ParentID == buildingID).Select(x => x.BuildingID).ToListAsync();
            if (lines.Count > 0)
            {
                model = model.Where(x => lines.Contains(x.LineID)).ToList();
            }
            else
            {
                var allLines = await _repoBuilding.FindAll().Where(x => x.ParentID == buildingID).Select(x => x.ID).ToListAsync();
                model = model.Where(x => allLines.Contains(x.LineID)).ToList();
            }
            // map dto
            var result = MapToTodolistDto(model);

            // filter by Middle of the day
            if (value == morning)
                result = result.Where(x => x.EstimatedStartTime.TimeOfDay <= start.TimeOfDay).ToList();

            // caculate

            var total = result.Count();
            var doneTotal = result.Where(x => x.PrintTime != null).Count();
            var todoTotal = result.Where(x => x.PrintTime is null
                                            && x.EstimatedFinishTime.TimeOfDay >= currentTime.TimeOfDay
                                         ).Count();

            var delayTotal = result.Where(x => x.PrintTime is null
                                                && x.EstimatedFinishTime.TimeOfDay < currentTime.TimeOfDay
                                                ).Count();

            // decentralization


            var EVA_UVTotal = result.Where(x => x.IsEVA_UV).Count();
            var response = new ToDoListForReturnDto();
            var data = result.Where(x => x.PrintTime is null && x.EstimatedFinishTime.TimeOfDay < currentTime.TimeOfDay).ToList();
            var dispatchListModel = await _repoDispatchList.FindAll(x =>
                                                            x.IsDelete == false
                                                            && x.EstimatedStartTime.Date == currentDate
                                                            && x.EstimatedFinishTime.Date == currentDate
                                                            && x.BuildingID == buildingID)
                                                             .ToListAsync();
            // map to dto
            var dispatchList = MapToDispatchListDto(dispatchListModel);
            if (value == morning)
                dispatchList = dispatchList.Where(x => x.EstimatedStartTime.TimeOfDay <= start.TimeOfDay).ToList();

            var dispatchTotal = dispatchList.Count();
            var dispatchListResult = dispatchList.Where(x => x.FinishDispatchingTime == null && x.EstimatedFinishTime.TimeOfDay < currentTime.TimeOfDay).ToList();
            var delayDispatchTotal = dispatchListResult.Count();
            var dispatchListDoneTotal = dispatchList.Where(x => x.FinishDispatchingTime != null).Count();

            var todoDispatchTotal = dispatchList.Where(x => x.FinishDispatchingTime == null && x.EstimatedStartTime.TimeOfDay <= currentTime.TimeOfDay && x.EstimatedFinishTime.TimeOfDay >= currentTime.TimeOfDay).Count();
            var recaculatetotal = todoTotal + delayTotal + doneTotal + todoDispatchTotal + delayDispatchTotal + dispatchListDoneTotal;

            doneTotal = doneTotal + dispatchListDoneTotal;
            response.TodoDetail(data, doneTotal, todoTotal, delayTotal, recaculatetotal);

            response.DispatcherDetail(data, dispatchListDoneTotal, todoDispatchTotal, delayDispatchTotal, dispatchTotal);
            return response;
            // return new ToDoListForReturnDto(result.Where(x => x.PrintTime is null).ToList(), doneTotal, todoTotal, delayTotal, total);
        }

        public async Task<ToDoListForReturnDto> ToDo(int buildingID)
        {
            try
            {
                var userID = _jwtService.GetUserID();
                var role = await _userRoleRepository.FindAll(x => x.UserID == userID).FirstOrDefaultAsync();

                var currentTime = DateTime.Now.ToLocalTime();
                currentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, 0);
                var currentDate = currentTime.Date;
                var value = currentTime.ToString("tt", CultureInfo.InvariantCulture);
                var morning = "AM";
                var start = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 00, 00);
                var model = await _repoToDoList.FindAll(x =>
                       x.IsDelete == false
                       && x.EstimatedStartTime.Date == currentDate
                       && x.EstimatedFinishTime.Date == currentDate
                       && x.GlueName.Contains(" + ")
                       && x.BuildingID == buildingID)
                   .ToListAsync();

                var lines = await _repoBuildingUser.FindAll().Include(x => x.Building).Where(x => x.Building.ParentID == buildingID).Select(x => x.BuildingID).ToListAsync();
                if (lines.Count > 0)
                {
                    model = model.Where(x => lines.Contains(x.LineID)).ToList();
                }
                else
                {
                    var allLines = await _repoBuilding.FindAll().Where(x => x.ParentID == buildingID).Select(x => x.ID).ToListAsync();
                    model = model.Where(x => allLines.Contains(x.LineID)).ToList();
                }
                // map dto
                var result = MapToTodolistDto(model);

                // filter by Middle of the day
                if (value == morning)
                    result = result.Where(x => x.EstimatedStartTime.TimeOfDay <= start.TimeOfDay).ToList();

                // caculate
                var total = result.Count();
                var data = result.Where(x => x.PrintTime is null && x.EstimatedFinishTime.TimeOfDay >= currentTime.TimeOfDay).ToList();
                var todoTotal = data.Count();
                var delayTotal = result.Where(x => x.PrintTime is null
                                                 && x.EstimatedFinishTime.TimeOfDay < currentTime.TimeOfDay
                                                 ).Count();
                var doneTotal = result.Where(x => x.PrintTime != null).Count();


                var EVA_UVTotal = result.Where(x => x.IsEVA_UV).Count();
                var dispatchListModel = await _repoDispatchList.FindAll(x =>
                                                           x.IsDelete == false
                                                           && x.EstimatedStartTime.Date == currentDate
                                                           && x.EstimatedFinishTime.Date == currentDate
                                                           && x.BuildingID == buildingID).ToListAsync();

                // map to dto
                var dispatchList = MapToDispatchListDto(dispatchListModel);
                if (value == morning)
                    dispatchList = dispatchList.Where(x => x.EstimatedStartTime.TimeOfDay <= start.TimeOfDay).ToList();
                var response = new ToDoListForReturnDto();

                var dispatchTotal = dispatchList.Count();
                var dispatchListDoneTotal = dispatchList.Where(x => x.FinishDispatchingTime != null).Count();
                var dispatchListResult = dispatchList.Where(x => x.FinishDispatchingTime == null && x.EstimatedFinishTime.TimeOfDay < currentTime.TimeOfDay).ToList();
                var delayDispatchTotal = dispatchListResult.Count();
                var todoDispatchTotal = dispatchList.Where(x => x.FinishDispatchingTime == null && x.EstimatedStartTime.TimeOfDay <= currentTime.TimeOfDay && x.EstimatedFinishTime.TimeOfDay >= currentTime.TimeOfDay).Count();
                var recaculatetotal = todoTotal + delayTotal + doneTotal + todoDispatchTotal + delayDispatchTotal + dispatchListDoneTotal;

                doneTotal = doneTotal + dispatchListDoneTotal;
                response.TodoDetail(data, doneTotal, todoTotal, delayTotal, recaculatetotal);

                response.DispatcherDetail(data, dispatchListDoneTotal, todoDispatchTotal, delayDispatchTotal, dispatchTotal);

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }

        // Helper for dispatchlist, dispatchlist delay
        List<DispatchListDto> MapToDispatchListDto(List<DispatchList> model)
        {
            var currentTime = DateTime.Now.ToLocalTime();
            var currentDate = currentTime.Date;
            var groupBy = model.GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime, x.GlueNameID });
            var dispatchlist = new List<DispatchListDto>();
            foreach (var todo in groupBy)
            {
                var item = todo.FirstOrDefault();
                var lineList = todo.Select(x => x.LineName).DistinctBy(x => x).OrderBy(x => x).ToList();

                var itemTodolist = new DispatchListDto();
                itemTodolist.ID = item.ID;
                itemTodolist.PlanID = item.PlanID;
                itemTodolist.MixingInfoID = item.MixingInfoID;
                itemTodolist.GlueID = item.GlueID;
                itemTodolist.LineID = item.LineID;
                itemTodolist.LineName = item.LineName;
                itemTodolist.GlueName = item.GlueName;
                itemTodolist.GlueNameID = item.GlueNameID;
                itemTodolist.Supplier = item.Supplier;
                itemTodolist.ColorCode = item.ColorCode;

                itemTodolist.StartDispatchingTime = item.StartDispatchingTime;
                itemTodolist.FinishDispatchingTime = item.FinishDispatchingTime;

                itemTodolist.PrintTime = item.PrintTime;

                itemTodolist.DeliveredAmount = item.DeliveredAmount;

                itemTodolist.EstimatedStartTime = item.EstimatedStartTime;
                itemTodolist.EstimatedFinishTime = item.EstimatedFinishTime;

                itemTodolist.FinishTimeOfPeriod = item.FinishTimeOfPeriod;
                itemTodolist.StartTimeOfPeriod = item.StartTimeOfPeriod;
                itemTodolist.IsDelete = item.IsDelete;

                itemTodolist.LineNames = lineList;
                itemTodolist.BuildingID = item.BuildingID;
                dispatchlist.Add(itemTodolist);
            }
            var modelTemp = dispatchlist.Where(x => x.EstimatedFinishTime.Date == currentDate)
                .OrderBy(x => x.EstimatedStartTime).GroupBy(x => new { x.GlueNameID, x.EstimatedStartTime, x.EstimatedFinishTime }).ToList();
            var result = new List<DispatchListDto>();
            foreach (var item in modelTemp)
            {
                result.AddRange(item.OrderByDescending(x => x.GlueName));
            }
            return result;
        }

        public async Task<DispatchListForReturnDto> DispatchList(int buildingID)
        {
            var userID = _jwtService.GetUserID();
            var role = await _userRoleRepository.FindAll(x => x.UserID == userID).FirstOrDefaultAsync();

            try
            {
                var currentTime = DateTime.Now.ToLocalTime();
                var currentDate = currentTime.Date;
                var value = currentTime.ToString("tt", CultureInfo.InvariantCulture);
                var morning = "AM";
                var start = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 00, 00);
                var model = await _repoDispatchList.FindAll(x =>
                       x.IsDelete == false
                       && x.EstimatedStartTime.Date == currentDate
                       && x.EstimatedFinishTime.Date == currentDate
                       && x.BuildingID == buildingID)
                   .ToListAsync();
                var lines = await _repoBuildingUser.FindAll().Include(x => x.Building).Where(x => x.Building.ParentID == buildingID).Select(x => x.BuildingID).ToListAsync();
                if (lines.Count > 0)
                {
                    model = model.Where(x => lines.Contains(x.LineID)).ToList();
                }
                else
                {
                    var allLines = await _repoBuilding.FindAll().Where(x => x.ParentID == buildingID).Select(x => x.ID).ToListAsync();
                    model = model.Where(x => allLines.Contains(x.LineID)).ToList();
                }
                // map to dto
                var dispatchList = MapToDispatchListDto(model);

                // filter by Middle of the day
                if (value == morning)
                    dispatchList = dispatchList.Where(x => x.EstimatedStartTime.TimeOfDay <= start.TimeOfDay).ToList();

                // Caculate
                var dispatchTotal = dispatchList.Count();

                var dispatchListResult = dispatchList.Where(x => x.FinishDispatchingTime == null && x.EstimatedStartTime.TimeOfDay <= currentTime.TimeOfDay && x.EstimatedFinishTime.TimeOfDay >= currentTime.TimeOfDay).ToList();
                var todoDispatchTotal = dispatchListResult.Count();

                var delayDispatchTotal = dispatchList.Where(x => x.FinishDispatchingTime == null && x.EstimatedFinishTime.TimeOfDay < currentTime.TimeOfDay).Count();
                var dispatchListDoneTotal = dispatchList.Where(x => x.FinishDispatchingTime != null).Count();


                var EVA_UVTotal = dispatchList.Where(x => x.IsEVA_UV).Count();
                var response = new DispatchListForReturnDto();
                response.DispatcherDetail(dispatchListResult, dispatchListDoneTotal, todoDispatchTotal, delayDispatchTotal, dispatchTotal);
                var todoModel = await _repoToDoList.FindAll(x =>
                                          x.IsDelete == false
                                          && x.EstimatedStartTime.Date == currentDate
                                          && x.EstimatedFinishTime.Date == currentDate
                                          && x.GlueName.Contains(" + ")
                                          && x.BuildingID == buildingID)
                                      .ToListAsync();

                // map dto
                var todoResult = MapToTodolistDto(todoModel);

                // filter by Middle of the day
                if (value == morning)
                    todoResult = todoResult.Where(x => x.EstimatedStartTime.TimeOfDay <= start.TimeOfDay).ToList();

                // caculate
                var total = todoResult.Count();
                var data = todoResult.Where(x => x.PrintTime is null && x.EstimatedFinishTime.TimeOfDay >= currentTime.TimeOfDay).ToList();
                var todoTotal = data.Count();
                var delayTotal = todoResult.Where(x => x.PrintTime is null
                                                    && x.EstimatedFinishTime.TimeOfDay < currentTime.TimeOfDay).Count();


                var doneTotal = todoResult.Where(x => x.PrintTime != null).Count();
                total = doneTotal + delayTotal + todoTotal + dispatchListDoneTotal + todoDispatchTotal + delayDispatchTotal;
                doneTotal = doneTotal + dispatchListDoneTotal;
                response.TodoDetail(dispatchListResult, doneTotal, todoTotal, delayTotal, total);

                return response;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<DispatchListForReturnDto> DispatchListDelay(int buildingID)
        {
            var userID = _jwtService.GetUserID();
            var role = await _userRoleRepository.FindAll(x => x.UserID == userID).FirstOrDefaultAsync();
            try
            {
                var currentTime = DateTime.Now.ToLocalTime();
                var currentDate = currentTime.Date;
                var value = currentTime.ToString("tt", CultureInfo.InvariantCulture);
                var morning = "AM";
                var start = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 00, 00);
                var model = await _repoDispatchList.FindAll(x =>
                       x.IsDelete == false
                       && x.EstimatedStartTime.Date == currentDate
                       && x.EstimatedFinishTime.Date == currentDate
                       && x.BuildingID == buildingID)
                   .ToListAsync();
                if (role.RoleID == (int)Enums.Role.Dispatcher)
                {
                    var lines = await _repoBuildingUser.FindAll().Include(x => x.Building).Where(x => x.Building.ParentID == buildingID).Select(x => x.BuildingID).ToListAsync();
                    model = model.Where(x => lines.Contains(x.LineID)).ToList();
                }
                else
                {
                    var lines = await _repoBuilding.FindAll().Where(x => x.ParentID == buildingID).Select(x => x.ID).ToListAsync();
                    model = model.Where(x => lines.Contains(x.LineID)).ToList();
                }
                // map to dto
                var dispatchList = MapToDispatchListDto(model);
                // filter by Middle of the day
                if (value == morning)
                    dispatchList = dispatchList.Where(x => x.EstimatedStartTime.TimeOfDay <= start.TimeOfDay).ToList();


                // caculate
                var dispatchTotal = dispatchList.Count();
                var dispatchListResult = dispatchList.Where(x => x.FinishDispatchingTime == null && x.EstimatedFinishTime.TimeOfDay < currentTime.TimeOfDay).ToList();
                var delayDispatchTotal = dispatchListResult.Count();
                var todoDispatchTotal = dispatchList.Where(x => x.FinishDispatchingTime == null && x.EstimatedStartTime.TimeOfDay <= currentTime.TimeOfDay && x.EstimatedFinishTime.TimeOfDay >= currentTime.TimeOfDay).Count();
                var dispatchListDoneTotal = dispatchList.Where(x => x.FinishDispatchingTime != null).Count();


                var EVA_UVTotal = dispatchList.Where(x => x.IsEVA_UV).Count();
                var response = new DispatchListForReturnDto();
                response.DispatcherDetail(dispatchListResult, dispatchListDoneTotal, todoDispatchTotal, delayDispatchTotal, dispatchTotal);
                var todoModel = await _repoToDoList.FindAll(x =>
                                                      x.IsDelete == false
                                                      && x.EstimatedStartTime.Date == currentDate
                                                      && x.EstimatedFinishTime.Date == currentDate
                                                      && x.GlueName.Contains(" + ")
                                                      && x.BuildingID == buildingID)
                                .ToListAsync();

                // map dto
                var todoResult = MapToTodolistDto(todoModel);

                // filter by Middle of the day
                if (value == morning)
                    todoResult = todoResult.Where(x => x.EstimatedStartTime.TimeOfDay <= start.TimeOfDay).ToList();

                // caculate doneTodolist
                var total = todoResult.Count();
                var data = todoResult.Where(x => x.PrintTime is null && x.EstimatedFinishTime.TimeOfDay >= currentTime.TimeOfDay).ToList();
                var todoTotal = data.Count();
                var delayTotal = todoResult.Where(x => x.PrintTime is null
                                                    && x.EstimatedFinishTime.TimeOfDay < currentTime.TimeOfDay).Count();
                var doneTotal = todoResult.Where(x => x.PrintTime != null).Count();

                total = doneTotal + delayTotal + todoTotal + dispatchListDoneTotal + todoDispatchTotal + delayDispatchTotal;
                doneTotal = doneTotal + dispatchListDoneTotal;
                response.TodoDetail(dispatchListResult, doneTotal, todoTotal, delayTotal, total);

                return response;

            }
            catch (Exception)
            {

                throw;
            }
        }

        // Khong su dung nua
        public async Task<List<DispatchListDto>> GetDispatchListDetail(int glueNameID, string estimatedStartTime, string estimatedFinishTime)
        {
            var start = Convert.ToDateTime(estimatedStartTime);
            var end = Convert.ToDateTime(estimatedFinishTime);
            var dispatchList = await _repoDispatchList.FindAll(x => x.GlueNameID == glueNameID
            && x.EstimatedFinishTime == end
            && x.EstimatedStartTime == start
            ).ProjectTo<DispatchListDto>(_configMapper).ToListAsync();

            return dispatchList;
        }

        // Pha them (Addition)
        public async Task<ToDoListForReturnDto> ToDoAddition(int buildingID)
        {
            try
            {
                var userID = _jwtService.GetUserID();
                var role = await _userRoleRepository.FindAll(x => x.UserID == userID).FirstOrDefaultAsync();

                var currentTime = DateTime.Now.ToLocalTime();
                currentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, 0);
                var currentDate = currentTime.Date;
                var start = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 00, 00);
                var model = await _repoToDoList.FindAll(x =>
                       x.IsDelete == false
                       && x.EstimatedStartTime.Date == currentDate
                       && x.EstimatedFinishTime.Date == currentDate
                       && x.GlueName.Contains(" + ")
                       && x.BuildingID == buildingID)
                   .ToListAsync();
                ///
                if (role.RoleID == (int)Enums.Role.Dispatcher)
                {
                    var lines = await _repoBuildingUser.FindAll().Include(x => x.Building).Where(x => x.Building.ParentID == buildingID).Select(x => x.BuildingID).ToListAsync();
                    model = model.Where(x => lines.Contains(x.LineID)).ToList();
                }
                else
                {
                    var lines = await _repoBuilding.FindAll().Where(x => x.ParentID == buildingID).Select(x => x.ID).ToListAsync();
                    model = model.Where(x => lines.Contains(x.LineID)).ToList();
                }

                // map dto
                var result = MapToTodolistDto(model);
                return new ToDoListForReturnDto(result, 0, 0, 0, 0);

            }
            catch
            {
                throw;
            }

        }

        /// Khong su dung
        public async Task<DispatchListForReturnDto> DispatchAddition(int buildingID)
        {
            try
            {
                var currentTime = DateTime.Now.ToLocalTime();
                currentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, 0);
                var currentDate = currentTime.Date;
                var start = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 00, 00);
                var model = (await _repoDispatchList.FindAll(x =>
                       x.IsDelete == false
                       && x.EstimatedStartTime.Date == currentDate
                       && x.EstimatedFinishTime.Date == currentDate
                       && x.BuildingID == buildingID)
                   .ToListAsync()).DistinctBy(x => x.GlueName).ToList();

                // map dto
                var result = MapToDispatchListDto(model);
                return new DispatchListForReturnDto(result, 0, 0, 0, 0);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }

        // Data of Dispatch Modal
        public async Task<List<DispatchDetailDto>> GetDispatchDetail(int buildingID, int glueNameID, DateTime estimatedStartTime, DateTime estimatedFinsihTime)
        {
            var userID = _jwtService.GetUserID();
            var role = await _userRoleRepository.FindAll(x => x.UserID == userID).FirstOrDefaultAsync();
            var dispatchlist = await _repoDispatch.FindAll(x => x.GlueNameID == glueNameID
                                && x.EstimatedStartTime == estimatedStartTime
                                && !x.IsDelete
                                && x.EstimatedFinishTime == estimatedFinsihTime)
                            .Include(x => x.Building)
                            .Select(x => new DispatchDetailDto
                            {
                                ID = x.ID,
                                Amount = x.Amount,
                                DeliveryTime = x.DeliveryTime,
                                LineName = x.Building.Name,
                                LineID = x.Building.ID
                            })
                            .ToListAsync();
            // decentralzation
            if (role.RoleID == (int)Enums.Role.Dispatcher)
            {
                var lines = await _repoBuildingUser.FindAll().Include(x => x.Building).Where(x => x.Building.ParentID == buildingID).Select(x => x.BuildingID).ToListAsync();
                dispatchlist = dispatchlist.Where(x => lines.Contains(x.LineID)).ToList();
            }
            else if (role.RoleID == (int)Enums.Role.Admin || role.RoleID == (int)Enums.Role.Supervisor || role.RoleID == (int)Enums.Role.Staff || role.RoleID == (int)Enums.Role.Worker)
            {
                var lines = await _repoBuilding.FindAll().Where(x => x.ParentID == buildingID).Select(x => x.ID).ToListAsync();
                dispatchlist = dispatchlist.Where(x => lines.Contains(x.LineID)).ToList();
            }
            return dispatchlist;
        }

        public async Task<List<MixingInfo>> GetMixingInfoHistory(int buildingID, int glueNameID, string estimatedStartTime, string estimatedFinishTime)
        {
            var startPeriod = Convert.ToDateTime(estimatedStartTime);
            var endPeriod = Convert.ToDateTime(estimatedFinishTime);
            var ct = DateTime.Now;
            var history = await _repoMixingInfo.FindAll(x => x.GlueNameID == glueNameID
                                                        && x.BuildingID == buildingID
                                                        && x.EstimatedStartTime <= ct
                                                        && x.EstimatedFinishTime >= ct
                                                        )
                                                .Include(x => x.MixingInfoDetails)
                                                .OrderByDescending(x => x.CreatedTime)
                                                .ToListAsync();
            return history;
        }

        //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
        public async Task<object> MixedHistory(int MixingInfoID)
        {
            var result = await _repoMixingInfoDetail.FindAll().Include(x => x.Ingredient).Where(x => x.MixingInfoID == MixingInfoID).Select(x => new
            {
                ID = x.ID,
                Amount = x.Amount,
                Batch = x.Batch,
                Ingredient_ID = x.IngredientID,
                Ingredient_Name = x.Ingredient.Name,
                MixingInfo_ID = x.MixingInfoID,
                Time_Start = x.Time_Start

            }).ToListAsync();
            return new
            {
                status = true,
                result
            };
        }

        // MixingInfo
        public MixingInfo FindPrintGlue(int mixingInfoID)
        {
            var item = _repoMixingInfo.FindAll(x => x.ID == mixingInfoID).Include(x => x.MixingInfoDetails).FirstOrDefault();
            return item;
        }

        // Dispatch data
        public async Task<object> Dispatch(DispatchParams todolistDto)
        {
            var dispatches = await _repoDispatch.FindAll(x => x.MixingInfoID == todolistDto.MixingInfoID && x.CreatedTime.Date == todolistDto.EstimatedFinishTime.Date)
                .Include(x => x.Building)
                .Select(x => new DispatchTodolistDto
                {
                    ID = x.ID,
                    LineID = x.LineID,
                    Line = x.Building.Name,
                    MixingInfoID = x.MixingInfoID,
                    Real = x.Amount,
                    CreatedTime = x.CreatedTime,
                    DeliveryTime = x.DeliveryTime
                })
                .ToListAsync();
            return dispatches;
        }

        // khong su dung
        public async Task<ToDoListForReturnDto> DispatchDone(int buildingID)
        {
            var currentTime = DateTime.Now.ToLocalTime();
            var currentDate = currentTime.Date;
            var value = currentTime.ToString("tt", CultureInfo.InvariantCulture);
            var morning = "AM";
            var start = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 00, 00);
            var model = await _repoDispatchList.FindAll(x =>
                  x.IsDelete == false
                  && x.EstimatedStartTime.Date == currentDate
                  && x.EstimatedFinishTime.Date == currentDate
                  && x.BuildingID == buildingID)
               .ToListAsync();

            var groupBy = model.GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime, x.GlueName });
            var todolist = new List<ToDoListDto>();
            foreach (var todo in groupBy)
            {
                var item = todo.FirstOrDefault();
                var lineList = todo.Select(x => x.LineName).OrderBy(x => x).ToList();
                var deliveredAmount = todo.FirstOrDefault().DeliveredAmount;

                var itemTodolist = new ToDoListDto();
                itemTodolist.ID = item.ID;
                itemTodolist.JobType = 2;
                itemTodolist.PlanID = item.PlanID;
                itemTodolist.MixingInfoID = item.MixingInfoID;
                itemTodolist.GlueID = item.GlueID;
                itemTodolist.LineID = item.LineID;
                itemTodolist.LineName = item.LineName;
                itemTodolist.GlueName = item.GlueName;
                itemTodolist.Supplier = item.Supplier;
                itemTodolist.Status = item.FinishDispatchingTime.HasValue == false ? false : item.FinishDispatchingTime.Value.ToRemoveSecond() <= item.EstimatedFinishTime;

                itemTodolist.StartMixingTime = null;
                itemTodolist.FinishMixingTime = null;

                itemTodolist.StartStirTime = null;
                itemTodolist.FinishStirTime = null;

                itemTodolist.StartDispatchingTime = item.StartDispatchingTime;
                itemTodolist.FinishDispatchingTime = item.FinishDispatchingTime;

                itemTodolist.PrintTime = item.PrintTime;

                itemTodolist.MixedConsumption = 0;
                itemTodolist.DeliveredConsumption = 0;
                itemTodolist.StandardConsumption = 0;
                itemTodolist.DeliveredAmount = deliveredAmount;

                itemTodolist.DispatchTime = item.CreatedTime;
                itemTodolist.EstimatedStartTime = item.EstimatedStartTime;
                itemTodolist.EstimatedFinishTime = item.EstimatedFinishTime;

                itemTodolist.AbnormalStatus = item.AbnormalStatus;

                itemTodolist.LineNames = lineList;
                itemTodolist.BuildingID = item.BuildingID;
                todolist.Add(itemTodolist);
            }
            var modelTemp = todolist.Where(x => x.EstimatedFinishTime.Date == currentDate)
               .OrderBy(x => x.EstimatedStartTime).GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime }).ToList();
            var result = new List<ToDoListDto>();
            foreach (var item in modelTemp)
            {
                result.AddRange(item.OrderByDescending(x => x.GlueName));
            }
            if (value == morning)
            {
                result = result.Where(x => x.EstimatedStartTime.TimeOfDay <= start.TimeOfDay).ToList();
            }

            var doneList = new List<ToDoListDto>();
            var doneListTemp = result.Where(x => x.PrintTime != null).ToList(); /*Leo Update*/
            var groupbyTime = doneListTemp.GroupBy(x => x.EstimatedStartTime).ToList();
            foreach (var item in groupbyTime)
            {
                var doneItem = item.Where(x => x.GlueName.Contains(" + ")).OrderBy(x => x.GlueName).OrderBy(x => x.Supplier).ToList();
                var doneItem2 = item.Where(x => !x.GlueName.Contains(" + ")).OrderBy(x => x.GlueName).OrderBy(x => x.Supplier).ToList();
                var res = doneItem.Concat(doneItem2).ToList();
                doneList.AddRange(res);
            }
            var total = result.Count;
            var doneTotal = doneList.Count;

            /*Leo Update*/
            var todoTotal = result.Where(x => x.PrintTime is null && x.EstimatedFinishTime >= currentTime && x.GlueName.Contains(" + ")).Count();
            var delayTotal = result.Where(x => x.PrintTime is null && x.EstimatedFinishTime < currentTime && x.GlueName.Contains(" + ")).Count();
            /*Leo Update*/
            return new ToDoListForReturnDto(doneList, doneTotal, todoTotal, delayTotal, total);
        }

        #endregion

        #region Todolist Action
        //  Action of todolist table

        #region Helper For GenerateToDoList
        private async Task<List<GlueForGenerateToDoListDto>> GetAllMultipleGlueByToday(List<int> plans)
        {
            var currentTime = DateTime.Now;
            var currentDate = currentTime.Date;
            var plansModel = await _repoPlan.FindAll(x => plans.Contains(x.ID))
                .Include(x => x.Building)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueName)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueIngredients)
                    .ThenInclude(x => x.Ingredient)
                    .ThenInclude(x => x.Supplier)
                    .SelectMany(x => x.BPFCEstablish.Glues.Where(x => x.isShow), (plan, glue) => new GlueForGenerateToDoListDto
                    {
                        WorkingHour = plan.WorkingHour,
                        HourlyOutput = plan.HourlyOutput,
                        FinishWorkingTime = plan.FinishWorkingTime,
                        StartWorkingTime = plan.StartWorkingTime,
                        DueDate = plan.DueDate,
                        Building = plan.Building,
                        PlanID = plan.ID,
                        BPFCID = plan.BPFCEstablishID,
                        CreatedDate = plan.CreatedDate,
                        Consumption = glue.Consumption,
                        GlueID = glue.ID,
                        BuildingKindID = plan.Building.KindID ?? 0,
                        KindID = plan.Building.KindID != null && glue.KindID != null && glue.KindID == plan.Building.KindID ? glue.KindID ?? 0 : 0,
                        GlueNameID = glue.GlueNameID,
                        GlueName = glue.Name,
                        ChemicalA = glue.GlueIngredients.FirstOrDefault(x => x.Position == "A").Ingredient,
                    }).Where(x => x.BuildingKindID == 0).ToListAsync();
            return plansModel;
        }
        private ResponseDetail<GlueForGenerateToDoListDto> ValidateData(List<GlueForGenerateToDoListDto> data)
        {

            if (data.Count == 0) return new ResponseDetail<GlueForGenerateToDoListDto>
            {
                Status = false,
                Message = "Không có danh sách keo nào cho kế hoạch làm việc này!"
            };
            return new ResponseDetail<GlueForGenerateToDoListDto>(null, true, "");
        }
        private ResponseDetail<Building> ValidateBuilding(Building building)
        {

            if (building is null) return new ResponseDetail<Building>
            {
                Status = false,
                Message = "Không tìm thấy tòa nhà nào trong hệ thống!"
            };

            if (building.LunchTime is null) return new ResponseDetail<Building>
            {
                Status = false,
                Message = $"Tòa nhà {building.Name} chưa cài đặt giờ ăn trưa!"
            };
            return new ResponseDetail<Building>(building, true, "");
        }
        private ResponseDetail<Building> ValidateLine(Building line)
        {
            if (line is null) return new ResponseDetail<Building>
            {
                Status = false,
                Message = "Không tìm thấy chuyền nào trong hệ thống!"
            };
            return new ResponseDetail<Building>(line, true, "");
        }
        private ResponseDetail<object> ValidateChemical(GlueForGenerateToDoListDto item)
        {
            // - Kiểm tra hóa chất lỗi cài đặt.
            if (item.ChemicalA is null) new ResponseDetail<object>
            {
                Status = false,
                Message = $"Keo {item.GlueName} không có hóa chất A!"
            };

            var hourlyOutput = item.HourlyOutput;
            if (hourlyOutput == 0) new ResponseDetail<object>
            {
                Status = false,
                Message = $"Vui lòng thêm sản lượng hàng giờ cho chuyền {item.Building.Name}!"
            };
            return new ResponseDetail<object>(null, true, "");
        }
        private ResponseDetail<List<PeriodMixing>> ValidatePeriod(Building data)
        {

            var building = data;

            if (building is null) return new ResponseDetail<List<PeriodMixing>>
            {
                Status = false,
                Message = "Không tìm thấy tòa nhà nào trong hệ thống!"
            };

            if (building.LunchTime is null) return new ResponseDetail<List<PeriodMixing>>
            {
                Status = false,
                Message = $"Tòa nhà {building.Name} chưa cài đặt giờ ăn trưa!"
            };


            if (building.PeriodMixingList.Where(x => x.IsDelete == false).Count() == 0) return new ResponseDetail<List<PeriodMixing>>
            {
                Status = false,
                Message = $"Tòa nhà {building.Name} chưa cài đặt period!"
            };

            return new ResponseDetail<List<PeriodMixing>>(null, true, "");
        }
        #endregion
        public async Task<object> GenerateToDoList(List<int> plans)
        {
            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var currentTime = DateTime.Now.ToRemoveSecond();
                    var currentDate = currentTime.Date;
                    // B1: Lấy tất cả các keo chứa nhiều hóa chất (phân biệt bằng kiểm tra chứa dấu (+)) trong ngày hiện tại dựa vào dueData.
                    var data = await GetAllMultipleGlueByToday(plans);

                    // B2: Lấy giờ ăn trưa (lunchTime) để tạo todolist
                    var validate = ValidateData(data);

                    var value = data.FirstOrDefault();
                    var line = await _repoBuilding.FindAll(x => x.ID == value.Building.ID).FirstOrDefaultAsync();

                    var validateLine = ValidateLine(line);
                    if (validateLine.Status == false) return validateLine;

                    var building = await _repoBuilding.FindAll(x => x.ID == line.ParentID)
                    .Include(x => x.LunchTime)
                   .Include(x => x.PeriodMixingList)
                   .ThenInclude(x => x.PeriodDispatchList)
                   .FirstOrDefaultAsync();

                    var validateBuilding = ValidateBuilding(building);
                    if (validateBuilding.Status == false) return validateBuilding;

                    var startLunchTimeBuilding = building.LunchTime.StartTime;
                    var endLunchTimeBuilding = building.LunchTime.EndTime;

                    // B3: Lấy ra period để tạo todolist
                    // +Kiểm tra period
                    var result = ValidatePeriod(building);
                    if (result.Status == false) return result;

                    // +Lấy ra period
                    var periods = building.PeriodMixingList.Where(x => x.IsDelete == false).ToList();
                    var endWorkingTime = new TimeSpan(16, 30, 0); // Giờ kết thúc làm việc

                    // + Lấy ra những period tăng ca
                    var todolistOvertime = periods.Where(x => x.IsOvertime == true).ToList();

                    var todolist = new List<ToDoListDto>();
                    // Tao Todo cho ASY
                    // B4: Gom nhóm theo keo (nhiều chuyền sẽ sử dụng chung loại keo nên gom nhóm)
                    var glues = data.Where(x => x.GlueName.Contains("+")).GroupBy(x => x.GlueName).ToList();
                    /* B5: Tạo nhiệm vụ
                    * Th1: Lên kế hoạch cho ngày hôm sau
                    * Th2: Lên kế hoạch trong ngày
                      Mac dinh khong tao gio tang ca
                    */
                    if (glues.Count > 0)
                    {
                        foreach (var glue in glues)
                        {
                            foreach (var item in glue)
                            {
                                // - Kiểm tra keo có bị lỗi cài đặt.
                                var validateChemical = ValidateChemical(item);
                                if (validateChemical.Status == false) return validateChemical;

                                var checmicalA = item.ChemicalA;
                                var hourlyOutput = item.HourlyOutput;
                                var finishWorkingTime = item.FinishWorkingTime;
                                double prepareTime = checmicalA.PrepareTime;

                                var kgPair = item.Consumption.ToDouble() / 1000;

                                var startPeriod = new TimeSpan(7, 30, 0);
                                var endPeriod = new TimeSpan(8, 30, 0);
                                // -Th1: Lên kế hoạch từ ngày hôm trước
                                if (item.DueDate.Date != currentDate && item.CreatedDate.Date == currentDate
                                    || item.DueDate.Date == currentDate && item.CreatedDate.Date != currentDate
                                    )
                                {
                                    // - Todolist sẽ được tạo theo giờ ăn trưa và period 
                                    for (int index = 0; index < periods.Count; index++)
                                    {
                                        var estimatedStartTime = item.DueDate.Date.Add(new TimeSpan(periods[index].StartTime.Hour, periods[index].StartTime.Minute, 0));
                                        var estimatedFinishTime = item.DueDate.Date.Add(new TimeSpan(periods[index].EndTime.Hour, periods[index].EndTime.Minute, 0));

                                        double replacementFrequency = (periods[index].EndTime - periods[index].StartTime).TotalHours;

                                        // - Nếu là period đầu tiên trong ngày thì chỉ tính từ 7:30 (TG bắt đầu làm việc của toàn công ty)
                                        if (index == 0)
                                        {
                                            replacementFrequency = (periods[index].EndTime.TimeOfDay - startPeriod).TotalHours;
                                        }
                                        var todo = new ToDoListDto(
                                            item.GlueID,
                                            item.GlueNameID.Value,
                                            item.GlueName,
                                            item.PlanID,
                                            item.Building.ParentID.Value,
                                            item.Building.ID,
                                            item.Building.Name,
                                            item.BPFCID,
                                            item.KindID,
                                            item.KindID == (int)Enums.Kind.EVA_UV,
                                            item.ChemicalA.Supplier.Name,
                                            kgPair * (double)hourlyOutput * replacementFrequency,
                                            estimatedStartTime,
                                            estimatedFinishTime
                                            );

                                        todolist.Add(todo);
                                    }
                                }
                                else // Tao abnorml plan
                                {

                                    for (int index = 0; index < periods.Count; index++)
                                    {
                                        var estimatedStartTime = item.DueDate.Date.Add(new TimeSpan(periods[index].StartTime.Hour, periods[index].StartTime.Minute, 0));
                                        var estimatedFinishTime = item.DueDate.Date.Add(new TimeSpan(periods[index].EndTime.Hour, periods[index].EndTime.Minute, 0));
                                        double replacementFrequency = (periods[index].EndTime - periods[index].StartTime).TotalHours;
                                        if (index == 0)
                                        {
                                            replacementFrequency = (periods[index].EndTime.TimeOfDay - startPeriod).TotalHours;
                                        }
                                        var todo = new ToDoListDto(
                                            item.GlueID,
                                            item.GlueNameID.Value,
                                            item.GlueName,
                                            item.PlanID,
                                            item.Building.ParentID.Value,
                                            item.Building.ID,
                                            item.Building.Name,
                                            item.BPFCID,
                                            item.KindID,
                                            item.KindID == (int)Enums.Kind.EVA_UV,
                                            item.ChemicalA.Supplier.Name,
                                            kgPair * (double)hourlyOutput * replacementFrequency,
                                            estimatedStartTime,
                                            estimatedFinishTime
                                           );

                                        // neu ton tai roi thi khong them nua

                                        if (item.StartWorkingTime.TimeOfDay <= periods[index].StartTime.TimeOfDay
                                            || item.StartWorkingTime.TimeOfDay >= periods[index].StartTime.TimeOfDay
                                            && item.StartWorkingTime.TimeOfDay <= periods[index].EndTime.TimeOfDay
                                            )
                                        {
                                            // 8:37 > 7:30

                                            if (estimatedStartTime.TimeOfDay <= item.CreatedDate.TimeOfDay
                                                    && estimatedFinishTime.TimeOfDay >= item.CreatedDate.TimeOfDay
                                                    ||
                                                    estimatedFinishTime.TimeOfDay >= item.CreatedDate.TimeOfDay
                                                    && estimatedStartTime.TimeOfDay >= item.CreatedDate.TimeOfDay
                                                    )
                                            {
                                                todolist.Add(todo);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var model = _mapper.Map<List<ToDoList>>(todolist);
                        var todolistForAdd = new List<ToDoList>();
                        foreach (var item in todolistOvertime)
                        {
                            var todolistModel = model.Where(x => x.EstimatedStartTime.TimeOfDay == item.StartTime.TimeOfDay
                                                                    && x.EstimatedFinishTime.TimeOfDay == item.EndTime.TimeOfDay).ToList();
                            todolistModel.ForEach(item =>
                            {
                                item.IsDelete = true;
                            });
                            todolistForAdd.AddRange(todolistModel);

                            var todolistNoOvertimeModel = model.Where(x => x.EstimatedStartTime.TimeOfDay != item.StartTime.TimeOfDay
                                                               && x.EstimatedFinishTime.TimeOfDay != item.EndTime.TimeOfDay).ToList();
                            todolistForAdd.AddRange(todolistNoOvertimeModel);

                        }
                        _repoToDoList.AddRange(todolistForAdd);
                        await _repoToDoList.SaveAll();
                    }

                    await AddDispatchList_v105(plans);

                    var plansModel = await _repoPlan.FindAll(x => plans.Contains(x.ID)).ToListAsync();
                    var userID = _jwtService.GetUserID();
                    plansModel.ForEach(item =>
                    {
                        item.UpdatedTime = currentTime;
                        item.UpdatedBy = userID;
                    });
                    _repoPlan.UpdateRange(plansModel);
                    await _repoPlan.SaveAll();

                    transaction.Complete();
                    return new ResponseDetail<object>
                    {
                        Status = true,
                        Message = "Tạo danh sách việc làm thành công!"
                    };
                }
                catch (Exception)
                {
                    transaction.Dispose();
                    return new ResponseDetail<object>
                    {
                        Status = false,
                        Message = "Tạo danh sách việc làm thất bại!"
                    };
                }
            }
        }
        // 3/17/2021
        private async Task AddDispatchList_v105(List<int> plans)
        {
            var userID = _jwtService.GetUserID();
            var currentTime = DateTime.Now.ToRemoveSecond();
            var currentDate = currentTime.Date;

            var plansModel = await GetAllMultipleGlueByToday(plans);

            var value = plansModel.FirstOrDefault();

            var line = await _repoBuilding.FindAll(x => x.ID == value.Building.ID).FirstOrDefaultAsync();

            var building = await _repoBuilding.FindAll(x => x.ID == line.ParentID)
                .Include(x => x.BuildingType)
                .Include(x => x.LunchTime)
                   .Include(x => x.PeriodMixingList)
                   .ThenInclude(x => x.PeriodDispatchList)
               .FirstOrDefaultAsync();

            var periods = building.PeriodMixingList.Where(x => x.IsDelete == false).ToList();
            var dispatchlistOvertime = periods.Where(x => x.IsOvertime).ToList();
            var startLunchTimeBuilding = building.LunchTime.StartTime;
            var endLunchTimeBuilding = building.LunchTime.EndTime;

            var dispatchlist = new List<DispatchListDto>();

            var glues = plansModel.Where(x => x.KindTypeCode != KindTypeOption.SPE).GroupBy(x => x.GlueName).ToList();
            foreach (var glue in glues)
            {
                foreach (var item in glue)
                {
                    // Nếu lên kế hoạch tu ngay hom truoc
                    if (item.DueDate.Date != currentDate && item.CreatedDate.Date == currentDate
                        || item.DueDate.Date == currentDate && item.CreatedDate.Date != currentDate
                        )
                    {
                        foreach (var periodMixingItem in periods)
                        {
                            var colorCode = 0;
                            var startTime = periodMixingItem.StartTime.TimeOfDay;
                            var endTime = periodMixingItem.EndTime.TimeOfDay;
                            var startTimeOfPeriod = item.DueDate.Date.Add(startTime);
                            var finishTimeOfPeriod = item.DueDate.Date.Add(endTime);
                            foreach (var periodDispatchItem in periodMixingItem.PeriodDispatchList.Where(x => x.IsDelete == false))
                            {
                                var startTimeDispatch = periodMixingItem.StartTime.TimeOfDay;
                                var endTimeDispatch = periodMixingItem.EndTime.TimeOfDay;
                                var estimatedStartTime = item.DueDate.Date.Add(startTimeDispatch);
                                var estimatedFinishTime = item.DueDate.Date.Add(endTimeDispatch);

                                var todo = new DispatchListDto();
                                todo.GlueName = item.GlueName;
                                todo.GlueID = item.GlueID;
                                todo.PlanID = item.PlanID;
                                todo.LineID = item.Building.ID;
                                todo.LineName = item.Building.Name;
                                todo.PlanID = item.PlanID;
                                todo.BPFCID = item.BPFCID;
                                todo.ColorCode = (ColorCode)colorCode++;
                                todo.Supplier = item.ChemicalA.Supplier.Name;
                                todo.PlanID = item.PlanID;
                                todo.GlueNameID = item.GlueNameID.Value;
                                todo.BuildingID = item.Building.ParentID.Value;
                                todo.EstimatedStartTime = estimatedStartTime;
                                todo.EstimatedFinishTime = estimatedFinishTime;
                                todo.CreatedTime = currentTime;
                                todo.CreatedBy = userID;
                                todo.StartTimeOfPeriod = startTimeOfPeriod;
                                todo.FinishTimeOfPeriod = finishTimeOfPeriod;
                                dispatchlist.Add(todo);
                            }
                        }

                    }
                    else// Nếu có đổi modelName trong ngày
                    {
                        foreach (var periodMixingItem in periods)
                        {
                            var colorCode = 0;
                            var startTime = periodMixingItem.StartTime.TimeOfDay;
                            var endTime = periodMixingItem.EndTime.TimeOfDay;
                            var startTimeOfPeriod = item.DueDate.Date.Add(startTime);
                            var finishTimeOfPeriod = item.DueDate.Date.Add(endTime);
                            foreach (var periodDispatchItem in periodMixingItem.PeriodDispatchList.Where(x => x.IsDelete == false))
                            {
                                var startTimeDispatch = periodMixingItem.StartTime.TimeOfDay;
                                var endTimeDispatch = periodMixingItem.EndTime.TimeOfDay;
                                var estimatedStartTime = item.DueDate.Date.Add(startTimeDispatch);
                                var estimatedFinishTime = item.DueDate.Date.Add(endTimeDispatch);
                                // Chi tao nhung todo > createdDate cua workplan
                                var createdTimeOfWorkPlan = item.CreatedDate.TimeOfDay;
                                if (
                                        createdTimeOfWorkPlan >= startTimeDispatch && createdTimeOfWorkPlan <= endTimeDispatch
                                    || startTimeDispatch >= createdTimeOfWorkPlan && endTimeDispatch >= createdTimeOfWorkPlan
                                    )
                                {
                                    var todo = new DispatchListDto();
                                    todo.GlueName = item.GlueName;
                                    todo.GlueID = item.GlueID;
                                    todo.PlanID = item.PlanID;
                                    todo.LineID = item.Building.ID;
                                    todo.LineName = item.Building.Name;
                                    todo.PlanID = item.PlanID;
                                    todo.BPFCID = item.BPFCID;
                                    todo.ColorCode = (ColorCode)colorCode++;
                                    todo.Supplier = item.ChemicalA.Supplier.Name;
                                    todo.PlanID = item.PlanID;
                                    todo.GlueNameID = item.GlueNameID.Value;
                                    todo.BuildingID = item.Building.ParentID.Value;
                                    todo.EstimatedStartTime = estimatedStartTime;
                                    todo.EstimatedFinishTime = estimatedFinishTime;
                                    todo.CreatedTime = currentTime;
                                    todo.CreatedBy = userID;
                                    todo.StartTimeOfPeriod = startTimeOfPeriod;
                                    todo.FinishTimeOfPeriod = finishTimeOfPeriod;

                                    dispatchlist.Add(todo);
                                }

                            }
                        }
                    }
                }
            }
            try
            {
                var dispatchlistForAdd = new List<DispatchList>();
                var model = _mapper.Map<List<DispatchList>>(dispatchlist);
                foreach (var item in dispatchlistOvertime)
                {
                    var dispatchlistOvertimeModel = model.Where(x => x.StartTimeOfPeriod.TimeOfDay == item.StartTime.TimeOfDay
                                                            && x.FinishTimeOfPeriod.TimeOfDay == item.EndTime.TimeOfDay).ToList();
                    dispatchlistOvertimeModel.ForEach(item =>
                    {
                        item.IsDelete = true;
                    });

                    var dispatchlistNoOvertimeModel = model.Where(x => x.StartTimeOfPeriod.TimeOfDay != item.StartTime.TimeOfDay
                                                            && x.FinishTimeOfPeriod.TimeOfDay != item.EndTime.TimeOfDay).ToList();

                    dispatchlistForAdd.AddRange(dispatchlistOvertimeModel);
                    dispatchlistForAdd.AddRange(dispatchlistNoOvertimeModel);
                }
                _repoDispatchList.AddRange(dispatchlistForAdd);
                await _repoDispatchList.SaveAll();

            }
            catch (Exception)
            {
            }
        }
        private async Task AddDispatchList(List<int> plans)
        {
            var userID = _jwtService.GetUserID();
            var currentTime = DateTime.Now.ToRemoveSecond();
            var currentDate = currentTime.Date;
            var plansModel = await _repoPlan.FindAll(x => plans.Contains(x.ID))
                .Include(x => x.Building)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelName)
                 .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelNo)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArticleNo)
                     .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArtProcess)
                    .ThenInclude(x => x.Process)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueName)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueIngredients)
                    .ThenInclude(x => x.Ingredient)
                    .ThenInclude(x => x.Supplier)
                    .SelectMany(x => x.BPFCEstablish.Glues.Where(x => x.isShow), (plan, glue) => new
                    {
                        plan.WorkingHour,
                        plan.HourlyOutput,
                        plan.FinishWorkingTime,
                        plan.StartWorkingTime,
                        BPFCName = $"{plan.BPFCEstablish.ModelName.Name}->{plan.BPFCEstablish.ModelNo.Name}->{plan.BPFCEstablish.ArticleNo.Name}->{plan.BPFCEstablish.ArtProcess.Process.Name}",
                        plan.DueDate,
                        plan.Building,
                        PlanID = plan.ID,
                        BPFCID = plan.BPFCEstablishID,
                        plan.CreatedDate,
                        glue.Consumption,
                        GlueID = glue.ID,
                        KindID = glue.KindID ?? 0,
                        BuildingKindID = plan.Building.KindID ?? 0,
                        glue.GlueNameID,
                        GlueName = glue.Name,
                        ChemicalA = glue.GlueIngredients.FirstOrDefault(x => x.Position == "A").Ingredient,
                    }).Where(x => x.BuildingKindID == 0).ToListAsync();

            var value = plansModel.FirstOrDefault();

            var line = await _repoBuilding.FindAll(x => x.ID == value.Building.ID).FirstOrDefaultAsync();

            var building = await _repoBuilding.FindAll(x => x.ID == line.ParentID)
               .Include(x => x.PeriodMixingList)
               .FirstOrDefaultAsync();

            var periods = building.PeriodMixingList.Where(x => x.IsDelete == false).ToList();
            var endWorkTime = new TimeSpan(16, 30, 0);
            var dispatchlistOvertime = periods.Where(x => x.IsOvertime || (x.StartTime.TimeOfDay >= endWorkTime && x.EndTime.TimeOfDay >= endWorkTime)).ToList();
            var startLunchTimeBuilding = building.LunchTime.StartTime;
            var endLunchTimeBuilding = building.LunchTime.EndTime;

            var dispatchlist = new List<DispatchListDto>();

            var glues = plansModel.Where(x => x.KindID != (int)Enums.Kind.EVA_UV).GroupBy(x => x.GlueName).ToList();
            var startWorkingTime = new TimeSpan(7, 30, 0);
            foreach (var glue in glues)
            {
                foreach (var item in glue)
                {
                    var checmicalA = item.ChemicalA;
                    var startLunchTime = item.DueDate.Date.Add(new TimeSpan(startLunchTimeBuilding.Hour, startLunchTimeBuilding.Minute, 0));
                    var endLunchTime = item.DueDate.Date.Add(new TimeSpan(endLunchTimeBuilding.Hour, endLunchTimeBuilding.Minute, 0));

                    var finishWorkingTime = item.FinishWorkingTime;

                    double prepareTime = checmicalA.PrepareTime;

                    var kgPair = item.Consumption.ToDouble() / 1000;
                    double lunchHour = (endLunchTime - startLunchTime).TotalHours;
                    // Nếu lên kế hoạch tu ngay hom truoc
                    if (item.DueDate.Date != currentDate && item.CreatedDate.Date == currentDate
                        || item.DueDate.Date == currentDate && item.CreatedDate.Date != currentDate
                        )
                    {
                        for (int index = 0; index < periods.Count; index++)
                        {
                            // Neu la period dau tien thi bat dau tu 7:30 
                            var startTime = periods[index].StartTime.TimeOfDay;
                            var endTime = periods[index].EndTime.TimeOfDay;

                            var swt = item.DueDate.Date.Add(startTime);

                            var defaulHour = TimeSpan.FromHours(1);

                            if (index == 0)
                            {
                                startTime = startWorkingTime; // Bắt đầu làm việc từ 7:30
                            }

                            var startTimeOfPeriod = item.DueDate.Date.Add(startTime);
                            var finishTimeOfPeriod = item.DueDate.Date.Add(endTime);

                            // 7:30
                            var startDispatchTime = item.DueDate.Date.Add(startTime);
                            var startDispatchTimeTemp = startDispatchTime; // 8:30

                            var colorCode = 0;

                            while (true)
                            {
                                var totalMin = (finishTimeOfPeriod.TimeOfDay - startDispatchTimeTemp.TimeOfDay).TotalMinutes; // 9:00 - 9:30 = -30 phut
                                if (totalMin <= 0) break;

                                var endDispatchTime = startDispatchTimeTemp.Add(defaulHour); // 9:30
                                                                                             // 9:30 > 9:00

                                if (endDispatchTime.TimeOfDay > endTime)
                                {
                                    endDispatchTime = endDispatchTime.Date.Add(endTime);
                                }

                                var todo = new DispatchListDto();
                                todo.GlueName = item.GlueName;
                                todo.GlueID = item.GlueID;
                                todo.PlanID = item.PlanID;
                                todo.LineID = item.Building.ID;
                                todo.LineName = item.Building.Name;
                                todo.PlanID = item.PlanID;
                                todo.BPFCID = item.BPFCID;
                                todo.ColorCode = (ColorCode)colorCode++;
                                todo.Supplier = item.ChemicalA.Supplier.Name;
                                todo.PlanID = item.PlanID;
                                todo.GlueNameID = item.GlueNameID.Value;
                                todo.BuildingID = item.Building.ParentID.Value;
                                todo.EstimatedStartTime = startDispatchTimeTemp;
                                todo.EstimatedFinishTime = endDispatchTime;
                                todo.CreatedTime = currentTime;
                                todo.CreatedBy = userID;
                                todo.StartTimeOfPeriod = swt;
                                todo.FinishTimeOfPeriod = finishTimeOfPeriod;
                                dispatchlist.Add(todo);
                                startDispatchTimeTemp = endDispatchTime; // 8:00
                            }
                        }
                    }
                    else// Nếu có đổi modelName trong ngày
                    {
                        for (int index = 0; index < periods.Count; index++)
                        {
                            // Neu la period dau tien thi bat dau tu 7:30 
                            var startTime = periods[index].StartTime.TimeOfDay;
                            var endTime = periods[index].EndTime.TimeOfDay;

                            var swt = item.DueDate.Date.Add(startTime);

                            var defaulHour = TimeSpan.FromHours(1);

                            if (index == 0)
                            {
                                startTime = startWorkingTime; // Bắt đầu làm việc từ 7:30
                            }

                            var startTimeOfPeriod = item.DueDate.Date.Add(startTime);
                            var finishTimeOfPeriod = item.DueDate.Date.Add(endTime);

                            // 7:30
                            var startDispatchTime = item.DueDate.Date.Add(startTime);
                            var startDispatchTimeTemp = startDispatchTime; // 8:30

                            var colorCode = 0;
                            while (true)
                            {
                                var totalMin = (finishTimeOfPeriod.TimeOfDay - startDispatchTimeTemp.TimeOfDay).TotalMinutes; // 9:00 - 9:30 = -30 phut
                                if (totalMin <= 0) break;

                                var endDispatchTime = startDispatchTimeTemp.Add(defaulHour); // 9:30
                                // 9:30 > 9:00
                                if (endDispatchTime.TimeOfDay > endTime)
                                {
                                    endDispatchTime = endDispatchTime.Date.Add(endTime);
                                }
                                var todo = new DispatchListDto();
                                todo.GlueName = item.GlueName;
                                todo.GlueID = item.GlueID;
                                todo.PlanID = item.PlanID;
                                todo.LineID = item.Building.ID;
                                todo.LineName = item.Building.Name;
                                todo.PlanID = item.PlanID;
                                todo.BPFCID = item.BPFCID;
                                todo.ColorCode = (ColorCode)colorCode++;
                                todo.Supplier = item.ChemicalA.Supplier.Name;
                                todo.PlanID = item.PlanID;
                                todo.GlueNameID = item.GlueNameID.Value;
                                todo.BuildingID = item.Building.ParentID.Value;
                                todo.EstimatedStartTime = startDispatchTimeTemp;
                                todo.EstimatedFinishTime = endDispatchTime;
                                todo.CreatedTime = currentTime;
                                todo.CreatedBy = userID;
                                todo.StartTimeOfPeriod = startTimeOfPeriod;
                                todo.FinishTimeOfPeriod = finishTimeOfPeriod;

                                // chi tao nhung thoi gian trong tuong lai
                                var finsihTimeOfWorkplan = item.FinishWorkingTime.TimeOfDay;
                                var startTimeOfWorkplan = item.StartWorkingTime.TimeOfDay;
                                // 16:30 >= 7:30 && 7:30 
                                // 16:30 >= 16:30 && 11:00 >= 10:50
                                if (finsihTimeOfWorkplan >= startDispatchTimeTemp.TimeOfDay && endDispatchTime.TimeOfDay >= startTimeOfWorkplan)
                                {
                                    if (
                                        // 7:00 <= 8:45 && 7:30 >= 8:45 || 9:00 > 8:45 && 9:30 > 8:45
                                        startDispatchTimeTemp.TimeOfDay <= item.CreatedDate.TimeOfDay
                                    && endDispatchTime.TimeOfDay >= item.CreatedDate.TimeOfDay
                                    ||
                                        startDispatchTimeTemp.TimeOfDay >= item.CreatedDate.TimeOfDay
                                    && endDispatchTime.TimeOfDay >= item.CreatedDate.TimeOfDay
                                    )
                                        dispatchlist.Add(todo);
                                }
                                startDispatchTimeTemp = endDispatchTime; // 8:00
                            }
                        }
                    }
                }
            }
            try
            {
                var dispatchlistForAdd = new List<DispatchList>();
                var model = _mapper.Map<List<DispatchList>>(dispatchlist);
                foreach (var item in dispatchlistOvertime)
                {
                    var dispatchlistOvertimeModel = model.Where(x => x.StartTimeOfPeriod.TimeOfDay == item.StartTime.TimeOfDay
                                                            && x.FinishTimeOfPeriod.TimeOfDay == item.EndTime.TimeOfDay).ToList();
                    dispatchlistOvertimeModel.ForEach(item =>
                    {
                        item.IsDelete = true;
                    });

                    var dispatchlistNoOvertimeModel = model.Where(x => x.StartTimeOfPeriod.TimeOfDay != item.StartTime.TimeOfDay
                                                            && x.FinishTimeOfPeriod.TimeOfDay != item.EndTime.TimeOfDay).ToList();

                    dispatchlistForAdd.AddRange(dispatchlistOvertimeModel);
                    dispatchlistForAdd.AddRange(dispatchlistNoOvertimeModel);
                }
                _repoDispatchList.AddRange(dispatchlistForAdd);
                await _repoDispatchList.SaveAll();

            }
            catch (Exception)
            {
            }
        }

        // Không sử dụng
        public async Task<object> GenerateDispatchList(List<int> plans)
        {
            if (plans.Count == 0) return new
            {
                status = false,
                message = "Không có kế hoạch làm việc nào được gửi lên server"
            };
            var currentTime = DateTime.Now;
            var currentDate = currentTime.Date;
            var plansModel = await _repoPlan.FindAll(x => plans.Contains(x.ID))
                .Include(x => x.Building)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelName)
                 .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelNo)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArticleNo)
                     .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArtProcess)
                    .ThenInclude(x => x.Process)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueName)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueIngredients)
                    .ThenInclude(x => x.Ingredient)
                    .ThenInclude(x => x.Supplier)
                    .SelectMany(x => x.BPFCEstablish.Glues.Where(x => x.isShow), (plan, glue) => new
                    {
                        plan.WorkingHour,
                        plan.HourlyOutput,
                        plan.FinishWorkingTime,
                        plan.StartWorkingTime,
                        BPFCName = $"{plan.BPFCEstablish.ModelName.Name}->{plan.BPFCEstablish.ModelNo.Name}->{plan.BPFCEstablish.ArticleNo.Name}->{plan.BPFCEstablish.ArtProcess.Process.Name}",
                        plan.DueDate,
                        plan.Building,
                        PlanID = plan.ID,
                        BPFCID = plan.BPFCEstablishID,
                        plan.CreatedDate,
                        glue.Consumption,
                        GlueID = glue.ID,
                        glue.GlueNameID,
                        GlueName = glue.Name,
                        ChemicalA = glue.GlueIngredients.FirstOrDefault(x => x.Position == "A").Ingredient,
                    }).ToListAsync();

            var value = plansModel.FirstOrDefault();
            if (plansModel.Count == 0) return new
            {
                status = false,
                message = "Không có danh sách keo nào cho kế hoạch làm việc này!"
            };

            var line = await _repoBuilding.FindAll(x => x.ID == value.Building.ID).FirstOrDefaultAsync();
            if (line is null) return new
            {
                status = false,
                message = "Không tìm thấy chuyền nào trong hệ thống!"
            };

            var building = await _repoBuilding.FindAll(x => x.ID == line.ParentID)
               .Include(x => x.PeriodMixingList)
               .FirstOrDefaultAsync();
            if (building is null) return new
            {
                status = false,
                message = "Không tìm thấy tòa nhà nào trong hệ thống!"
            };

            if (building.LunchTime is null) return new
            {
                status = false,
                message = $"Tòa nhà {building.Name} chưa cài đặt giờ ăn trưa!"
            };
            var periods = building.PeriodMixingList.Where(x => x.IsDelete == false).ToList();
            if (periods.Count == 0 && periods == null) return new
            {
                status = false,
                message = $"Tòa nhà {building.Name} chưa cài đặt period!"
            };
            var startLunchTimeBuilding = building.LunchTime.StartTime;
            var endLunchTimeBuilding = building.LunchTime.EndTime;

            var dispatchlist = new List<DispatchListDto>();

            var glues = plansModel.GroupBy(x => x.GlueName).ToList();

            foreach (var glue in glues)
            {
                foreach (var item in glue)
                {
                    if (item.ChemicalA is null) return new
                    {
                        status = false,
                        message = $"Keo {item.GlueName} trong {item.BPFCName} không có hóa chất A! Vui lòng xem lại BPFC!"
                    };
                    var checmicalA = item.ChemicalA;
                    var startLunchTime = item.DueDate.Date.Add(new TimeSpan(startLunchTimeBuilding.Hour, startLunchTimeBuilding.Minute, 0));
                    var endLunchTime = item.DueDate.Date.Add(new TimeSpan(endLunchTimeBuilding.Hour, endLunchTimeBuilding.Minute, 0));

                    var finishWorkingTime = item.FinishWorkingTime;

                    double prepareTime = checmicalA.PrepareTime;

                    var kgPair = item.Consumption.ToDouble() / 1000;
                    double lunchHour = (endLunchTime - startLunchTime).TotalHours;
                    // Nếu lên kế hoạch cho ngày tiếp theo 
                    if (item.DueDate.Date != currentDate && item.CreatedDate.Date != currentDate)
                    {
                        for (int index = 0; index < periods.Count; index++)
                        {
                            var startWorkingTime = periods[index].StartTime;
                            var endWorkingTime = periods[index].EndTime;
                            var defaulHour = 30;
                            var dispatchAmount = periods[index].EndTime - periods[index].StartTime;
                            // 7:30
                            var startDispatchTime = item.DueDate.Date.Add(new TimeSpan(startWorkingTime.Hour, startWorkingTime.Minute, 0));
                            var startDispatchTimeTemp = startDispatchTime; // 7:30
                            while (true)
                            {
                                if (startDispatchTimeTemp.TimeOfDay >= endWorkingTime.TimeOfDay)
                                {
                                    break;
                                }
                                var endDispatchTime = startDispatchTimeTemp.AddMinutes(defaulHour); // 8:00
                                var todo = new DispatchListDto();
                                todo.GlueName = item.GlueName;
                                todo.GlueID = item.GlueID;
                                todo.PlanID = item.PlanID;
                                todo.LineID = item.Building.ID;
                                todo.LineName = item.Building.Name;
                                todo.PlanID = item.PlanID;
                                todo.BPFCID = item.BPFCID;
                                todo.Supplier = item.ChemicalA.Supplier.Name;
                                todo.PlanID = item.PlanID;
                                todo.ColorCode = (ColorCode)index;
                                todo.GlueNameID = item.GlueNameID.Value;
                                todo.BuildingID = item.Building.ParentID.Value;
                                todo.EstimatedStartTime = startDispatchTimeTemp;
                                todo.EstimatedFinishTime = endDispatchTime;

                                dispatchlist.Add(todo);
                                startDispatchTimeTemp = endDispatchTime; // 8:00
                            }
                        }
                    }
                    else
                    {
                        for (int index = 0; index < periods.Count; index++)
                        {
                            var startWorkingTime = periods[index].StartTime;
                            var endWorkingTime = periods[index].EndTime;
                            var startTimeOfPeriod = item.DueDate.Date.Add(new TimeSpan(startWorkingTime.Hour, startWorkingTime.Minute, 0));
                            var finishTimeOfPeriod = item.DueDate.Date.Add(new TimeSpan(endWorkingTime.Hour, endWorkingTime.Minute, 0));

                            var defaulHour = 30;
                            var dispatchAmount = periods[index].EndTime - periods[index].StartTime;
                            // 7:30
                            var startDispatchTime = item.DueDate.Date.Add(new TimeSpan(startWorkingTime.Hour, startWorkingTime.Minute, 0));
                            var startDispatchTimeTemp = startDispatchTime; // 7:30
                            var colorCode = 0;
                            while (true)
                            {
                                if (startDispatchTimeTemp.TimeOfDay >= endWorkingTime.TimeOfDay)
                                {
                                    break;
                                }

                                var endDispatchTime = startDispatchTimeTemp.AddMinutes(defaulHour); // 8:00
                                var todo = new DispatchListDto();
                                todo.GlueName = item.GlueName;
                                todo.GlueID = item.GlueID;
                                todo.PlanID = item.PlanID;
                                todo.LineID = item.Building.ID;
                                todo.LineName = item.Building.Name;
                                todo.PlanID = item.PlanID;
                                todo.BPFCID = item.BPFCID;
                                todo.ColorCode = (ColorCode)colorCode++;
                                todo.Supplier = item.ChemicalA.Supplier.Name;
                                todo.PlanID = item.PlanID;
                                todo.GlueNameID = item.GlueNameID.Value;
                                todo.BuildingID = item.Building.ParentID.Value;
                                todo.EstimatedStartTime = startDispatchTimeTemp;
                                todo.EstimatedFinishTime = endDispatchTime;

                                todo.StartTimeOfPeriod = startTimeOfPeriod;
                                todo.FinishTimeOfPeriod = finishTimeOfPeriod;

                                // chi tao nhung thoi gian trong tuong lai
                                var finsihTimeOfWorkplan = item.FinishWorkingTime.TimeOfDay;
                                var startTimeOfWorkplan = item.StartWorkingTime.TimeOfDay;
                                // 16:30 >= 7:30 && 7:30 
                                // 16:30 >= 10:50 && 11:00 >= 10:50
                                if (finsihTimeOfWorkplan >= startDispatchTimeTemp.TimeOfDay && endDispatchTime.TimeOfDay >= startTimeOfWorkplan)
                                    dispatchlist.Add(todo);
                                startDispatchTimeTemp = endDispatchTime; // 8:00
                            }
                        }
                    }
                }
            }
            try
            {
                var model = _mapper.Map<List<DispatchList>>(dispatchlist);
                _repoDispatchList.AddRange(model);
                _repoDispatchList.Save();
                return new
                {
                    status = true,
                    message = "Tạo danh sách giao thành công!"
                };
            }
            catch (Exception)
            {
                return new
                {
                    status = false,
                    message = "Tạo danh sách giao thất bại!"
                };
            }
        }
        // Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
        public async Task<bool> AddRange(List<ToDoList> toDoList)
        {
            _repoToDoList.AddRange(toDoList);
            try
            {
                return await _repoToDoList.SaveAll();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CancelRange(List<ToDoListForCancelDto> todolistList)
        {
            var flag = new List<bool>();
            foreach (var todolist in todolistList)
            {
                var model = _repoToDoList.FindAll(x => x.ID == todolist.ID && todolist.LineNames.Contains(x.LineName)).ToList();
                if (model is null) flag.Add(false);
                _repoToDoList.RemoveMultiple(model);
                flag.Add(await _repoToDoList.SaveAll());
            }
            return flag.All(x => x is true);
        }

        public async Task<bool> Cancel(ToDoListForCancelDto todolist)
        {
            var model = _repoToDoList.FindAll(x => x.ID == todolist.ID && todolist.LineNames.Contains(x.LineName)).ToList();
            if (model is null) return false;
            _repoToDoList.RemoveMultiple(model);
            return await _repoToDoList.SaveAll();
        }

        public void UpdateMixingTimeRange(ToDoListForUpdateDto model)
        {
            var list = _repoToDoList.FindAll(x => 
                    x.EstimatedStartTime == model.EstimatedStartTime 
                    && x.EstimatedFinishTime == model.EstimatedFinishTime 
                    && x.GlueName == model.GlueName
                    && x.BuildingID == model.BuildingID
            ).ToList();
            list.ForEach(x =>
            {
                x.FinishMixingTime = model.FinishTime.Value.ToLocalTime();
                x.StartMixingTime = model.StartTime.Value.ToLocalTime();
                x.MixedConsumption = model.Amount;
                x.MixingInfoID = model.MixingInfoID;
            });
            _repoToDoList.UpdateRange(list);
            _repoToDoList.Save();
        }

        // Đã chỉnh sửa lúc 8:18 1/28/2021
        public void UpdateDispatchTimeRange(ToDoListForUpdateDto model)
        {
            var dispatch = model.Dispatches.Select(x => x.Amount).ToList();
            var total = dispatch.Sum();
            var list = _repoToDoList.FindAll(x => x.MixingInfoID == model.MixingInfoID).ToList();
            list.ForEach(x =>
            {
                x.FinishDispatchingTime = model.FinishTime;
                x.StartDispatchingTime = model.StartTime;
                x.Status = model.FinishTime.Value.ToRemoveSecond() <= x.EstimatedFinishTime;
                x.DeliveredConsumption = total;
            });
            _repoToDoList.UpdateRange(list);
            _repoToDoList.Save();
        }

        public void UpdateStiringTimeRange(ToDoListForUpdateDto model)
        {
            var list = _repoToDoList.FindAll(x => x.MixingInfoID == model.MixingInfoID).ToList();
            list.ForEach(x =>
            {
                x.FinishStirTime = model.FinishTime.Value.ToLocalTime();
                x.StartStirTime = model.StartTime.Value.ToLocalTime();
            });
            _repoToDoList.UpdateRange(list);
            _repoToDoList.Save();
        }

        public MixingInfo PrintGlue(int mixingInfoID)
        {
            var mixing = _repoMixingInfo.FindById(mixingInfoID);
            if (mixing is null) return new MixingInfo();
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    var printTime = DateTime.Now.ToLocalTime();
                    mixing.PrintTime = printTime;
                    _repoMixingInfo.Update(mixing);
                    _repoMixingInfo.Save();
                    var todolist = _repoToDoList.FindAll(x => x.MixingInfoID == mixingInfoID).ToList();
                    todolist.ForEach(item =>
                    {
                        item.Status = mixing.Status;
                        item.PrintTime = mixing.PrintTime;

                    });
                    _repoToDoList.UpdateRange(todolist);
                    _repoToDoList.Save();



                    scope.Complete();
                    return mixing;
                }
                catch
                {
                    scope.Dispose();
                    return new MixingInfo();
                }
            }
        }

        public bool UpdateStartStirTimeByMixingInfoID(int mixingInfoID)
        {
            var mixing = _repoMixingInfo.FindById(mixingInfoID);
            if (mixing == null) return false;
            var lines = _repoBuilding.FindAll(x => x.ParentID == mixing.BuildingID).Select(x => x.ID).ToList();
            if (lines.Count == 0) return false;
            try
            {
                var list = _repoToDoList.FindAll(x => x.EstimatedStartTime == mixing.EstimatedStartTime && x.EstimatedFinishTime == mixing.EstimatedFinishTime && x.GlueName == mixing.GlueName && lines.Contains(x.LineID)).ToList();
                list.ForEach(x =>
                {
                    x.StartStirTime = DateTime.Now;
                });
                _repoToDoList.UpdateRange(list);
                _repoToDoList.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }


        public bool UpdateFinishStirTimeByMixingInfoID(int mixingInfoID)
        {
            var mixing = _repoMixingInfo.FindById(mixingInfoID);
            if (mixing == null) return false;
            var lines = _repoBuilding.FindAll(x => x.ParentID == mixing.BuildingID).Select(x => x.ID).ToList();
            try
            {
                var list = _repoToDoList.FindAll(x => x.EstimatedStartTime == mixing.EstimatedStartTime && x.EstimatedFinishTime == mixing.EstimatedFinishTime && x.GlueName == mixing.GlueName && lines.Contains(x.LineID)).ToList();
                list.ForEach(x =>
                {
                    x.FinishStirTime = DateTime.Now;
                });
                _repoToDoList.UpdateRange(list);
                _repoToDoList.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Pha them action
        public async Task<object> Addition(int glueNameID, int building, DateTime start, DateTime end)
        {
            var userID = _jwtService.GetUserID();
            var ct = DateTime.Now.ToRemoveSecond();
            var result = await _repoToDoList.FindAll()
                            .Where(x => x.BuildingID == building
                                && x.EstimatedStartTime.Date == ct.Date
                                && x.GlueNameID == glueNameID
                                && x.AbnormalStatus == false
                                && x.EstimatedStartTime.TimeOfDay <= ct.TimeOfDay
                                && x.EstimatedFinishTime.TimeOfDay >= ct.TimeOfDay)
                            .ToListAsync();
            var model = new List<ToDoList>();
            var src = DateTime.Now;


            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    model.Add(new ToDoList
                    {
                        AbnormalStatus = true,
                        PlanID = item.PlanID,
                        GlueID = item.GlueID,
                        GlueNameID = item.GlueNameID,
                        GlueName = item.GlueName,
                        BuildingID = item.BuildingID,
                        LineID = item.LineID,
                        BPFCID = item.BPFCID,
                        LineName = item.LineName,
                        Supplier = item.Supplier,
                        StandardConsumption = item.StandardConsumption,
                        EstimatedStartTime = ct,
                        EstimatedFinishTime = item.EstimatedFinishTime
                    });
                }
            }
            else
            {
                return new
                {
                    status = false,
                    message = "Không thể tạo Addition trong khoảng thời gian này! Vui lòng chọn lại khoảng thời gian phù hợp với hiện tại"
                };
            }
            var dispatchResult = await _repoDispatchList.FindAll()
                                    .Where(x => x.BuildingID == building
                                            && x.EstimatedStartTime.Date == ct.Date
                                            && x.GlueNameID == glueNameID
                                            && x.AbnormalStatus == false
                                            && x.EstimatedStartTime.TimeOfDay <= ct.TimeOfDay
                                            && x.EstimatedFinishTime.TimeOfDay >= ct.TimeOfDay
                                            && x.FinishDispatchingTime != null)
                                    .ToListAsync();
            var dispatchModel = new List<DispatchList>();

            var checkExistDispatch = await _repoDispatchList.FindAll()
                                            .Where(x => x.BuildingID == building
                                                && x.EstimatedStartTime.Date == ct.Date
                                                && x.GlueNameID == glueNameID
                                                && x.EstimatedStartTime.TimeOfDay <= ct.TimeOfDay
                                                && x.EstimatedFinishTime.TimeOfDay >= ct.TimeOfDay
                                                && x.FinishDispatchingTime == null)
                                            .AnyAsync();
            if (!checkExistDispatch)
            {
                foreach (var item in dispatchResult)
                {
                    dispatchModel.Add(new DispatchList
                    {
                        AbnormalStatus = true,
                        PlanID = item.PlanID,
                        GlueID = item.GlueID,
                        GlueNameID = item.GlueNameID,
                        GlueName = item.GlueName,
                        BuildingID = item.BuildingID,
                        LineID = item.LineID,
                        BPFCID = item.BPFCID,
                        LineName = item.LineName,
                        Supplier = item.Supplier,
                        EstimatedStartTime = ct,
                        EstimatedFinishTime = item.EstimatedFinishTime,
                        StartTimeOfPeriod = item.StartTimeOfPeriod,
                        FinishTimeOfPeriod = item.FinishTimeOfPeriod,
                        ColorCode = item.ColorCode,
                        CreatedTime = ct,
                        CreatedBy = userID

                    });
                }
            }
            // Neu giao roi thi add them lai
            // con chua giao thi khong can add
            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    _repoToDoList.AddRange(model);
                    await _repoToDoList.SaveAll();
                    if (!checkExistDispatch)
                    {
                        _repoDispatchList.AddRange(dispatchModel);
                        await _repoDispatchList.SaveAll();
                    }
                    transaction.Complete();
                    return new
                    {
                        status = true,
                        message = "Tạo Addition thành công!"
                    };
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                    return new
                    {
                        status = false,
                        message = $"Tạo Addition thất bại! {ex.Message}"
                    };
                }
            }

        }

        public async Task<ResponseDetail<object>> AdditionDispatch(int glueNameID)
        {
            var userID = _jwtService.GetUserID();
            var ct = DateTime.Now.ToRemoveSecond();
            var result = await _repoDispatchList.FindAll().Where(x => x.GlueNameID == glueNameID && x.AbnormalStatus == false && x.EstimatedStartTime.TimeOfDay <= ct.TimeOfDay && x.EstimatedFinishTime.TimeOfDay >= ct.TimeOfDay && x.FinishDispatchingTime != null).ToListAsync();
            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    item.EstimatedStartTime = ct;
                    item.FinishDispatchingTime = null;
                    item.AbnormalStatus = true;
                    item.DeliveredAmount = 0;
                    item.MixingInfoID = 0;
                    item.ID = 0;
                    item.CreatedTime = ct;
                    item.CreatedBy = userID;
                }
            }
            else
            {
                return new ResponseDetail<object>
                {
                    Status = false,
                    Message = "Không thể giao thêm trong khoảng thời gian này! Vui lòng chọn lại khoảng thời gian phù hợp với hiện tại"
                };
            }
            try
            {
                var model = _mapper.Map<List<DispatchList>>(result);
                _repoDispatchList.AddRange(model);
                _repoDispatchList.Save();
                return new ResponseDetail<object>
                {
                    Status = true,
                    Message = "Tạo giao thêm keo thành công!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDetail<object>
                {
                    Status = false,
                    Message = $"Tạo giao thêm keo thất bại! {ex.Message}"
                };
            }
        }

        public async Task<object> AddOvertime(List<int> plans)
        {
            var userID = _jwtService.GetUserID();
            if (plans.Count == 0) return new
            {
                status = false,
                message = "Không có kế hoạch làm việc nào được gửi lên server"
            };
            var currentTime = DateTime.Now;
            var currentDate = currentTime.Date;
            var plansModel = await _repoPlan.FindAll(x => plans.Contains(x.ID))
                .Include(x => x.Building)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueName)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueIngredients)
                    .ThenInclude(x => x.Ingredient)
                    .ThenInclude(x => x.Supplier)
                    .SelectMany(x => x.BPFCEstablish.Glues.Where(x => x.isShow), (plan, glue) => new
                    {
                        plan.WorkingHour,
                        plan.HourlyOutput,
                        plan.FinishWorkingTime,
                        plan.StartWorkingTime,
                        plan.DueDate,
                        plan.Building,
                        PlanID = plan.ID,
                        BPFCID = plan.BPFCEstablishID,
                        plan.CreatedDate,
                        glue.Consumption,
                        GlueID = glue.ID,
                        glue.GlueNameID,
                        GlueName = glue.Name,
                        ChemicalA = glue.GlueIngredients.FirstOrDefault(x => x.Position == "A").Ingredient,
                    }).ToListAsync();
            var glueList = plansModel.Select(x => x.GlueName).ToList();
            if (plansModel.Count == 0) return new
            {
                status = false,
                message = "Không có danh sách keo nào cho kế hoạch làm việc này!"
            };
            var value = plansModel.FirstOrDefault();

            var line = await _repoBuilding.FindAll(x => x.ID == value.Building.ID).FirstOrDefaultAsync();
            if (line is null) return new
            {
                status = false,
                message = "Không tìm thấy chuyền nào trong hệ thống!"
            };

            var building = await _repoBuilding.FindAll(x => x.ID == line.ParentID)
              .Include(x => x.LunchTime)
              .Include(x => x.PeriodMixingList)
              .FirstOrDefaultAsync();

            if (building is null) return new
            {
                status = false,
                message = "Không tìm thấy tòa nhà nào trong hệ thống!"
            };

            if (building.LunchTime is null) return new
            {
                status = false,
                message = $"Tòa nhà {building.Name} chưa cài đặt giờ ăn trưa!"
            };

            var periods = building.PeriodMixingList.Where(x => x.IsDelete == false).ToList();

            if (periods.Count == 0 && periods == null) return new
            {
                status = false,
                message = $"Tòa nhà {building.Name} chưa cài đặt period!"
            };
            // Chi tao gio tang ca
            periods = periods.Where(x => x.IsOvertime == true).ToList();
            var finishWorkingTimeOfWorkplan = periods.OrderByDescending(x => x.EndTime).FirstOrDefault().EndTime;
            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var planUpdate = await _repoPlan.FindAll(x => plans.Contains(x.ID)).ToListAsync();
                    planUpdate.ForEach(item =>
                    {
                        item.FinishWorkingTime = finishWorkingTimeOfWorkplan;
                        item.UpdatedOvertime = finishWorkingTimeOfWorkplan;
                        item.UpdatedOvertimeBy = userID;
                        item.IsOvertime = true;
                    });
                    _repoPlan.UpdateRange(planUpdate);
                    await _repoPlan.SaveAll();

                    var todolistForUpdate = new List<ToDoList>();
                    var dispatchlistForUpdate = new List<DispatchList>();
                    foreach (var item in periods)
                    {
                        var todolist = await _repoToDoList.FindAll(x => plans.Contains(x.PlanID) && x.EstimatedStartTime.TimeOfDay == item.StartTime.TimeOfDay
                                                                && x.EstimatedFinishTime.TimeOfDay == item.EndTime.TimeOfDay).ToListAsync();
                        todolist.ForEach(item =>
                        {
                            item.IsDelete = false;
                        });
                        todolistForUpdate.AddRange(todolist);

                        var dispatchlist = await _repoDispatchList.FindAll(x => plans.Contains(x.PlanID) && x.StartTimeOfPeriod.TimeOfDay == item.StartTime.TimeOfDay && x.FinishTimeOfPeriod.TimeOfDay == item.EndTime.TimeOfDay).ToListAsync();
                        dispatchlist.ForEach(item =>
                        {
                            item.IsDelete = false;
                        });
                        dispatchlistForUpdate.AddRange(dispatchlist);

                    }
                    _repoPlan.UpdateRange(planUpdate);
                    await _repoPlan.SaveAll();

                    // cap nhat todolist
                    _repoToDoList.UpdateRange(todolistForUpdate);
                    await _repoDispatchList.SaveAll();

                    //cap nhat dispatchlist
                    _repoDispatchList.UpdateRange(dispatchlistForUpdate);
                    await _repoDispatchList.SaveAll();
                    transaction.Complete();
                    return new
                    {
                        status = true,
                        message = "Đã cài đặt giờ tăng ca!"
                    };
                }
                catch (Exception)
                {
                    transaction.Dispose();
                    return new
                    {
                        status = false,
                        message = "Không cài đặt được giờ tăng ca!"
                    };
                }
            }

        }

        public async Task<object> RemoveOvertime(List<int> plans)
        {
            var userID = _jwtService.GetUserID();

            if (plans.Count == 0) return new
            {
                status = false,
                message = "Không có kế hoạch làm việc nào được gửi lên server"
            };
            var currentTime = DateTime.Now;
            var currentDate = currentTime.Date;
            var plansModel = await _repoPlan.FindAll(x => plans.Contains(x.ID))
                .Include(x => x.Building)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueName)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueIngredients)
                    .ThenInclude(x => x.Ingredient)
                    .ThenInclude(x => x.Supplier)
                    .SelectMany(x => x.BPFCEstablish.Glues.Where(x => x.isShow), (plan, glue) => new
                    {
                        plan.WorkingHour,
                        plan.HourlyOutput,
                        plan.FinishWorkingTime,
                        plan.StartWorkingTime,
                        plan.DueDate,
                        plan.Building,
                        PlanID = plan.ID,
                        BPFCID = plan.BPFCEstablishID,
                        plan.CreatedDate,
                        glue.Consumption,
                        GlueID = glue.ID,
                        glue.GlueNameID,
                        GlueName = glue.Name,
                        ChemicalA = glue.GlueIngredients.FirstOrDefault(x => x.Position == "A").Ingredient,
                    }).ToListAsync();
            var glueList = plansModel.Select(x => x.GlueName).ToList();
            if (plansModel.Count == 0) return new
            {
                status = false,
                message = "Không có danh sách keo nào cho kế hoạch làm việc này!"
            };
            var value = plansModel.FirstOrDefault();

            var line = await _repoBuilding.FindAll(x => x.ID == value.Building.ID).FirstOrDefaultAsync();
            if (line is null) return new
            {
                status = false,
                message = "Không tìm thấy chuyền nào trong hệ thống!"
            };

            var checkExistLine = _repoPlan.FindAll().Any(x => x.BuildingID == line.ID);

            var building = await _repoBuilding.FindAll(x => x.ID == line.ParentID)
                .Include(x => x.LunchTime)
              .Include(x => x.PeriodMixingList)
              .FirstOrDefaultAsync();

            if (building is null) return new
            {
                status = false,
                message = "Không tìm thấy tòa nhà nào trong hệ thống!"
            };

            if (building.LunchTime is null) return new
            {
                status = false,
                message = $"Tòa nhà {building.Name} chưa cài đặt giờ ăn trưa!"
            };

            var periods = building.PeriodMixingList.Where(x => x.IsDelete == false).ToList();

            if (periods.Count == 0 && periods == null) return new
            {
                status = false,
                message = $"Tòa nhà {building.Name} chưa cài đặt period!"
            };
            // Chi tao gio tang ca
            var finishWorkingTime = periods.Where(x => x.IsOvertime == false).OrderByDescending(x => x.EndTime).FirstOrDefault().EndTime;
            periods = periods.Where(x => x.IsOvertime == true).ToList();
            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var todolistForUpdate = new List<ToDoList>();
                    var dispatchlistForUpdate = new List<DispatchList>();
                    foreach (var item in periods)
                    {
                        var todolist = await _repoToDoList.FindAll(x => plans.Contains(x.PlanID) && x.EstimatedStartTime.TimeOfDay == item.StartTime.TimeOfDay
                                                                && x.EstimatedFinishTime.TimeOfDay == item.EndTime.TimeOfDay).ToListAsync();
                        todolist.ForEach(item =>
                        {
                            item.IsDelete = true;
                        });
                        todolistForUpdate.AddRange(todolist);

                        var dispatchlist = await _repoDispatchList.FindAll(x => plans.Contains(x.PlanID) && x.StartTimeOfPeriod.TimeOfDay == item.StartTime.TimeOfDay && x.FinishTimeOfPeriod.TimeOfDay == item.EndTime.TimeOfDay).ToListAsync();
                        dispatchlist.ForEach(item =>
                        {
                            item.IsDelete = true;
                        });
                        dispatchlistForUpdate.AddRange(dispatchlist);

                    }

                    // cap nhat workplan
                    var planUpdate = await _repoPlan.FindAll(x => plans.Contains(x.ID)).ToListAsync();
                    planUpdate.ForEach(item =>
                    {
                        item.FinishWorkingTime = finishWorkingTime;
                        item.UpdatedOvertime = finishWorkingTime;
                        item.UpdatedOvertimeBy = userID;
                        item.IsOvertime = false;
                    });
                    _repoPlan.UpdateRange(planUpdate);
                    await _repoPlan.SaveAll();

                    // cap nhat todolist
                    _repoToDoList.UpdateRange(todolistForUpdate);
                    await _repoDispatchList.SaveAll();

                    //cap nhat dispatchlist
                    _repoDispatchList.UpdateRange(dispatchlistForUpdate);
                    await _repoDispatchList.SaveAll();

                    transaction.Complete();
                    return new
                    {
                        status = true,
                        message = $"Đã hủy giờ tăng ca!"
                    };
                }
                catch (Exception)
                {
                    transaction.Dispose();
                    return new
                    {
                        status = false,
                        message = $"Không xóa được giờ tăng ca!"
                    };
                }
            }

            throw new NotImplementedException();
        }
        #endregion

        #region Mixing Action
        public async Task<MixingInfo> Mix(MixingInfoForCreateDto mixing)
        {
            try
            {
                var item = _mapper.Map<MixingInfoForCreateDto, MixingInfo>(mixing);
                item.Code = CodeUtility.RandomString(8);
                item.CreatedTime = DateTime.Now;
                var glue = await _repoGlue.FindAll().FirstOrDefaultAsync(x => x.isShow == true && x.ID == mixing.GlueID);
                item.ExpiredTime = DateTime.Now.AddHours(glue.ExpiredTime);
                _repoMixingInfo.Add(item);
                await _repoMixingInfo.SaveAll();
                // await _repoMixing.AddOrUpdate(item.ID);
                return item;
            }
            catch
            {
                return new MixingInfo();
            }
        }

        MixingDetailForResponse MixingDetail(string glueName, DateTime date)
        {
            var mixedModel = _repoMixingInfo.FindAll(x => x.CreatedTime.Date == date)
                         .Include(x => x.MixingInfoDetails)
                         .Include(x => x.Glue)
                             .ThenInclude(x => x.GlueName)
                         .Where(x => x.Glue.GlueName.Name == glueName).ToList();
            var count = _repoMixingInfo.FindAll().Include(x => x.Glue)
                             .ThenInclude(x => x.GlueName).Where(x => x.Glue.GlueName.Name.Equals(glueName))
                            .Count();
            double mixedCon = 0;
            double deliveryCon = 0;
            if (count == 0) // Chua add cai nao trong db thi return ve rong
            {
                return new MixingDetailForResponse();
            }
            else if (mixedModel.Count == 0) // neu ngay hien tai khong co thi lay cua ngay truoc do
            {
                date = date.AddDays(-1);
                return MixingDetail(glueName, date);
            }
            else // ngay hien tai co thi lay cua ngay hien tai
            {
                var mixingModel = mixedModel.FirstOrDefault();
                var deliveryModel = _repoDispatch.FindAll(x => mixedModel.Select(a => a.ID).Contains(x.MixingInfoID) && x.CreatedTime.Date == date).ToList();
                //deliveryCon = deliveryModel.Count() == 0 ? 0 : deliveryModel.Sum(x => x.Amount);
                double sumDelivery = 0;
                double sumMixed = 0;
                foreach (var item in mixedModel)
                {
                    var subDeliveryModel = deliveryModel.Where(x => x.MixingInfoID == item.ID);
                    sumDelivery += subDeliveryModel.Count() == 0 ? 0 : subDeliveryModel.Sum(x => x.Amount);

                    sumMixed += item.MixingInfoDetails.Sum(x => x.Amount);
                }
                deliveryCon = sumDelivery / deliveryModel.Count();
                mixedCon = sumMixed / mixedModel.Count();
                deliveryCon = Double.IsNaN(deliveryCon) ? 0 : deliveryCon;
                mixedCon = Double.IsNaN(mixedCon) ? 0 : mixedCon;
                return new MixingDetailForResponse(mixedCon, mixingModel.CreatedTime, deliveryCon, mixingModel.CreatedTime);
            }
        }

        public MixingDetailForResponse GetMixingDetail(MixingDetailParams obj)
        {
            var glueName = obj.GlueName.ToSafetyString().Trim();
            var date = DateTime.Now.ToLocalTime().Date;
            return MixingDetail(glueName, date);
        }

        #endregion

        #region Dispatchlist Action

        public async Task<ResponseDetail<object>> UpdateDispatchDetail(DispatchListForUpdateDto update)
        {
            var item = await _repoDispatchList.FindAll(x => x.ID == update.ID).FirstOrDefaultAsync();
            if (item == null)
                return new ResponseDetail<object>()
                {
                    Status = false,
                    Message = "Không tìm thấy dữ liệu!"
                };
            try
            {
                item.DeliveredAmount = update.Amount;
                item.CreatedTime = DateTime.Now;
                _repoDispatchList.Update(item);
                var status = await _repoDispatchList.SaveAll();

                return new ResponseDetail<object>()
                {
                    Status = true,
                    Message = "Thành công!"
                };
            }
            catch (Exception)
            {
                return new ResponseDetail<object>()
                {
                    Status = false,
                    Message = "Không cập nhật được dữ liệu!"
                };
                throw;
            }
        }

        public async Task<ResponseDetail<string>> FinishDispatch(FinishDispatchParams obj)
        {
            var dispatchlist = await _repoDispatchList.FindAll(x => obj.Lines.Contains(x.LineID) && x.GlueNameID == obj.GlueNameID && x.EstimatedStartTime == obj.EstimatedStartTime && x.EstimatedFinishTime == obj.EstimatedFinishTime).ToListAsync();
            dispatchlist.ForEach(item =>
            {
                item.FinishDispatchingTime = DateTime.Now.ToLocalTime();
            });
            _repoDispatchList.UpdateRange(dispatchlist);
            try
            {
                await _repoDispatchList.SaveAll();
                return new ResponseDetail<string>("Success", true, "");
            }
            catch (Exception ex)
            {
                return new ResponseDetail<string>("Error", false, ex.Message);
            }

        }

        public async Task<MixingInfo> PrintGlueDispatchListAsync(int mixingInfoID, int glueNameID, DateTime estimatedStartTime, DateTime estimatedFinsihTime)
        {
            var mixing = await _repoMixingInfo.FindAll(x => x.ID == mixingInfoID).FirstOrDefaultAsync();
            if (mixing is null) return new MixingInfo();
            var dispatchlist = await _repoDispatchList.FindAll(x => x.GlueNameID == glueNameID && x.EstimatedStartTime == estimatedStartTime && x.EstimatedFinishTime == estimatedFinsihTime).ToListAsync();
            // B1: Cap nhat print time cho tat ca cac line
            // B2: Nhung nhung multiple glue thi trong 30 phut phai giao 2 lan , nhung single glue thi giao 1 lan
            // TH1: Neu la multiple thi them moi 2 dong du lieu vao bang Dispatch
            // TH2: Them 1 dong du lieu vao bang dispatch
            using TransactionScope scope = new TransactionScopeAsync().Create();
            {
                try
                {
                    // B1
                    var printTime = DateTime.Now.ToLocalTime();
                    mixing.PrintTime = printTime;
                    _repoMixingInfo.Update(mixing);
                    _repoMixingInfo.Save();

                    dispatchlist.ForEach(item =>
                    {
                        item.Status = mixing.Status;
                        item.MixingInfoID = mixingInfoID;
                        item.PrintTime = mixing.PrintTime;
                    });

                    _repoDispatchList.UpdateRange(dispatchlist);
                    await _repoDispatchList.SaveAll();

                    // B2
                    var dispatchListDetail = new List<Dispatch>();
                    var groupBy = dispatchlist.GroupBy(x => new
                    {
                        x.GlueNameID,
                        x.EstimatedFinishTime,
                        x.EstimatedStartTime
                    }).ToList();
                    foreach (var groupByItem in groupBy)
                    {
                        foreach (var item in groupByItem)
                        {
                            var dispatchItem = await _repoDispatch.FindAll(x => x.GlueNameID == item.GlueNameID
                                                && x.LineID == item.LineID
                                                && x.EstimatedStartTime == item.EstimatedStartTime
                                                && x.EstimatedFinishTime == item.EstimatedFinishTime
                                                ).FirstOrDefaultAsync();
                            // TH1
                            if (item.GlueName.Contains("+"))
                            {
                                if (dispatchItem == null)
                                {
                                    var listAdd = new List<Dispatch>
                                {
                                    new Dispatch
                                    {
                                        MixingInfoID = item.MixingInfoID,
                                        LineID = item.LineID,
                                        GlueNameID = item.GlueNameID,
                                        EstimatedFinishTime = item.EstimatedFinishTime,
                                        EstimatedStartTime = item.EstimatedStartTime,
                                    },new Dispatch
                                    {
                                        MixingInfoID = item.MixingInfoID,
                                        LineID = item.LineID,
                                        GlueNameID = item.GlueNameID,
                                        EstimatedFinishTime = item.EstimatedFinishTime,
                                        EstimatedStartTime = item.EstimatedStartTime,
                                    },
                                };
                                    _repoDispatch.AddRange(listAdd);
                                    await _repoDispatch.SaveAll();
                                }
                            }
                            else // TH2
                            {
                                if (dispatchItem == null)
                                {
                                    var listAdd = new List<Dispatch>
                                {
                                    new Dispatch
                                    {
                                        MixingInfoID = item.MixingInfoID,
                                        LineID = item.LineID,
                                        GlueNameID = item.GlueNameID,
                                        EstimatedFinishTime = item.EstimatedFinishTime,
                                        EstimatedStartTime = item.EstimatedStartTime,
                                    }
                                };
                                    _repoDispatch.AddRange(listAdd);
                                    await _repoDispatch.SaveAll();
                                }
                            }
                        }
                    }
                    scope.Complete();
                    return mixing;

                }
                catch
                {
                    scope.Dispose();
                    return new MixingInfo();
                }
            }
        }

        public MixingInfo PrintGlueDispatchList(int mixingInfoID, int dispatchListID)
        {
            var mixing = _repoMixingInfo.FindById(mixingInfoID);
            if (mixing is null) return new MixingInfo();
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    var printTime = DateTime.Now.ToLocalTime();
                    mixing.PrintTime = printTime;
                    _repoMixingInfo.Update(mixing);
                    _repoMixingInfo.Save();
                    var dispatchlist = _repoDispatchList.FindAll(x => x.ID == dispatchListID).FirstOrDefault();
                    dispatchlist.Status = mixing.Status;
                    dispatchlist.MixingInfoID = mixingInfoID;
                    dispatchlist.PrintTime = mixing.PrintTime;
                    _repoDispatchList.Update(dispatchlist);
                    _repoDispatchList.Save();
                    var dispatchListDetail = new DispatchListDetail
                    {
                        MixingInfoID = mixingInfoID,
                        DispatchListID = dispatchListID,
                        CreatedTime = DateTime.Now,
                    };
                    _repoDispatchListDetail.Add(dispatchListDetail);
                    _repoDispatchListDetail.Save();


                    scope.Complete();
                    return mixing;

                }
                catch
                {
                    scope.Dispose();
                    return new MixingInfo();
                }
            }
        }

        public async Task<ResponseDetail<string>> UpdateDispatchDetail(DispatchDetailForUpdateDto item)
        {

            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var dispatchItem = await _repoDispatch.FindAll(x => x.ID == item.ID).FirstOrDefaultAsync();
                    dispatchItem.DeliveryTime = item.Amount != dispatchItem.Amount ? DateTime.Now.ToLocalTime() : dispatchItem.DeliveryTime;
                    dispatchItem.Amount = item.Amount;
                    _repoDispatch.Update(dispatchItem);
                    await _repoDispatch.SaveAll();

                    var dispatchlist = await _repoDispatchList.FindAll(x => item.LineID == x.LineID && x.GlueNameID == item.GlueNameID && x.EstimatedStartTime == item.EstimatedStartTime && x.EstimatedFinishTime == item.EstimatedFinishTime).ToListAsync();
                    dispatchlist.ForEach(x =>
                    {
                        if (x.GlueName.Contains("+"))
                        {
                            var dispatchAmount = _repoDispatch.FindAll(x => item.LineID == x.LineID && x.GlueNameID == item.GlueNameID && x.EstimatedStartTime == item.EstimatedStartTime && x.EstimatedFinishTime == item.EstimatedFinishTime).Sum(x => x.Amount);
                            double amount = dispatchAmount;
                            double test = amount / 1000;
                            x.DeliveredAmount = amount > 0 ? Math.Round((amount / 1000), 3) : 0;
                        }
                        else
                        {
                            double amount = item.Amount;
                            double test = amount / 1000;

                            x.DeliveredAmount = amount > 0 ? Math.Round((amount / 1000), 3) : 0;
                        }
                    });
                    _repoDispatchList.UpdateRange(dispatchlist);
                    await _repoDispatchList.SaveAll();

                    transaction.Complete();
                    return new ResponseDetail<string>("Success", true, "");
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                    return new ResponseDetail<string>("Error", true, ex.Message);
                    throw;
                }
            }

        }

        public async Task<ResponseDetail<string>> UpdateMixingInfoDispatchList(int mixingInfoID, int glueNameID, DateTime estimatedStartTime, DateTime estimatedFinsihTime)
        {
            var mixing = await _repoMixingInfo.FindAll(x => x.ID == mixingInfoID).FirstOrDefaultAsync();
            if (mixing is null) return new ResponseDetail<string>("Error", true, "Not found mixing!");
            var dispatchlist = await _repoDispatchList.FindAll(x => 
            x.GlueNameID == glueNameID 
            && x.EstimatedStartTime == estimatedStartTime 
            && x.EstimatedFinishTime == estimatedFinsihTime
            && x.BuildingID == mixing.BuildingID
            ).ToListAsync();

            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    dispatchlist.ForEach(item =>
                    {
                        item.Status = mixing.Status;
                        item.MixingInfoID = mixingInfoID;
                        item.PrintTime = mixing.PrintTime;
                    });

                    _repoDispatchList.UpdateRange(dispatchlist);
                    await _repoDispatchList.SaveAll();
                    transaction.Complete();
                    return new ResponseDetail<string>("Success", true, "");
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                    return new ResponseDetail<string>("Error", true, ex.Message);
                }
            }
        }

        #endregion

        #region Report

        public Byte[] ExcelExportDoneList(List<ToDoListForExportDto> model)
        {
            try
            {
                var currentTime = DateTime.Now;
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var memoryStream = new MemoryStream();
                using (ExcelPackage p = new ExcelPackage(memoryStream))
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "Henry Pham";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "Done List";
                    //Tạo một sheet để làm việc trên đó
                    p.Workbook.Worksheets.Add("Done List");

                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = p.Workbook.Worksheets["Done List"];

                    // đặt tên cho sheet
                    ws.Name = "Done List";
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 11;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";
                    var headerArray = new List<string>()
                    {
                        "Sequence",
                        "Line",
                        "Station",
                        "Model Name",
                        "Model NO",
                        "Article NO",
                        "Supplier",
                        "Glue",
                        "SMT",
                        "FMT",
                        "SST",
                        "Stir CT",
                        "AVG. RPM",
                        "FST",
                        "PT",
                        "SDT",
                        "FDT",
                        "Std. Con. (kg)",
                        "Mixed Con. (kg)",
                        "Delivered Con. (kg)",
                        "Status",
                        "EST",
                        "EFT",
                    };
                    int headerRowIndex = 1;
                    foreach (var headerItem in headerArray.Select((value, i) => new { i, value }))
                    {
                        var headerColIndex = headerItem.i + 1;
                        var headerExcelRange = ws.Cells[headerRowIndex, headerColIndex];
                        headerExcelRange.Value = headerItem.value;
                        headerExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        headerExcelRange.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#1F4E78"));
                        headerExcelRange.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#fff"));
                        headerExcelRange.Style.Font.Size = 16;

                    }

                    int bodyRowIndex = 1;
                    int bodyColIndex = 1;

                    foreach (var bodyItem in model)
                    {
                        bodyColIndex = 1;
                        bodyRowIndex++;

                        var sequenceExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        sequenceExcelRange.Value = bodyItem.Sequence;
                        sequenceExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sequenceExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var lineExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        lineExcelRange.Value = bodyItem.Line;
                        lineExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        lineExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var stationExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        stationExcelRange.Value = bodyItem.Station;
                        stationExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        stationExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var modelNameExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        modelNameExcelRange.Value = bodyItem.ModelName;
                        modelNameExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        modelNameExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var modelNOExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        modelNOExcelRange.Value = bodyItem.ModelNO;
                        modelNOExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        modelNOExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var articleNOExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        articleNOExcelRange.Value = bodyItem.ArticleNO;
                        articleNOExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        articleNOExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var supplierExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        supplierExcelRange.Value = bodyItem.Supplier;
                        supplierExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        supplierExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var glueNameExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        glueNameExcelRange.Value = bodyItem.GlueName;
                        glueNameExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        glueNameExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var SMTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null)  //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                        {
                            SMTExcelRange.Value = "-";
                        }
                        else
                        {
                            SMTExcelRange.Value = bodyItem.StartMixingTime == null || !bodyItem.GlueName.Contains(" + ") ? "N/A" : bodyItem.StartMixingTime.Value.ToString("HH:mm");
                        }
                        SMTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        SMTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var FMTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null)  //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                        {
                            FMTExcelRange.Value = "-";
                        }
                        else
                        {
                            FMTExcelRange.Value = bodyItem.FinishMixingTime == null || !bodyItem.GlueName.Contains(" + ") ? "N/A" : bodyItem.FinishMixingTime.Value.ToString("HH:mm");
                        }
                        FMTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        FMTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var SSTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null)  //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                        {
                            SSTExcelRange.Value = "-";
                        }
                        else
                        {
                            if (bodyItem.GlueName.Contains(" + ") && bodyItem.MixedConsumption < 1)
                            {
                                SSTExcelRange.Value = "Manual";

                            }
                            else if (bodyItem.GlueName.Contains(" + ")
                              && bodyItem.StartStirTime != null
                              && bodyItem.FinishStirTime != null
                              && bodyItem.MixedConsumption >= 1)
                            {
                                SSTExcelRange.Value = bodyItem.StartStirTime.Value.ToString("HH:mm");
                            }
                            else if (bodyItem.GlueName.Contains(" + ")
                             && bodyItem.StartStirTime == null
                             && bodyItem.FinishStirTime == null
                             && bodyItem.MixedConsumption >= 1)
                            {
                                SSTExcelRange.Value = "Error";
                            }
                            else
                            {
                                SSTExcelRange.Value = "N/A";
                            }
                        }
                        SSTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        SSTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var SCTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null)  //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                        {
                            SCTExcelRange.Value = "-";
                        }
                        else
                        {
                            if (bodyItem.GlueName.Contains(" + ")
                          && bodyItem.StartStirTime != null
                          && bodyItem.FinishStirTime != null
                          && bodyItem.MixedConsumption >= 1)
                            {
                                SCTExcelRange.Value = bodyItem.StirCicleTime;
                            }
                            else
                            {
                                SCTExcelRange.Value = "N/A";

                            }
                        }
                        SCTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        SCTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var AVGRPMExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null)  //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                        {
                            AVGRPMExcelRange.Value = "-";
                        }
                        else
                        {
                            if (bodyItem.GlueName.Contains(" + ")
                          && bodyItem.StartStirTime != null
                          && bodyItem.FinishStirTime != null
                          && bodyItem.MixedConsumption >= 1)
                            {
                                AVGRPMExcelRange.Value = bodyItem.AverageRPM;
                            }
                            else
                            {
                                AVGRPMExcelRange.Value = "N/A";

                            }
                        }
                        AVGRPMExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        AVGRPMExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var FSTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null)  //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                        {
                            FSTExcelRange.Value = "-";
                        }
                        else
                        {
                            if (bodyItem.GlueName.Contains(" + ") && bodyItem.MixedConsumption < 1)
                            {
                                FSTExcelRange.Value = "Manual";

                            }
                            else if (bodyItem.GlueName.Contains(" + ")
                              && bodyItem.StartStirTime != null
                              && bodyItem.FinishStirTime != null
                              && bodyItem.MixedConsumption >= 1)
                            {
                                FSTExcelRange.Value = bodyItem.FinishStirTime.Value.ToString("HH:mm");
                            }
                            else if (bodyItem.GlueName.Contains(" + ")
                             && bodyItem.StartStirTime == null
                             && bodyItem.FinishStirTime == null
                             && bodyItem.MixedConsumption >= 1)
                            {
                                FSTExcelRange.Value = "Error";
                            }
                            else
                            {
                                FSTExcelRange.Value = "N/A";
                            }
                        }
                        FSTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        FSTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var PTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null)  //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                        {
                            PTExcelRange.Value = "-";
                        }
                        else
                        {
                            PTExcelRange.Value = bodyItem.PrintTime == null ? "N/A" : bodyItem.PrintTime.Value.ToString("HH:mm");
                        }
                        PTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        PTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var SDTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null)  //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                        {
                            SDTExcelRange.Value = "-";
                        }
                        else
                        {
                            SDTExcelRange.Value = bodyItem.PrintTime == null ? "N/A" : bodyItem.PrintTime.Value.ToString("HH:mm");
                        }
                        SDTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        SDTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var FDTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null)  //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                        {
                            FDTExcelRange.Value = "-";
                        }
                        else
                        {
                            FDTExcelRange.Value = bodyItem.PrintTime == null ? "N/A" : bodyItem.PrintTime.Value.ToString("HH:mm");
                        }
                        FDTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        FDTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var StdConExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null)  //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                        {
                            StdConExcelRange.Value = "-";
                        }
                        else
                        {
                            StdConExcelRange.Value = $"{Math.Round(bodyItem.StandardConsumption, 2)}";  //update bởi Quỳnh (Leo 1/28/2021 11:46)
                        }
                        StdConExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        StdConExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var MixedConExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null)  //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                        {
                            MixedConExcelRange.Value = "-";
                        }
                        else
                        {
                            MixedConExcelRange.Value = !bodyItem.GlueName.Contains(" + ") ? "N/A" : $"{Math.Round(bodyItem.MixedConsumption, 2)}"; //update bởi Quỳnh (Leo 1/28/2021 11:46)
                        }
                        MixedConExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        MixedConExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var deliverdConExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null) //update bởi Quỳnh (Leo 1/28/2021 11:46)
                        {
                            deliverdConExcelRange.Value = "-";
                        }
                        else
                        {
                            deliverdConExcelRange.Value = $"{Math.Round(bodyItem.DeliveredConsumption, 2)}";
                        }
                        deliverdConExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        deliverdConExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var statusRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null) //update bởi Quỳnh (Leo 1/28/2021 11:46)
                        {
                            statusRange.Value = "-";
                        }
                        else
                        {
                            statusRange.Value = bodyItem.Status;
                            statusRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            statusRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));
                            statusRange.Style.Font.Color.SetColor(bodyItem.Status == "Pass" ? ColorTranslator.FromHtml("#28a745") : ColorTranslator.FromHtml("#a30000")); // #a30000
                        }

                        var ESTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        ESTExcelRange.Value = bodyItem.EstimatedStartTime == null ? "N/A" : bodyItem.EstimatedStartTime.ToString("HH:mm");
                        ESTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ESTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var EFTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        EFTExcelRange.Value = bodyItem.EstimatedFinishTime == null ? "N/A" : bodyItem.EstimatedFinishTime.ToString("HH:mm");
                        EFTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        EFTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));
                    } //#BDD7EE

                    int mergeFromColIndex = 1;
                    int mergeFromRowIndex = 2;
                    int mergeToRowIndex = 1;
                    foreach (var item in model.GroupBy(x => x.Sequence))
                    {
                        mergeToRowIndex += item.Count();
                        var sequenceExcelRange = ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeFromColIndex];
                        sequenceExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sequenceExcelRange.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#fff"));

                        sequenceExcelRange.Merge = true;
                        //ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeFromColIndex].Value = item.Key;
                        sequenceExcelRange.Style.Font.Size = 20;
                        sequenceExcelRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        sequenceExcelRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        sequenceExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sequenceExcelRange.Style.Fill.BackgroundColor.SetColor(item.Key % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));
                        mergeFromRowIndex = mergeToRowIndex + 1;
                    }
                    //Make all text fit the cells
                    //ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Cells[ws.Dimension.Address].Style.Font.Bold = true;
                    ws.Cells[ws.Dimension.Address].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[ws.Dimension.Address].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    //make the borders of cell F6 thick
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;

                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    return bin;
                }
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                Console.WriteLine(mes);
                return new Byte[] { };
            }
        }

        // report
        public async Task<Byte[]> ExportExcelToDoListByBuilding(int buildingID)
        {
            var currentTime = DateTime.Now.ToLocalTime();
            var currentDate = currentTime.Date;
            // B1: Lay ra tat task cua todolist
            // TH1: Lay cac task da hoan thanh
            // Th2: lay danh sach cac task chua hoan thanh va bi tre
            // B2: Map 2 danh sach tren cua todolist vao dto

            // B1: Lay ra tat task cua dispatchlist
            // TH1: Lay cac task da hoan thanh
            // Th2: lay danh sach cac task chua hoan thanh va bi tre
            // B2: Map 2 danh sach tren cua todolist vao dto

            var model = await _repoToDoList.FindAll(x =>
                   x.IsDelete == false
                   && x.EstimatedStartTime.Date == currentDate
                   && x.EstimatedFinishTime.Date == currentDate
                   && x.PrintTime != null //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                   && x.BuildingID == buildingID
                   )
                .Include(x => x.Plan)
                    .ThenInclude(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelName)
                .Include(x => x.Plan)
                    .ThenInclude(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelNo)
                 .Include(x => x.Plan)
                    .ThenInclude(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArticleNo)
                .Include(x => x.Plan)
                    .ThenInclude(x => x.Stations)
                .Select(x => new
                {
                    x.Plan,
                    x.EstimatedFinishTime,
                    x.EstimatedStartTime,
                    x.FinishDispatchingTime,
                    x.StartDispatchingTime,
                    x.FinishStirTime,
                    x.StartStirTime,
                    x.StartMixingTime,
                    x.FinishMixingTime,
                    x.PrintTime,
                    x.MixingInfoID,
                    x.MixedConsumption,
                    x.DeliveredConsumption,
                    x.StandardConsumption,
                    x.LineName,
                    x.Supplier,
                    x.GlueID,
                    x.GlueName,
                    x.Status,
                    Station = x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID) == null ? 0 : x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID).Amount,
                    ModelName = x.Plan.BPFCEstablish.ModelName == null ? "N/A" : x.Plan.BPFCEstablish.ModelName.Name,
                    ModelNO = x.Plan.BPFCEstablish.ModelNo == null ? "N/A" : x.Plan.BPFCEstablish.ModelNo.Name,
                    ArticleNO = x.Plan.BPFCEstablish.ArticleNo == null ? "N/A" : x.Plan.BPFCEstablish.ArticleNo.Name,
                })
               .ToListAsync();
            var delay = await _repoToDoList.FindAll(x =>
                  x.IsDelete == false
                  && x.EstimatedStartTime.Date == currentDate
                  && x.EstimatedFinishTime.Date == currentDate
                  && x.EstimatedFinishTime < currentTime
                  && x.PrintTime == null //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                  && x.BuildingID == buildingID
                  )
               .Include(x => x.Plan)
                   .ThenInclude(x => x.BPFCEstablish)
                   .ThenInclude(x => x.ModelName)
               .Include(x => x.Plan)
                   .ThenInclude(x => x.BPFCEstablish)
                   .ThenInclude(x => x.ModelNo)
                .Include(x => x.Plan)
                   .ThenInclude(x => x.BPFCEstablish)
                   .ThenInclude(x => x.ArticleNo)
               .Include(x => x.Plan)
                   .ThenInclude(x => x.Stations)
               .Select(x => new
               {
                   x.Plan,
                   x.EstimatedFinishTime,
                   x.EstimatedStartTime,
                   x.FinishDispatchingTime,
                   x.StartDispatchingTime,
                   x.FinishStirTime,
                   x.StartStirTime,
                   x.StartMixingTime,
                   x.FinishMixingTime,
                   x.PrintTime,
                   x.MixingInfoID,
                   x.MixedConsumption,
                   x.DeliveredConsumption,
                   x.StandardConsumption,
                   x.LineName,
                   x.Supplier,
                   x.GlueID,
                   x.GlueName,
                   x.Status,
                   Station = x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID) == null ? 0 : x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID).Amount,
                   ModelName = x.Plan.BPFCEstablish.ModelName == null ? "N/A" : x.Plan.BPFCEstablish.ModelName.Name,
                   ModelNO = x.Plan.BPFCEstablish.ModelNo == null ? "N/A" : x.Plan.BPFCEstablish.ModelNo.Name,
                   ArticleNO = x.Plan.BPFCEstablish.ArticleNo == null ? "N/A" : x.Plan.BPFCEstablish.ArticleNo.Name,
               }).Where(x => x.GlueName.Contains(" + ")) //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
              .ToListAsync();
            var exportList = new List<ToDoListForExportDto>();
            var groupBy = model.Concat(delay).GroupBy(x => new { x.EstimatedStartTime, x.GlueName })
                .OrderBy(x => x.Key.EstimatedStartTime)
                .ThenBy(x => x.Key.GlueName)
                .ToList();
            foreach (var groupByItem in groupBy.Select((value, i) => new { i, value }))
            {
                var sequence = groupByItem.i + 1;
                foreach (var item in groupByItem.value.OrderBy(x => x.LineName).ToList())
                {
                    var stirModel = await _repoStir.FindAll(a => a.MixingInfoID == item.MixingInfoID).OrderBy(x => x.CreatedTime).ToListAsync();
                    var averageRPM = item.MixingInfoID == 0 || stirModel.Count == 0 ? 0 : stirModel.Average(x => x.RPM);
                    var stirCicleTime = item.MixingInfoID == 0 || stirModel.Count == 0 ? 0 : stirModel.Sum(x => x.ActualDuration);
                    var exportItem = new ToDoListForExportDto();
                    exportItem.Sequence = sequence;
                    exportItem.Line = item.LineName;
                    exportItem.Station = item.Station;
                    exportItem.Supplier = item.Supplier;
                    var modelName = item.Plan.BPFCEstablish.ModelName;
                    var modelNO = item.Plan.BPFCEstablish.ModelName;
                    var articleNO = item.Plan.BPFCEstablish.ModelName;

                    exportItem.ModelName = item.ModelName;
                    exportItem.ModelNO = item.ModelNO;
                    exportItem.ArticleNO = item.ArticleNO;
                    exportItem.GlueName = item.GlueName;

                    exportItem.StartMixingTime = item.StartMixingTime;
                    exportItem.FinishMixingTime = item.FinishMixingTime;

                    exportItem.StartStirTime = item.StartStirTime;
                    exportItem.FinishStirTime = item.FinishStirTime;


                    exportItem.PrintTime = item.PrintTime;
                    exportItem.StirCicleTime = stirCicleTime;
                    exportItem.AverageRPM = Math.Floor(averageRPM).ToInt();

                    exportItem.StartDispatchingTime = item.StartDispatchingTime;
                    exportItem.FinishDispatchingTime = item.FinishDispatchingTime;

                    exportItem.StandardConsumption = item.StandardConsumption;

                    exportItem.MixedConsumption = item.MixedConsumption;
                    exportItem.DeliveredConsumption = item.DeliveredConsumption;
                    var status = item.FinishDispatchingTime.HasValue == false ? false : item.FinishDispatchingTime.Value.ToRemoveSecond() <= item.EstimatedFinishTime;
                    exportItem.Status = status ? "Pass" : "Fail";


                    exportItem.EstimatedStartTime = item.EstimatedStartTime;
                    exportItem.EstimatedFinishTime = item.EstimatedFinishTime;
                    exportList.Add(exportItem);
                }
            }
            var dispatchListModel = await _repoDispatchList.FindAll(x =>
                  x.IsDelete == false
                  && x.EstimatedStartTime.Date == currentDate
                  && x.EstimatedFinishTime.Date == currentDate
                  && x.FinishDispatchingTime != null
                  && x.BuildingID == buildingID
                  )
               .Include(x => x.Plan)
                   .ThenInclude(x => x.BPFCEstablish)
                   .ThenInclude(x => x.ModelName)
               .Include(x => x.Plan)
                   .ThenInclude(x => x.BPFCEstablish)
                   .ThenInclude(x => x.ModelNo)
                .Include(x => x.Plan)
                   .ThenInclude(x => x.BPFCEstablish)
                   .ThenInclude(x => x.ArticleNo)
               .Include(x => x.Plan)
                   .ThenInclude(x => x.Stations)
               .Select(x => new
               {
                   x.Plan,
                   x.EstimatedFinishTime,
                   x.EstimatedStartTime,
                   x.FinishDispatchingTime,
                   x.StartDispatchingTime,
                   x.PrintTime,
                   x.MixingInfoID,
                   x.DeliveredAmount,
                   x.LineName,
                   x.LineID,
                   x.Supplier,
                   x.GlueID,
                   x.GlueName,
                   x.Status,
                   Station = x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID) == null ? 0 : x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID).Amount,
                   ModelName = x.Plan.BPFCEstablish.ModelName == null ? "N/A" : x.Plan.BPFCEstablish.ModelName.Name,
                   ModelNO = x.Plan.BPFCEstablish.ModelNo == null ? "N/A" : x.Plan.BPFCEstablish.ModelNo.Name,
                   ArticleNO = x.Plan.BPFCEstablish.ArticleNo == null ? "N/A" : x.Plan.BPFCEstablish.ArticleNo.Name,
               })
              .ToListAsync();
            var delayDispatchListModel = await _repoDispatchList.FindAll(x =>
                 x.IsDelete == false
                 && x.EstimatedStartTime.Date == currentDate
                 && x.EstimatedFinishTime.Date == currentDate
                 && x.EstimatedFinishTime < currentTime
                 && x.FinishDispatchingTime == null //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                 && x.BuildingID == buildingID
                 )
              .Include(x => x.Plan)
                  .ThenInclude(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ModelName)
              .Include(x => x.Plan)
                  .ThenInclude(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ModelNo)
               .Include(x => x.Plan)
                  .ThenInclude(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ArticleNo)
              .Include(x => x.Plan)
                  .ThenInclude(x => x.Stations)
              .Select(x => new
              {
                  x.Plan,
                  x.EstimatedFinishTime,
                  x.EstimatedStartTime,
                  x.FinishDispatchingTime,
                  x.StartDispatchingTime,
                  x.PrintTime,
                  x.MixingInfoID,
                  x.DeliveredAmount,
                  x.LineName,
                  x.LineID,
                  x.Supplier,
                  x.GlueID,
                  x.GlueName,
                  x.Status,
                  Station = x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID) == null ? 0 : x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID).Amount,
                  ModelName = x.Plan.BPFCEstablish.ModelName == null ? "N/A" : x.Plan.BPFCEstablish.ModelName.Name,
                  ModelNO = x.Plan.BPFCEstablish.ModelNo == null ? "N/A" : x.Plan.BPFCEstablish.ModelNo.Name,
                  ArticleNO = x.Plan.BPFCEstablish.ArticleNo == null ? "N/A" : x.Plan.BPFCEstablish.ArticleNo.Name,
              })
             .ToListAsync();
            var groupByDispatchList = dispatchListModel.Concat(delayDispatchListModel).GroupBy(x => new { x.EstimatedStartTime, x.GlueName })
              .OrderBy(x => x.Key.EstimatedStartTime)
              .ThenBy(x => x.Key.GlueName)
              .ToList();
            foreach (var groupByItem in groupByDispatchList.Select((value, i) => new { i, value }))
            {
                var sequence = groupByItem.i + 1;
                foreach (var item in groupByItem.value.OrderBy(x => x.LineName).ToList())
                {
                    var dispatchModel = await _repoDispatch.FindAll(a => a.MixingInfoID == item.MixingInfoID && a.LineID == item.LineID)
                        .FirstOrDefaultAsync();
                    var exportItem = new ToDoListForExportDto();
                    exportItem.Sequence = sequence;
                    exportItem.Line = item.LineName;
                    exportItem.Station = item.Station;
                    exportItem.Supplier = item.Supplier;
                    var modelName = item.Plan.BPFCEstablish.ModelName;
                    var modelNO = item.Plan.BPFCEstablish.ModelName;
                    var articleNO = item.Plan.BPFCEstablish.ModelName;

                    exportItem.ModelName = item.ModelName;
                    exportItem.ModelNO = item.ModelNO;
                    exportItem.ArticleNO = item.ArticleNO;
                    exportItem.GlueName = item.GlueName;

                    exportItem.StartDispatchingTime = item.StartDispatchingTime;
                    exportItem.FinishDispatchingTime = item.FinishDispatchingTime;

                    exportItem.DeliveredConsumption = item.DeliveredAmount;
                    exportItem.DeliveredConsumptionEachLine = dispatchModel is null ? 0 : dispatchModel.Amount;
                    var status = item.FinishDispatchingTime.HasValue == false ? false : item.FinishDispatchingTime.Value.ToRemoveSecond() <= item.EstimatedFinishTime;
                    exportItem.Status = status ? "Pass" : "Fail";

                    exportItem.EstimatedStartTime = item.EstimatedStartTime;
                    exportItem.EstimatedFinishTime = item.EstimatedFinishTime;
                    exportList.Add(exportItem);
                }
            }
            return ExcelExportDoneList(exportList);
        }

        public async Task<byte[]> ExportExcelToDoListWholeBuilding()
        {
            var buildingList = await _repoBuilding.FindAll(x => x.Level == 2).ToListAsync();
            var buildings = buildingList.Select(x => x.ID).ToList();
            var buildingNameList = buildingList.Select(x => x.Name).ToList();
            var currentTime = DateTime.Now.ToLocalTime();
            var currentDate = currentTime.Date;
            var model = await _repoToDoList.FindAll(x =>
                   x.IsDelete == false
                   && x.EstimatedStartTime.Date == currentDate
                   && x.EstimatedFinishTime.Date == currentDate
                   && x.FinishDispatchingTime != null
                   && buildings.Contains(x.BuildingID))
                .Include(x => x.Plan)
                    .ThenInclude(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelName)
                .Include(x => x.Plan)
                    .ThenInclude(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelNo)
                 .Include(x => x.Plan)
                    .ThenInclude(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArticleNo)
                .Include(x => x.Plan)
                    .ThenInclude(x => x.Stations)
                .Select(x => new
                {
                    x.Plan,
                    x.EstimatedFinishTime,
                    x.EstimatedStartTime,
                    x.FinishDispatchingTime,
                    x.StartDispatchingTime,
                    x.FinishStirTime,
                    x.StartStirTime,
                    x.StartMixingTime,
                    x.FinishMixingTime,
                    x.PrintTime,
                    x.MixingInfoID,
                    x.MixedConsumption,
                    x.DeliveredConsumption,
                    x.StandardConsumption,
                    x.LineName,
                    x.LineID,
                    x.Supplier,
                    x.GlueID,
                    x.GlueName,
                    x.BuildingID,
                    x.Status,
                    Station = x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID) == null ? 0 : x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID).Amount,
                    ModelName = x.Plan.BPFCEstablish.ModelName == null ? "N/A" : x.Plan.BPFCEstablish.ModelName.Name,
                    ModelNO = x.Plan.BPFCEstablish.ModelNo == null ? "N/A" : x.Plan.BPFCEstablish.ModelNo.Name,
                    ArticleNO = x.Plan.BPFCEstablish.ArticleNo == null ? "N/A" : x.Plan.BPFCEstablish.ArticleNo.Name,
                })
               .ToListAsync();
            var delay = await _repoToDoList.FindAll(x =>
                 x.IsDelete == false
                 && x.EstimatedStartTime.Date == currentDate
                 && x.EstimatedFinishTime.Date == currentDate
                 && x.EstimatedFinishTime < currentTime
                 && x.FinishDispatchingTime == null
                 && buildings.Contains(x.BuildingID)
                 )
              .Include(x => x.Plan)
                  .ThenInclude(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ModelName)
              .Include(x => x.Plan)
                  .ThenInclude(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ModelNo)
               .Include(x => x.Plan)
                  .ThenInclude(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ArticleNo)
              .Include(x => x.Plan)
                  .ThenInclude(x => x.Stations)
              .Select(x => new
              {
                  x.Plan,
                  x.EstimatedFinishTime,
                  x.EstimatedStartTime,
                  x.FinishDispatchingTime,
                  x.StartDispatchingTime,
                  x.FinishStirTime,
                  x.StartStirTime,
                  x.StartMixingTime,
                  x.FinishMixingTime,
                  x.PrintTime,
                  x.MixingInfoID,
                  x.MixedConsumption,
                  x.DeliveredConsumption,
                  x.StandardConsumption,
                  x.LineName,
                  x.LineID,
                  x.Supplier,
                  x.GlueID,
                  x.GlueName,
                  x.BuildingID,
                  x.Status,
                  Station = x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID) == null ? 0 : x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID).Amount,
                  ModelName = x.Plan.BPFCEstablish.ModelName == null ? "N/A" : x.Plan.BPFCEstablish.ModelName.Name,
                  ModelNO = x.Plan.BPFCEstablish.ModelNo == null ? "N/A" : x.Plan.BPFCEstablish.ModelNo.Name,
                  ArticleNO = x.Plan.BPFCEstablish.ArticleNo == null ? "N/A" : x.Plan.BPFCEstablish.ArticleNo.Name,
              })
             .ToListAsync();
            var exportList = new List<ToDoListForExportDto>();
            var newModel = model.Concat(delay)
                .ToList();

            var groupByBuilding = newModel.GroupBy(x => x.BuildingID).OrderBy(x => x.Key).ToList();
            foreach (var building in groupByBuilding)
            {
                var groupBy = building.GroupBy(x => new { x.EstimatedStartTime, x.GlueName })
               .OrderBy(x => x.Key.EstimatedStartTime)
               .ThenBy(x => x.Key.GlueName)
               .ToList();
                foreach (var groupByItem in groupBy.Select((value, i) => new { i, value }))
                {
                    var sequence = groupByItem.i + 1;
                    foreach (var item in groupByItem.value.OrderBy(x => x.LineName).ToList())
                    {
                        var stirModel = await _repoStir.FindAll(a => a.MixingInfoID == item.MixingInfoID).ToListAsync();
                        var averageRPM = item.MixingInfoID == 0 || stirModel.Count == 0 ? 0 : stirModel.Average(x => x.RPM);
                        var stirCicleTime = item.MixingInfoID == 0 || stirModel.Count == 0 ? 0 : stirModel.Sum(x => x.ActualDuration);
                        var exportItem = new ToDoListForExportDto();
                        exportItem.Sequence = sequence;
                        exportItem.Line = item.LineName;
                        exportItem.BuildingID = item.BuildingID;
                        exportItem.Station = item.Station;
                        exportItem.Supplier = item.Supplier;
                        var modelName = item.Plan.BPFCEstablish.ModelName;
                        var modelNO = item.Plan.BPFCEstablish.ModelName;
                        var articleNO = item.Plan.BPFCEstablish.ModelName;

                        exportItem.ModelName = item.ModelName;
                        exportItem.ModelNO = item.ModelNO;
                        exportItem.ArticleNO = item.ArticleNO;
                        exportItem.GlueName = item.GlueName;

                        exportItem.StartMixingTime = item.StartMixingTime;
                        exportItem.FinishMixingTime = item.FinishMixingTime;

                        exportItem.StartStirTime = item.StartStirTime;
                        exportItem.FinishStirTime = item.FinishStirTime;

                        exportItem.PrintTime = item.PrintTime;
                        exportItem.StirCicleTime = stirCicleTime;
                        exportItem.AverageRPM = averageRPM;

                        exportItem.StartDispatchingTime = item.StartDispatchingTime;
                        exportItem.FinishDispatchingTime = item.FinishDispatchingTime;

                        exportItem.StandardConsumption = item.StandardConsumption;

                        exportItem.MixedConsumption = item.MixedConsumption;
                        exportItem.DeliveredConsumption = item.DeliveredConsumption;
                        var status = item.FinishDispatchingTime.HasValue == false ? false : item.FinishDispatchingTime.Value.ToRemoveSecond() <= item.EstimatedFinishTime;
                        exportItem.Status = status ? "Pass" : "Fail";


                        exportItem.EstimatedStartTime = item.EstimatedStartTime;
                        exportItem.EstimatedFinishTime = item.EstimatedFinishTime;
                        exportList.Add(exportItem);
                    }
                }

            }
            return ExcelExportForReport(exportList);
        }

        public async Task<byte[]> ExportExcelToDoListWholeBuilding(DateTime startTime, DateTime endTime)
        {
            var start = startTime.Date;
            var end = endTime.Date;
            var buildingList = await _repoBuilding.FindAll(x => x.Level == 2).ToListAsync();
            var buildings = buildingList.Select(x => x.ID).ToList();
            var buildingNameList = buildingList.Select(x => x.Name).ToList();
            var currentTime = DateTime.Now.ToLocalTime();
            var newModel = await _repoToDoList.FindAll(x =>
                   x.IsDelete == false
                   && x.EstimatedFinishTime.Date >= start && x.EstimatedFinishTime.Date <= end
                   && buildings.Contains(x.BuildingID))
                .Include(x => x.Plan)
                    .ThenInclude(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelName)
                .Include(x => x.Plan)
                    .ThenInclude(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelNo)
                 .Include(x => x.Plan)
                    .ThenInclude(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArticleNo)
                .Include(x => x.Plan)
                    .ThenInclude(x => x.Stations)
                .Select(x => new
                {
                    x.Plan,
                    x.EstimatedFinishTime,
                    x.EstimatedStartTime,
                    x.FinishDispatchingTime,
                    x.StartDispatchingTime,
                    x.FinishStirTime,
                    x.StartStirTime,
                    x.StartMixingTime,
                    x.FinishMixingTime,
                    x.PrintTime,
                    x.MixingInfoID,
                    x.MixedConsumption,
                    x.DeliveredConsumption,
                    x.StandardConsumption,
                    x.LineName,
                    x.LineID,
                    x.Supplier,
                    x.GlueID,
                    x.GlueName,
                    x.BuildingID,
                    x.Status,
                    Station = x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID) == null ? 0 : x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID).Amount,
                    ModelName = x.Plan.BPFCEstablish.ModelName == null ? "N/A" : x.Plan.BPFCEstablish.ModelName.Name,
                    ModelNO = x.Plan.BPFCEstablish.ModelNo == null ? "N/A" : x.Plan.BPFCEstablish.ModelNo.Name,
                    ArticleNO = x.Plan.BPFCEstablish.ArticleNo == null ? "N/A" : x.Plan.BPFCEstablish.ArticleNo.Name,
                })
               .ToListAsync();
            var exportList = new List<ToDoListForExportDto>();
            var groupByBuilding = newModel.GroupBy(x => x.BuildingID).OrderBy(x => x.Key).ToList();
            foreach (var building in groupByBuilding)
            {
                var groupBy = building.GroupBy(x => new { x.EstimatedStartTime, x.GlueName })
               .OrderBy(x => x.Key.EstimatedStartTime)
               .ThenBy(x => x.Key.GlueName)
               .ToList();
                foreach (var groupByItem in groupBy.Select((value, i) => new { i, value }))
                {
                    var sequence = groupByItem.i + 1;
                    foreach (var item in groupByItem.value.OrderBy(x => x.LineName).ToList())
                    {
                        var stirModel = await _repoStir.FindAll(a => a.MixingInfoID == item.MixingInfoID).ToListAsync();
                        var averageRPM = item.MixingInfoID == 0 || stirModel.Count == 0 ? 0 : stirModel.Average(x => x.RPM);
                        var stirCicleTime = item.MixingInfoID == 0 || stirModel.Count == 0 ? 0 : stirModel.Sum(x => x.ActualDuration);
                        var exportItem = new ToDoListForExportDto();
                        exportItem.Sequence = sequence;
                        exportItem.Line = item.LineName;
                        exportItem.BuildingID = item.BuildingID;
                        exportItem.Station = item.Station;
                        exportItem.Supplier = item.Supplier;
                        var modelName = item.Plan.BPFCEstablish.ModelName;
                        var modelNO = item.Plan.BPFCEstablish.ModelName;
                        var articleNO = item.Plan.BPFCEstablish.ModelName;

                        exportItem.ModelName = item.ModelName;
                        exportItem.ModelNO = item.ModelNO;
                        exportItem.ArticleNO = item.ArticleNO;
                        exportItem.GlueName = item.GlueName;

                        exportItem.StartMixingTime = item.StartMixingTime;
                        exportItem.FinishMixingTime = item.FinishMixingTime;

                        exportItem.StartStirTime = item.StartStirTime;
                        exportItem.FinishStirTime = item.FinishStirTime;

                        exportItem.PrintTime = item.PrintTime;
                        exportItem.StirCicleTime = stirCicleTime;
                        exportItem.AverageRPM = averageRPM;

                        exportItem.StartDispatchingTime = item.StartDispatchingTime;
                        exportItem.FinishDispatchingTime = item.FinishDispatchingTime;

                        exportItem.StandardConsumption = item.StandardConsumption;

                        exportItem.MixedConsumption = item.MixedConsumption;
                        exportItem.DeliveredConsumption = item.DeliveredConsumption;
                        var status = item.FinishDispatchingTime.HasValue == false ? false : item.FinishDispatchingTime.Value.ToRemoveSecond() <= item.EstimatedFinishTime;
                        exportItem.Status = status ? "Pass" : "Fail";


                        exportItem.EstimatedStartTime = item.EstimatedStartTime;
                        exportItem.EstimatedFinishTime = item.EstimatedFinishTime;
                        exportList.Add(exportItem);
                    }
                }

            }
            return ExcelExportForReport(exportList);
        }
        // Report 2
        private Byte[] ExcelExportForReport(List<ToDoListForExportDto> todolist)
        {
            try
            {
                var buildings = _repoBuilding.FindAll(x => x.Level == 2).ToList();
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var memoryStream = new MemoryStream();
                using (ExcelPackage p = new ExcelPackage(memoryStream))
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "Henry Pham";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "Mixing Room Report";
                    //Tạo một sheet để làm việc trên đó
                    var groupby = todolist.GroupBy(x => x.BuildingID).ToList();
                    if (groupby.Count == 0) return null;
                    foreach (var groupbyItem in groupby)
                    {
                        var building = buildings.FirstOrDefault(x => x.ID == groupbyItem.Key);
                        var currentTime = DateTime.Now.ToLocalTime();

                        var groupBySequence = groupbyItem.GroupBy(x => x.Sequence).Select(x => x.FirstOrDefault()).ToList();
                        var delayTotal = groupBySequence.Where(x => x.EstimatedFinishTime < currentTime
                            && x.FinishDispatchingTime == null).Count();
                        var total = groupBySequence.Count();
                        var doneTotal = groupBySequence.Where(x => x.FinishDispatchingTime != null && x.Status == "Pass").Count();
                        var percentageOfDelay = Math.Round(((double)delayTotal / total) * 100, 0);
                        var percentageOfDone = Math.Round(((double)doneTotal / total) * 100, 0);
                        var stirErrorTotal = groupBySequence.Where(x => x.GlueName.Contains("+") && x.StartStirTime == null && x.MixedConsumption >= 1).Count();
                        var statusFailTotal = groupBySequence.Where(x => x.FinishDispatchingTime != null && x.Status == "Fail").GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime, x.GlueName }).Count();
                        var percentageOfStirError = Math.Round(((double)stirErrorTotal / total) * 100, 0);
                        var percentageOfStatusFail = Math.Round(((double)statusFailTotal / total) * 100, 0);
                        var analyzeHeader = new List<string>
                    {
                        "Total",
                        total.ToString(),
                        "Rate",
                        "Root Cause",
                        "Action Plan"
                    };
                        var doneRow = new List<string>
                    {
                        "Done",
                        doneTotal.ToString(),
                        doneTotal == 0? "0%" : percentageOfDone + "%",
                        "-",
                        "-"
                    };
                        var delayRow = new List<string>
                    {
                        "Delay",
                        delayTotal.ToString(),
                        delayTotal == 0? "0%" : percentageOfDelay + "%",
                        delayTotal == 0? "-" : "Didn't implement the task before estimated finished time.",
                        delayTotal == 0? "-" : "Please Implement the task in advance."
                    };
                        var stirErrorRow = new List<string>
                    {
                        "Stir Error",
                        stirErrorTotal.ToString(),
                        Double.IsNaN(percentageOfStirError) ? "0%" : percentageOfStirError + "%",
                       stirErrorTotal == 0? "-" : "Didn't implement the task before estimated finished time.",
                        stirErrorTotal == 0? "-" :"Please Implement the task in advance."
                    };
                        var statusFailRow = new List<string>
                    {
                        "Status Fail",
                        statusFailTotal.ToString(),
                        Double.IsNaN(percentageOfStatusFail) ? "0%" : percentageOfStatusFail + "%",
                        statusFailTotal == 0? "-" :"Finished implementing the task later than estimated finished time.",
                        statusFailTotal == 0? "-" :"Please implement the task before estimated finished time."
                    };
                        var analyzeList = new List<List<string>> { analyzeHeader, doneRow, delayRow, stirErrorRow, statusFailRow };
                        var model = groupbyItem.Where(x => x != null).OrderBy(x => x.Sequence).ToList();

                        var sheet = building is null ? "N/A" : building.Name;
                        p.Workbook.Worksheets.Add(sheet);

                        // lấy sheet vừa add ra để thao tác
                        ExcelWorksheet ws = p.Workbook.Worksheets[sheet];

                        // đặt tên cho sheet
                        ws.Name = sheet;
                        // fontsize mặc định cho cả sheet
                        ws.Cells.Style.Font.Size = 11;
                        // font family mặc định cho cả sheet
                        ws.Cells.Style.Font.Name = "Calibri";
                        var headerArray = new List<string>()
                    {
                        "Sequence",
                        "Line",
                        "Station",
                        "Model Name",
                        "Model NO",
                        "Article NO",
                        "Supplier",
                        "Glue",
                        "SMT",
                        "FMT",
                        "SST",
                        "Stir CT",
                        "AVG. RPM",
                        "FST",
                        "PT",
                        "SDT",
                        "FDT",
                        "Std. Con.",
                        "Mixed Con.",
                        "Delivered Con.",
                        "Status",
                        "EST",
                        "EFT",
                    };
                        int headerRowIndex = 1;
                        foreach (var headerItem in headerArray.Select((value, i) => new { i, value }))
                        {
                            var headerColIndex = headerItem.i + 1;
                            var headerExcelRange = ws.Cells[headerRowIndex, headerColIndex];
                            headerExcelRange.Value = headerItem.value;
                            headerExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerExcelRange.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#1F4E78"));
                            headerExcelRange.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#fff"));
                            headerExcelRange.Style.Font.Size = 16;

                        }

                        int bodyRowIndex = 1;
                        int bodyColIndex = 1;
                        foreach (var bodyItem in model)
                        {
                            bodyColIndex = 1;
                            bodyRowIndex++;

                            var sequenceExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            sequenceExcelRange.Value = bodyItem.Sequence;
                            sequenceExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sequenceExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var lineExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            lineExcelRange.Value = bodyItem.Line;
                            lineExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            lineExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var stationExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            stationExcelRange.Value = bodyItem.Station;
                            stationExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            stationExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var modelNameExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            modelNameExcelRange.Value = bodyItem.ModelName;
                            modelNameExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            modelNameExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var modelNOExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            modelNOExcelRange.Value = bodyItem.ModelNO;
                            modelNOExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            modelNOExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var articleNOExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            articleNOExcelRange.Value = bodyItem.ArticleNO;
                            articleNOExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            articleNOExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var supplierExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            supplierExcelRange.Value = bodyItem.Supplier;
                            supplierExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            supplierExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var glueNameExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            glueNameExcelRange.Value = bodyItem.GlueName;
                            glueNameExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            glueNameExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var SMTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            SMTExcelRange.Value = bodyItem.StartMixingTime == null || !bodyItem.GlueName.Contains(" + ") ? "N/A" : bodyItem.StartMixingTime.Value.ToString("HH:mm");
                            SMTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            SMTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var FMTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            FMTExcelRange.Value = bodyItem.FinishMixingTime == null || !bodyItem.GlueName.Contains(" + ") ? "N/A" : bodyItem.FinishMixingTime.Value.ToString("HH:mm");
                            FMTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            FMTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));
                            /// . If the SST, FST is manual, Stir CT and AVG. RPM should show N/A
                            /// . If the mixed Con. is >=  1 kg, and the worker didn't use the stir function, SST and FST should show Error.
                            /// If the glue no need to mix, the Stir CT and AVG. RPM should show N/A.

                            var SSTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            if (bodyItem.GlueName.Contains(" + ") && bodyItem.MixedConsumption < 1)
                            {
                                SSTExcelRange.Value = "Manual";

                            }
                            else if (bodyItem.GlueName.Contains(" + ")
                              && bodyItem.StartStirTime != null
                              && bodyItem.FinishStirTime != null
                              && bodyItem.MixedConsumption >= 1)
                            {
                                SSTExcelRange.Value = bodyItem.StartStirTime.Value.ToString("HH:mm");
                            }
                            else if (bodyItem.GlueName.Contains(" + ")
                             && bodyItem.StartStirTime == null
                             && bodyItem.FinishStirTime == null
                             && bodyItem.MixedConsumption >= 1)
                            {
                                SSTExcelRange.Value = "Error";
                            }
                            else
                            {
                                SSTExcelRange.Value = "N/A";
                            }
                            SSTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            SSTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var SCTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];

                            if (bodyItem.GlueName.Contains(" + ")
                          && bodyItem.StartStirTime != null
                          && bodyItem.FinishStirTime != null
                          && bodyItem.MixedConsumption >= 1)
                            {
                                SCTExcelRange.Value = bodyItem.StirCicleTime;
                            }
                            else
                            {
                                SCTExcelRange.Value = "N/A";

                            }
                            SCTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            SCTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var AVGRPMExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            if (bodyItem.GlueName.Contains(" + ")
                          && bodyItem.StartStirTime != null
                          && bodyItem.FinishStirTime != null
                          && bodyItem.MixedConsumption >= 1)
                            {
                                AVGRPMExcelRange.Value = bodyItem.AverageRPM;
                            }
                            else
                            {
                                AVGRPMExcelRange.Value = "N/A";

                            }
                            AVGRPMExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            AVGRPMExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var FSTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            if (bodyItem.GlueName.Contains(" + ") && bodyItem.MixedConsumption < 1)
                            {
                                FSTExcelRange.Value = "Manual";

                            }
                            else if (bodyItem.GlueName.Contains(" + ")
                              && bodyItem.StartStirTime != null
                              && bodyItem.FinishStirTime != null
                              && bodyItem.MixedConsumption >= 1)
                            {
                                FSTExcelRange.Value = bodyItem.FinishStirTime.Value.ToString("HH:mm");
                            }
                            else if (bodyItem.GlueName.Contains(" + ")
                             && bodyItem.StartStirTime == null
                             && bodyItem.FinishStirTime == null
                             && bodyItem.MixedConsumption >= 1)
                            {
                                FSTExcelRange.Value = "Error";
                            }
                            else
                            {
                                FSTExcelRange.Value = "N/A";
                            }
                            FSTExcelRange.Value = bodyItem.FinishStirTime == null ? "N/A" : bodyItem.FinishStirTime.Value.ToString("HH:mm");
                            FSTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            FSTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var PTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            PTExcelRange.Value = bodyItem.PrintTime == null ? "N/A" : bodyItem.PrintTime.Value.ToString("HH:mm");
                            PTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            PTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var SDTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            SDTExcelRange.Value = bodyItem.StartDispatchingTime == null ? "N/A" : bodyItem.StartDispatchingTime.Value.ToString("HH:mm");
                            SDTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            SDTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var FDTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            FDTExcelRange.Value = bodyItem.FinishDispatchingTime == null ? "N/A" : bodyItem.FinishDispatchingTime.Value.ToString("HH:mm");
                            FDTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            FDTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var StdConExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            StdConExcelRange.Value = $"{Math.Round(bodyItem.StandardConsumption, 2)}kg";
                            StdConExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            StdConExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var MixedConExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            MixedConExcelRange.Value = !bodyItem.GlueName.Contains(" + ") ? "N/A" : $"{Math.Round(bodyItem.MixedConsumption, 2)}kg";
                            MixedConExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            MixedConExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var deliverdConExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            deliverdConExcelRange.Value = $"{Math.Round(bodyItem.DeliveredConsumption, 2)}kg";
                            deliverdConExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            deliverdConExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var statusRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            statusRange.Value = bodyItem.Status;
                            statusRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            statusRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));
                            statusRange.Style.Font.Color.SetColor(bodyItem.Status == "Pass" ? ColorTranslator.FromHtml("#28a745") : ColorTranslator.FromHtml("#a30000")); // #a30000
                            var ESTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            ESTExcelRange.Value = bodyItem.EstimatedStartTime == null ? "N/A" : bodyItem.EstimatedStartTime.ToString("HH:mm");
                            ESTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ESTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                            var EFTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                            EFTExcelRange.Value = bodyItem.EstimatedFinishTime == null ? "N/A" : bodyItem.EstimatedFinishTime.ToString("HH:mm");
                            EFTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            EFTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));
                        } //#BDD7EE

                        //int mergeFromColIndex = 1;
                        //int mergeFromRowIndex = 2;
                        //int mergeToRowIndex = 1;
                        //foreach (var item in model.GroupBy(x => x.Sequence))
                        //{
                        //    mergeToRowIndex += item.Count();
                        //    var sequenceExcelRange = ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeFromColIndex];
                        //    sequenceExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //    sequenceExcelRange.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#fff"));

                        //    sequenceExcelRange.Merge = true;
                        //    sequenceExcelRange.Style.Font.Size = 20;
                        //    sequenceExcelRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        //    sequenceExcelRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        //    sequenceExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //    sequenceExcelRange.Style.Fill.BackgroundColor.SetColor(item.Key % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));
                        //    mergeFromRowIndex = mergeToRowIndex + 1;
                        //}

                        // merge Supplier, Glue, SMT, FMT, SST, SitrCT , StirAVG, FST, PT, SDT, FDT, Std.Con, MixedCon, Satus, EST, EFT
                        //int mergeFromColIndex = 1;
                        int mergeFromRowIndex = 2;
                        int mergeToRowIndex = 1;
                        int sequence = 1, supplier = 7, glue = 8, SMT = 9, FMT = 10, SST = 11, sitrCT = 12, stirAVG = 13, FST = 14, PT = 15, SDT = 16, FDT = 17, stdCon = 18, mixedCon = 19, status = 21, EST = 22, EFT = 23;

                        var colList = new List<int>
                    {
                    sequence,supplier , glue , SMT, FMT , SST , sitrCT, stirAVG , FST, PT , SDT , FDT , stdCon, mixedCon, status , EST , EFT
                    };
                        foreach (var item in model.GroupBy(x => x.Sequence))
                        {
                            mergeToRowIndex += item.Count();
                            foreach (var colItem in colList)
                            {
                                MergeRowAndCol(ws, item.Key, mergeFromRowIndex, colItem, mergeToRowIndex);
                            }
                            mergeFromRowIndex = mergeToRowIndex + 1;
                        }

                        //Make all text fit the cells
                        for (int i = 1; i <= bodyRowIndex; i++)
                        {
                            for (int j = 1; j <= bodyColIndex - 1; j++)
                            {
                                ws.Cells[i, j].Style.Font.Bold = true;
                                ws.Cells[i, j].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[i, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                ws.Cells[i, j].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                ws.Cells[i, j].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                ws.Cells[i, j].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                ws.Cells[i, j].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            }

                        }

                        for (int i = 2; i <= 7; i++)
                        {
                            for (int j = 3; j <= bodyColIndex + 5; j++)
                            {
                                ws.Cells[i, j].Style.Font.Bold = true;
                                ws.Cells[i, j].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[i, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            }

                        }

                        var rowIndex = 2;
                        var colIndex = 25;
                        var analyzeTitleExcelRange = ws.Cells[1, colIndex];
                        analyzeTitleExcelRange.Reset();
                        analyzeTitleExcelRange.Value = "Analyze";

                        analyzeTitleExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        analyzeTitleExcelRange.Style.Font.Size = 16;
                        analyzeTitleExcelRange.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#fff"));
                        analyzeTitleExcelRange.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#1F4E78"));
                        foreach (var analyzeRow in analyzeList)
                        {
                            rowIndex++;
                            colIndex = 25;
                            foreach (var item in analyzeRow)
                            {
                                var cells = ws.Cells[rowIndex, colIndex++];
                                cells.Reset();
                                cells.Value = item;
                                cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            }
                        }


                        ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    }
                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    return bin;
                }
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                Console.WriteLine(mes);
                return new Byte[] { };
            }
        }

        public Byte[] ExcelExportReportOfDoneList(List<ToDoListForExportDto> model)
        {
            try
            {
                var currentTime = DateTime.Now.ToLocalTime();
                var delayTotal = model.Where(x => x.EstimatedFinishTime < currentTime
                    && x.FinishDispatchingTime == null).Count();
                var total = model.Count();
                var doneTotal = model.Where(x => x.FinishDispatchingTime != null && x.Status == "Pass").Count();
                var percentageOfDelay = Math.Round(((double)delayTotal / total) * 100, 0);
                var percentageOfDone = Math.Round(((double)doneTotal / total) * 100, 0);
                var stirErrorTotal = model.Where(x => x.GlueName.Contains("+") && x.StartStirTime == null && x.MixedConsumption >= 1).Count();
                var statusFailTotal = model.Where(x => x.FinishDispatchingTime != null && x.Status == "Fail").GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime, x.GlueName }).Count();
                var percentageOfStirError = Math.Round(((double)stirErrorTotal / total) * 100, 0);
                var percentageOfStatusFail = Math.Round(((double)statusFailTotal / total) * 100, 0);
                var analyzeHeader = new List<string>
                    {
                        "Total",
                        total.ToString(),
                        "Rate",
                        "Root Cause",
                        "Action Plan"
                    };
                var doneRow = new List<string>
                    {
                        "Done",
                        doneTotal.ToString(),
                        doneTotal == 0? "0%" : percentageOfDone + "%",
                        "-",
                        "-"
                    };
                var delayRow = new List<string>
                    {
                        "Delay",
                        delayTotal.ToString(),
                        delayTotal == 0? "0%" : percentageOfDelay + "%",
                        delayTotal == 0? "-" : "Didn't implement the task before estimated finished time.",
                        delayTotal == 0? "-" : "Please Implement the task in advance."
                    };
                var stirErrorRow = new List<string>
                    {
                        "Stir Error",
                        stirErrorTotal.ToString(),
                        Double.IsNaN(percentageOfStirError) ? "0%" : percentageOfStirError + "%",
                       stirErrorTotal == 0? "-" : "Didn't implement the task before estimated finished time.",
                        stirErrorTotal == 0? "-" :"Please Implement the task in advance."
                    };
                var statusFailRow = new List<string>
                    {
                        "Status Fail",
                        statusFailTotal.ToString(),
                        Double.IsNaN(percentageOfStatusFail) ? "0%" : percentageOfStatusFail + "%",
                        statusFailTotal == 0? "-" :"Finished implementing the task later than estimated finished time.",
                        statusFailTotal == 0? "-" :"Please implement the task before estimated finished time."
                    };
                var analyzeList = new List<List<string>> { analyzeHeader, doneRow, delayRow, stirErrorRow, statusFailRow };
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var memoryStream = new MemoryStream();
                using (ExcelPackage p = new ExcelPackage(memoryStream))
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "Henry Pham";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "Done List";
                    //Tạo một sheet để làm việc trên đó
                    p.Workbook.Worksheets.Add("Done List");

                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = p.Workbook.Worksheets["Done List"];

                    // đặt tên cho sheet
                    ws.Name = "Done List";
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 11;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";

                    var headerArray = new List<string>()
                    {
                        "Sequence",
                        "Line",
                        "Station",
                        "Model Name",
                        "Model NO",
                        "Article NO",
                        "Supplier",
                        "Glue",
                        "SMT",
                        "FMT",
                        "SST",
                        "Stir CT",
                        "AVG. RPM",
                        "FST",
                        "PT",
                        "SDT",
                        "FDT",
                        "Std. Con. (kg)", //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                        "Mixed Con. (kg)", //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                        "Delivered Con. (kg)", //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                        "Status",
                        "EST",
                        "EFT",
                    };
                    int headerRowIndex = 1;
                    foreach (var headerItem in headerArray.Select((value, i) => new { i, value }))
                    {
                        var headerColIndex = headerItem.i + 1;
                        var headerExcelRange = ws.Cells[headerRowIndex, headerColIndex];
                        headerExcelRange.Value = headerItem.value;
                        headerExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        headerExcelRange.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#1F4E78"));
                        headerExcelRange.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#fff"));
                        headerExcelRange.Style.Font.Size = 16;

                    }

                    int bodyRowIndex = 1;
                    int bodyColIndex = 1;

                    foreach (var bodyItem in model)
                    {
                        bodyColIndex = 1;
                        bodyRowIndex++;

                        var sequenceExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        sequenceExcelRange.Value = bodyItem.Sequence;
                        sequenceExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sequenceExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var lineExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        lineExcelRange.Value = bodyItem.Line;
                        lineExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        lineExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var stationExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        stationExcelRange.Value = bodyItem.Station;
                        stationExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        stationExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var modelNameExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        modelNameExcelRange.Value = bodyItem.ModelName;
                        modelNameExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        modelNameExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var modelNOExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        modelNOExcelRange.Value = bodyItem.ModelNO;
                        modelNOExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        modelNOExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var articleNOExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        articleNOExcelRange.Value = bodyItem.ArticleNO;
                        articleNOExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        articleNOExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var supplierExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        supplierExcelRange.Value = bodyItem.Supplier;
                        supplierExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        supplierExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var glueNameExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        glueNameExcelRange.Value = bodyItem.GlueName;
                        glueNameExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        glueNameExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var SMTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null) // Leo Update FinishDispatchingTime -> PrintTime
                        {
                            SMTExcelRange.Value = "-";
                        }
                        else
                        {
                            SMTExcelRange.Value = bodyItem.StartMixingTime == null || !bodyItem.GlueName.Contains(" + ") ? "N/A" : bodyItem.StartMixingTime.Value.ToString("HH:mm");
                        }
                        SMTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        SMTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var FMTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null) // Leo Update FinishDispatchingTime -> PrintTime
                        {
                            FMTExcelRange.Value = "-";
                        }
                        else
                        {
                            FMTExcelRange.Value = bodyItem.FinishMixingTime == null || !bodyItem.GlueName.Contains(" + ") ? "N/A" : bodyItem.FinishMixingTime.Value.ToString("HH:mm");
                        }
                        FMTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        FMTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var SSTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null) // Leo Update FinishDispatchingTime -> PrintTime
                        {
                            SSTExcelRange.Value = "-";
                        }
                        else
                        {
                            if (bodyItem.GlueName.Contains(" + ") && bodyItem.MixedConsumption < 1)
                            {
                                SSTExcelRange.Value = "Manual";

                            }
                            else if (bodyItem.GlueName.Contains(" + ")
                              && bodyItem.StartStirTime != null
                              && bodyItem.FinishStirTime != null
                              && bodyItem.MixedConsumption >= 1)
                            {
                                SSTExcelRange.Value = bodyItem.StartStirTime.Value.ToString("HH:mm");
                            }
                            else if (bodyItem.GlueName.Contains(" + ")
                             && bodyItem.StartStirTime == null
                             && bodyItem.FinishStirTime == null
                             && bodyItem.MixedConsumption >= 1)
                            {
                                SSTExcelRange.Value = "Error";
                            }
                            else
                            {
                                SSTExcelRange.Value = "N/A";
                            }
                        }
                        SSTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        SSTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var SCTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null) // Leo Update FinishDispatchingTime -> PrintTime
                        {
                            SCTExcelRange.Value = "-";
                        }
                        else
                        {
                            if (bodyItem.GlueName.Contains(" + ")
                          && bodyItem.StartStirTime != null
                          && bodyItem.FinishStirTime != null
                          && bodyItem.MixedConsumption >= 1)
                            {
                                SCTExcelRange.Value = bodyItem.StirCicleTime;
                            }
                            else
                            {
                                SCTExcelRange.Value = "N/A";

                            }
                        }
                        SCTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        SCTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var AVGRPMExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null) // Leo Update FinishDispatchingTime -> PrintTime
                        {
                            AVGRPMExcelRange.Value = "-";
                        }
                        else
                        {
                            if (bodyItem.GlueName.Contains(" + ")
                          && bodyItem.StartStirTime != null
                          && bodyItem.FinishStirTime != null
                          && bodyItem.MixedConsumption >= 1)
                            {
                                AVGRPMExcelRange.Value = bodyItem.AverageRPM;
                            }
                            else
                            {
                                AVGRPMExcelRange.Value = "N/A";

                            }
                        }
                        AVGRPMExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        AVGRPMExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var FSTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null) // Leo Update FinishDispatchingTime -> PrintTime
                        {
                            FSTExcelRange.Value = "-";
                        }
                        else
                        {
                            if (bodyItem.GlueName.Contains(" + ") && bodyItem.MixedConsumption < 1)
                            {
                                FSTExcelRange.Value = "Manual";

                            }
                            else if (bodyItem.GlueName.Contains(" + ")
                              && bodyItem.StartStirTime != null
                              && bodyItem.FinishStirTime != null
                              && bodyItem.MixedConsumption >= 1)
                            {
                                FSTExcelRange.Value = bodyItem.FinishStirTime.Value.ToString("HH:mm");
                            }
                            else if (bodyItem.GlueName.Contains(" + ")
                             && bodyItem.StartStirTime == null
                             && bodyItem.FinishStirTime == null
                             && bodyItem.MixedConsumption >= 1)
                            {
                                FSTExcelRange.Value = "Error";
                            }
                            else
                            {
                                FSTExcelRange.Value = "N/A";
                            }
                        }
                        FSTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        FSTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var PTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null) // Leo Update FinishDispatchingTime -> PrintTime
                        {
                            PTExcelRange.Value = "-";
                        }
                        else
                        {
                            PTExcelRange.Value = bodyItem.PrintTime == null ? "N/A" : bodyItem.PrintTime.Value.ToString("HH:mm");
                        }
                        PTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        PTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var SDTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null) // Leo Update FinishDispatchingTime -> PrintTime
                        {
                            SDTExcelRange.Value = "-";
                        }
                        else
                        {
                            SDTExcelRange.Value = bodyItem.StartDispatchingTime == null ? "N/A" : bodyItem.PrintTime.Value.ToString("HH:mm"); // Leo Update FinishDispatchingTime -> PrintTime
                        }
                        SDTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        SDTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var FDTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null) // Leo Update FinishDispatchingTime -> PrintTime
                        {
                            FDTExcelRange.Value = "-";
                        }
                        else
                        {
                            FDTExcelRange.Value = bodyItem.PrintTime == null ? "N/A" : bodyItem.PrintTime.Value.ToString("HH:mm"); // Leo Update FinishDispatchingTime -> PrintTime
                        }
                        FDTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        FDTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var StdConExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null) // Leo Update FinishDispatchingTime -> PrintTime
                        {
                            StdConExcelRange.Value = "-";
                        }
                        else
                        {
                            StdConExcelRange.Value = $"{Math.Round(bodyItem.StandardConsumption, 2)}"; // Leo Update 
                        }
                        StdConExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        StdConExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var MixedConExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null) // Leo Update FinishDispatchingTime -> PrintTime
                        {
                            MixedConExcelRange.Value = "-";
                        }
                        else
                        {
                            MixedConExcelRange.Value = !bodyItem.GlueName.Contains(" + ") ? "N/A" : $"{Math.Round(bodyItem.MixedConsumption, 2)}"; // Leo Update
                        }
                        MixedConExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        MixedConExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var deliverdConExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null) // Leo Update FinishDispatchingTime -> PrintTime
                        {
                            deliverdConExcelRange.Value = "-";
                        }
                        else
                        {
                            deliverdConExcelRange.Value = $"{Math.Round(bodyItem.DeliveredConsumptionEachLine, 2)}"; // Leo Update
                        }
                        deliverdConExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        deliverdConExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var statusRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        if (bodyItem.EstimatedFinishTime < currentTime && bodyItem.PrintTime == null) // Leo Update FinishDispatchingTime -> PrintTime
                        {
                            statusRange.Value = "-";
                        }
                        else
                        {
                            statusRange.Value = bodyItem.Status;
                            statusRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            statusRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));
                            statusRange.Style.Font.Color.SetColor(bodyItem.Status == "Pass" ? ColorTranslator.FromHtml("#28a745") : ColorTranslator.FromHtml("#a30000")); // #a30000
                        }

                        var ESTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        ESTExcelRange.Value = bodyItem.EstimatedStartTime == null ? "N/A" : bodyItem.EstimatedStartTime.ToString("HH:mm");
                        ESTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ESTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var EFTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        EFTExcelRange.Value = bodyItem.EstimatedFinishTime == null ? "N/A" : bodyItem.EstimatedFinishTime.ToString("HH:mm");
                        EFTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        EFTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));
                    } //#BDD7EE


                    // merge Supplier, Glue, SMT, FMT, SST, SitrCT , StirAVG, FST, PT, SDT, FDT, Std.Con, MixedCon, Satus, EST, EFT
                    //int mergeFromColIndex = 1;
                    int mergeFromRowIndex = 2;
                    int mergeToRowIndex = 1;
                    int sequence = 1, supplier = 7, glue = 8, SMT = 9, FMT = 10, SST = 11, sitrCT = 12, stirAVG = 13, FST = 14, PT = 15, SDT = 16, FDT = 17, stdCon = 18, mixedCon = 19, status = 21, EST = 22, EFT = 23;

                    var colList = new List<int>
                    {
                    sequence,supplier , glue , SMT, FMT , SST , sitrCT, stirAVG , FST, PT , SDT , FDT , stdCon, mixedCon, status , EST , EFT
                    };
                    foreach (var item in model.GroupBy(x => x.Sequence))
                    {
                        mergeToRowIndex += item.Count();
                        foreach (var colItem in colList)
                        {
                            MergeRowAndCol(ws, item.Key, mergeFromRowIndex, colItem, mergeToRowIndex);
                        }
                        mergeFromRowIndex = mergeToRowIndex + 1;
                    }

                    // analyze






                    //Make all text fit the cells
                    //ws.Cells[ws.Dimension.Address].AutoFitColumns();


                    //make the borders of cell F6 thick
                    for (int i = 1; i <= bodyRowIndex; i++)
                    {
                        for (int j = 1; j <= bodyColIndex - 1; j++)
                        {
                            ws.Cells[i, j].Style.Font.Bold = true;
                            ws.Cells[i, j].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            ws.Cells[i, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[i, j].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            ws.Cells[i, j].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            ws.Cells[i, j].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            ws.Cells[i, j].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        }

                    }

                    for (int i = 2; i <= 7; i++)
                    {
                        for (int j = 3; j <= bodyColIndex + 5; j++)
                        {
                            ws.Cells[i, j].Style.Font.Bold = true;
                            ws.Cells[i, j].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            ws.Cells[i, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        }

                    }

                    var rowIndex = 2;
                    var colIndex = 25;
                    var analyzeTitleExcelRange = ws.Cells[1, colIndex];
                    analyzeTitleExcelRange.Reset();
                    analyzeTitleExcelRange.Value = "Analyze";

                    analyzeTitleExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    analyzeTitleExcelRange.Style.Font.Size = 16;
                    analyzeTitleExcelRange.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#fff"));
                    analyzeTitleExcelRange.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#1F4E78"));
                    foreach (var analyzeRow in analyzeList)
                    {
                        rowIndex++;
                        colIndex = 25;
                        foreach (var item in analyzeRow)
                        {
                            var cells = ws.Cells[rowIndex, colIndex++];
                            cells.Reset();
                            cells.Value = item;
                            cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        }
                    }


                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    return bin;
                }
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                Console.WriteLine(mes);
                return new Byte[] { };
            }
        }

        void MergeRowAndCol(ExcelWorksheet ws, int index, int mergeFromRowIndex, int mergeFromColIndex, int mergeToRowIndex)
        {
            var sequenceExcelRange = ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeFromColIndex];
            sequenceExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            sequenceExcelRange.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#fff"));

            sequenceExcelRange.Merge = true;
            int squenceColIndex = 1;
            if (mergeFromColIndex == squenceColIndex)
            {
                sequenceExcelRange.Style.Font.Size = 20;
            }
            sequenceExcelRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sequenceExcelRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sequenceExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            sequenceExcelRange.Style.Fill.BackgroundColor.SetColor(index % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));
        }
        public async Task<byte[]> ExportExcelNewReportOfDonelistByBuilding(int buildingID)
        {
            var currentTime = DateTime.Now.ToLocalTime();
            var currentDate = currentTime.Date;

            var doneTodolistModel = await _repoToDoList.FindAll(x =>
                   x.IsDelete == false
                   && x.EstimatedStartTime.Date == currentDate
                   && x.EstimatedFinishTime.Date == currentDate
                   && x.PrintTime != null
                   && x.BuildingID == buildingID
                   )
                .Include(x => x.Plan)
                    .ThenInclude(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelName)
                .Include(x => x.Plan)
                    .ThenInclude(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelNo)
                 .Include(x => x.Plan)
                    .ThenInclude(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArticleNo)
                .Include(x => x.Plan)
                    .ThenInclude(x => x.Stations)
                .Select(x => new
                {
                    x.Plan,
                    x.EstimatedFinishTime,
                    x.EstimatedStartTime,
                    x.FinishDispatchingTime,
                    x.StartDispatchingTime,
                    x.FinishStirTime,
                    x.StartStirTime,
                    x.StartMixingTime,
                    x.FinishMixingTime,
                    x.PrintTime,
                    x.MixingInfoID,
                    x.MixedConsumption,
                    x.DeliveredConsumption,
                    x.StandardConsumption,
                    x.LineName,
                    x.LineID,
                    x.Supplier,
                    x.GlueID,
                    x.GlueName,
                    x.Status,
                    Station = x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID) == null ? 0 : x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID).Amount,
                    ModelName = x.Plan.BPFCEstablish.ModelName == null ? "N/A" : x.Plan.BPFCEstablish.ModelName.Name,
                    ModelNO = x.Plan.BPFCEstablish.ModelNo == null ? "N/A" : x.Plan.BPFCEstablish.ModelNo.Name,
                    ArticleNO = x.Plan.BPFCEstablish.ArticleNo == null ? "N/A" : x.Plan.BPFCEstablish.ArticleNo.Name,
                })
               .ToListAsync();


            var delayTodolistModel = await _repoToDoList.FindAll(x =>
                  x.IsDelete == false
                  && x.EstimatedStartTime.Date == currentDate
                  && x.EstimatedFinishTime.Date == currentDate
                  && x.EstimatedFinishTime < currentTime
                  && x.PrintTime == null //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                  && x.BuildingID == buildingID
                  )
               .Include(x => x.Plan)
                   .ThenInclude(x => x.BPFCEstablish)
                   .ThenInclude(x => x.ModelName)
               .Include(x => x.Plan)
                   .ThenInclude(x => x.BPFCEstablish)
                   .ThenInclude(x => x.ModelNo)
                .Include(x => x.Plan)
                   .ThenInclude(x => x.BPFCEstablish)
                   .ThenInclude(x => x.ArticleNo)
               .Include(x => x.Plan)
                   .ThenInclude(x => x.Stations)
               .Select(x => new
               {
                   x.Plan,
                   x.EstimatedFinishTime,
                   x.EstimatedStartTime,
                   x.FinishDispatchingTime,
                   x.StartDispatchingTime,
                   x.FinishStirTime,
                   x.StartStirTime,
                   x.StartMixingTime,
                   x.FinishMixingTime,
                   x.PrintTime,
                   x.MixingInfoID,
                   x.MixedConsumption,
                   x.DeliveredConsumption,
                   x.StandardConsumption,
                   x.LineName,
                   x.LineID,
                   x.Supplier,
                   x.GlueID,
                   x.GlueName,
                   x.Status,
                   Station = x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID) == null ? 0 : x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID).Amount,
                   ModelName = x.Plan.BPFCEstablish.ModelName == null ? "N/A" : x.Plan.BPFCEstablish.ModelName.Name,
                   ModelNO = x.Plan.BPFCEstablish.ModelNo == null ? "N/A" : x.Plan.BPFCEstablish.ModelNo.Name,
                   ArticleNO = x.Plan.BPFCEstablish.ArticleNo == null ? "N/A" : x.Plan.BPFCEstablish.ArticleNo.Name,
               }).Where(x => x.GlueName.Contains(" + ")) //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
              .ToListAsync();
            var exportList = new List<ToDoListForExportDto>();
            var groupBy = doneTodolistModel.Concat(delayTodolistModel).GroupBy(x => new { x.EstimatedStartTime, x.GlueName })
                .OrderBy(x => x.Key.EstimatedStartTime)
                .ThenBy(x => x.Key.GlueName)
                .ToList();

            foreach (var groupByItem in groupBy.Select((value, i) => new { i, value }))
            {
                var sequence = groupByItem.i + 1;
                foreach (var item in groupByItem.value.OrderBy(x => x.LineName).ToList())
                {
                    var stirModel = await _repoStir.FindAll(a => a.MixingInfoID == item.MixingInfoID).OrderBy(x => x.CreatedTime).ToListAsync();
                    var dispatchModel = await _repoDispatch.FindAll(a => a.MixingInfoID == item.MixingInfoID && a.LineID == item.LineID)
                        .FirstOrDefaultAsync();

                    var averageRPM = item.MixingInfoID == 0 || stirModel.Count == 0 ? 0 : stirModel.Average(x => x.RPM);
                    var stirCicleTime = item.MixingInfoID == 0 || stirModel.Count == 0 ? 0 : stirModel.Sum(x => x.ActualDuration);
                    var exportItem = new ToDoListForExportDto();
                    exportItem.Sequence = sequence;
                    exportItem.Line = item.LineName;
                    exportItem.Station = item.Station;
                    exportItem.Supplier = item.Supplier;
                    var modelName = item.Plan.BPFCEstablish.ModelName;
                    var modelNO = item.Plan.BPFCEstablish.ModelName;
                    var articleNO = item.Plan.BPFCEstablish.ModelName;

                    exportItem.ModelName = item.ModelName;
                    exportItem.ModelNO = item.ModelNO;
                    exportItem.ArticleNO = item.ArticleNO;
                    exportItem.GlueName = item.GlueName;

                    exportItem.StartMixingTime = item.StartMixingTime;
                    exportItem.FinishMixingTime = item.FinishMixingTime;

                    exportItem.StartStirTime = item.StartStirTime;
                    exportItem.FinishStirTime = item.FinishStirTime;


                    exportItem.PrintTime = item.PrintTime;
                    exportItem.StirCicleTime = stirCicleTime;
                    exportItem.AverageRPM = averageRPM;

                    exportItem.StartDispatchingTime = item.StartDispatchingTime;
                    exportItem.FinishDispatchingTime = item.FinishDispatchingTime;

                    exportItem.StandardConsumption = item.StandardConsumption;

                    exportItem.MixedConsumption = item.MixedConsumption;
                    exportItem.DeliveredConsumption = item.DeliveredConsumption;
                    exportItem.DeliveredConsumptionEachLine = dispatchModel is null ? 0 : dispatchModel.Amount;
                    var status = item.PrintTime.HasValue == false ? false : item.PrintTime.Value.ToRemoveSecond() <= item.EstimatedFinishTime;
                    exportItem.Status = status ? "Pass" : "Fail";

                    exportItem.EstimatedStartTime = item.EstimatedStartTime;
                    exportItem.EstimatedFinishTime = item.EstimatedFinishTime;
                    exportList.Add(exportItem);
                }
            }
            var dispatchListModel = await _repoDispatchList.FindAll(x =>
                  x.IsDelete == false
                  && x.EstimatedStartTime.Date == currentDate
                  && x.EstimatedFinishTime.Date == currentDate
                  && x.FinishDispatchingTime != null
                  && x.BuildingID == buildingID
                  )
               .Include(x => x.Plan)
                   .ThenInclude(x => x.BPFCEstablish)
                   .ThenInclude(x => x.ModelName)
               .Include(x => x.Plan)
                   .ThenInclude(x => x.BPFCEstablish)
                   .ThenInclude(x => x.ModelNo)
                .Include(x => x.Plan)
                   .ThenInclude(x => x.BPFCEstablish)
                   .ThenInclude(x => x.ArticleNo)
               .Include(x => x.Plan)
                   .ThenInclude(x => x.Stations)
               .Select(x => new
               {
                   x.Plan,
                   x.EstimatedFinishTime,
                   x.EstimatedStartTime,
                   x.FinishDispatchingTime,
                   x.StartDispatchingTime,
                   x.PrintTime,
                   x.MixingInfoID,
                   x.DeliveredAmount,
                   x.LineName,
                   x.LineID,
                   x.Supplier,
                   x.GlueID,
                   x.GlueName,
                   x.Status,
                   Station = x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID) == null ? 0 : x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID).Amount,
                   ModelName = x.Plan.BPFCEstablish.ModelName == null ? "N/A" : x.Plan.BPFCEstablish.ModelName.Name,
                   ModelNO = x.Plan.BPFCEstablish.ModelNo == null ? "N/A" : x.Plan.BPFCEstablish.ModelNo.Name,
                   ArticleNO = x.Plan.BPFCEstablish.ArticleNo == null ? "N/A" : x.Plan.BPFCEstablish.ArticleNo.Name,
               })
              .ToListAsync();
            var delayDispatchListModel = await _repoDispatchList.FindAll(x =>
                 x.IsDelete == false
                 && x.EstimatedStartTime.Date == currentDate
                 && x.EstimatedFinishTime.Date == currentDate
                 && x.EstimatedFinishTime < currentTime
                 && x.FinishDispatchingTime == null //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
                 && x.BuildingID == buildingID
                 )
              .Include(x => x.Plan)
                  .ThenInclude(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ModelName)
              .Include(x => x.Plan)
                  .ThenInclude(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ModelNo)
               .Include(x => x.Plan)
                  .ThenInclude(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ArticleNo)
              .Include(x => x.Plan)
                  .ThenInclude(x => x.Stations)
              .Select(x => new
              {
                  x.Plan,
                  x.EstimatedFinishTime,
                  x.EstimatedStartTime,
                  x.FinishDispatchingTime,
                  x.StartDispatchingTime,
                  x.PrintTime,
                  x.MixingInfoID,
                  x.DeliveredAmount,
                  x.LineName,
                  x.LineID,
                  x.Supplier,
                  x.GlueID,
                  x.GlueName,
                  x.Status,
                  Station = x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID) == null ? 0 : x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID).Amount,
                  ModelName = x.Plan.BPFCEstablish.ModelName == null ? "N/A" : x.Plan.BPFCEstablish.ModelName.Name,
                  ModelNO = x.Plan.BPFCEstablish.ModelNo == null ? "N/A" : x.Plan.BPFCEstablish.ModelNo.Name,
                  ArticleNO = x.Plan.BPFCEstablish.ArticleNo == null ? "N/A" : x.Plan.BPFCEstablish.ArticleNo.Name,
              })
             .ToListAsync();
            var groupByDispatchList = dispatchListModel.Concat(delayDispatchListModel).GroupBy(x => new { x.EstimatedStartTime, x.GlueName })
              .OrderBy(x => x.Key.EstimatedStartTime)
              .ThenBy(x => x.Key.GlueName)
              .ToList();
            foreach (var groupByItem in groupByDispatchList.Select((value, i) => new { i, value }))
            {
                var sequence = groupByItem.i + 1;
                foreach (var item in groupByItem.value.OrderBy(x => x.LineName).ToList())
                {
                    var dispatchModel = await _repoDispatch.FindAll(a => a.MixingInfoID == item.MixingInfoID && a.LineID == item.LineID)
                        .FirstOrDefaultAsync();
                    var exportItem = new ToDoListForExportDto();
                    exportItem.Sequence = sequence;
                    exportItem.Line = item.LineName;
                    exportItem.Station = item.Station;
                    exportItem.Supplier = item.Supplier;
                    var modelName = item.Plan.BPFCEstablish.ModelName;
                    var modelNO = item.Plan.BPFCEstablish.ModelName;
                    var articleNO = item.Plan.BPFCEstablish.ModelName;

                    exportItem.ModelName = item.ModelName;
                    exportItem.ModelNO = item.ModelNO;
                    exportItem.ArticleNO = item.ArticleNO;
                    exportItem.GlueName = item.GlueName;

                    exportItem.StartDispatchingTime = item.StartDispatchingTime;
                    exportItem.FinishDispatchingTime = item.FinishDispatchingTime;

                    exportItem.DeliveredConsumption = item.DeliveredAmount;
                    exportItem.DeliveredConsumptionEachLine = dispatchModel is null ? 0 : dispatchModel.Amount;
                    var status = item.FinishDispatchingTime.HasValue == false ? false : item.FinishDispatchingTime.Value.ToRemoveSecond() <= item.EstimatedFinishTime;
                    exportItem.Status = status ? "Pass" : "Fail";

                    exportItem.EstimatedStartTime = item.EstimatedStartTime;
                    exportItem.EstimatedFinishTime = item.EstimatedFinishTime;
                    exportList.Add(exportItem);
                }
            }
            return ExcelExportReportOfDoneList(exportList);
        }
        #endregion

        #region SendMail Action

        public async Task<bool> SendMail(byte[] data, DateTime time)
        {
            var file = data;
            var subject = "Mixing Room Report";
            var fileName = $"mixingRoomReport{time.ToString("MMddyyyy")}.xlsx";
            var message = "Please refer to the Mixing Room Report";
            var mailList = new List<string>
            {
                //"sy.pham@shc.ssbshoes.com",
                "mel.kuo@shc.ssbshoes.com",
                "maithoa.tran@shc.ssbshoes.com",
                "andy.wu@shc.ssbshoes.com",
                "sin.chen@shc.ssbshoes.com",
                "leo.doan@shc.ssbshoes.com",
                "heidy.amos@shc.ssbshoes.com",
                "bonding.team@shc.ssbshoes.com",
                "Ian.Ho@shc.ssbshoes.com",
                "swook.lu@shc.ssbshoes.com",
                "damaris.li@shc.ssbshoes.com",
                "peter.tran@shc.ssbshoes.com"
            };
            try
            {
                await _emailService.SendEmailWithAttactExcelFileAsync(mailList, subject, message, fileName, file);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CheckBuildingType(List<int> plans)
        {
            var plansModel = await _repoPlan.FindAll(x => plans.Contains(x.ID))
               .Include(x => x.Building)
               .FirstOrDefaultAsync();
            return plansModel.Building.KindID > 0;
        }

        #endregion

    }
}
