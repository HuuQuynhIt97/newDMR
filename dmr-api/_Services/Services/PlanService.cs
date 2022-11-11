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
using System.Collections.Immutable;
using DMR_API.SignalrHub;
using Microsoft.AspNetCore.SignalR;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Style;
using System.Drawing;
using Microsoft.AspNetCore.Http;
using dmr_api.Models;

namespace DMR_API._Services.Services
{
    public class PlanService : IPlanService
    {
        #region Constructor

        private readonly int LINE_LEVEL = 3;
        private readonly IPlanRepository _repoPlan;
        private readonly IPlanDetailRepository _repoPlanDetail;
        private readonly IGlueRepository _repoGlue;
        private readonly IGlueIngredientRepository _repoGlueIngredient;
        private readonly IIngredientRepository _repoIngredient;
        private readonly IIngredientInfoRepository _repoIngredientInfo;

        private readonly IBuildingRepository _repoBuilding;
        private readonly IBPFCEstablishRepository _repoBPFC;
        private readonly IMixingInfoRepository _repoMixingInfo;
        private readonly IDispatchRepository _repoDispatch;
        private readonly IModelNameRepository _repoModelName;
        private readonly IHubContext<ECHub> _hubContext;
        private readonly IJWTService _jwtService;
        private readonly IHttpContextAccessor _accessor;
        private readonly IToDoListService _toDoListService;
        private readonly IStationService _stationService;
        private readonly IStationRepository _repoStation;
        private readonly IToDoListRepository _repoToDoList;
        private readonly IPeriodRepository _repoPeriod;
        private readonly IDispatchListRepository _repoDispatchList;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public PlanService(
            IPlanRepository repoPlan,
            IDispatchRepository repoDispatch,
            IPlanDetailRepository repoPlanDetail,
            IGlueRepository repoGlue,
            IGlueIngredientRepository repoGlueIngredient,
            IIngredientRepository repoIngredient,
            IBuildingRepository repoBuilding,
            IBPFCEstablishRepository repoBPFC,
            IIngredientInfoRepository repoIngredientInfo,
            IMixingInfoRepository repoMixingInfo,
            IModelNameRepository repoModelName,
            IToDoListService toDoListService,
            IStationService stationService,
            IStationRepository repoStation,
            IToDoListRepository repoToDoList,
            IPeriodRepository repoPeriod,
            IDispatchListRepository repoDispatchList,
            IHubContext<ECHub> hubContext,
            IJWTService jwtService,
            IHttpContextAccessor accessor,
            IMapper mapper,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoGlue = repoGlue;
            _repoGlueIngredient = repoGlueIngredient;
            _repoIngredient = repoIngredient;
            _repoIngredientInfo = repoIngredientInfo;
            _repoPlan = repoPlan;
            _repoPlanDetail = repoPlanDetail;
            _repoBuilding = repoBuilding;
            _repoModelName = repoModelName;
            _hubContext = hubContext;
            _jwtService = jwtService;
            _accessor = accessor;
            _repoBPFC = repoBPFC;
            _repoMixingInfo = repoMixingInfo;
            _repoDispatch = repoDispatch;
            _toDoListService = toDoListService;
            _stationService = stationService;
            _repoStation = repoStation;
            _repoToDoList = repoToDoList;
            _repoPeriod = repoPeriod;
            _repoDispatchList = repoDispatchList;
        }

        #endregion

        #region LoadData
        //Lấy toàn bộ danh sách Plan 
        public async Task<List<PlanDto>> GetAllAsync()
        {
            var min = DateTime.Now.Date;
            var max = DateTime.Now.AddDays(15).Date;
            var r = await _repoPlan.FindAll()
                .Include(x => x.Building)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.Glues)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelName)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelNo)
                .Include(x => x.BPFCEstablish)
                     .ThenInclude(x => x.ArticleNo)
                 .Include(x => x.BPFCEstablish)
                     .ThenInclude(x => x.ArtProcess)
                     .ThenInclude(x => x.Process)
                .Where(x => !x.BPFCEstablish.IsDelete)
                .ProjectTo<PlanDto>(_configMapper)
                .OrderByDescending(x => x.ID)
                .ToListAsync();
            return r;
        }

        public async Task<object> GetAllPlanByRange(int building, DateTime min, DateTime max)
        {
            var lines = new List<int>();
            if (building == 0 || building == 1)
            {
                lines = await _repoBuilding.FindAll(x => x.Level == 3).Select(x => x.ID).ToListAsync();
            }
            else
            {
                lines = await _repoBuilding.FindAll(x => x.ParentID == building).Select(x => x.ID).ToListAsync();
            }
            // sua ngay 3/15/2021 2:28pm
            //return await _repoPlan.FindAll()
            //    .Where(x => x.DueDate.Date >= min.Date && x.DueDate.Date <= max.Date && lines.Contains(x.BuildingID))
            //    .Include(x => x.Building)
            //        .ThenInclude(x => x.LunchTime)
            //        .ThenInclude(x => x.Periods)
            //    .Include(x => x.ToDoList)
            //    .Include(x => x.BPFCEstablish)
            //        .ThenInclude(x => x.Glues)
            //    .Include(x => x.BPFCEstablish)
            //        .ThenInclude(x => x.ModelName)
            //    .Include(x => x.BPFCEstablish)
            //        .ThenInclude(x => x.ModelNo)
            //    .Include(x => x.BPFCEstablish)
            //        .ThenInclude(x => x.ArticleNo)
            //    .Include(x => x.BPFCEstablish)
            //        .ThenInclude(x => x.ArtProcess)
            //        .ThenInclude(x => x.Process)
            //    .ProjectTo<PlanDto>(_configMapper)
            //    .OrderByDescending(x => x.BuildingName)
            //    .ToListAsync();
            // sua ngay 3/15/2021 2:28pm
            return await _repoPlan.FindAll()
              .Where(x => x.DueDate.Date >= min.Date && x.DueDate.Date <= max.Date && lines.Contains(x.BuildingID))
              .Include(x => x.Building)
                  .ThenInclude(x => x.PeriodMixingList)
                     .Include(x => x.Building)
                  .ThenInclude(x => x.Kind)
              .Include(x => x.ToDoList)
              .Include(x => x.BPFCEstablish)
                  .ThenInclude(x => x.Glues)
                  .ThenInclude(x => x.Kind)
              .Include(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ModelName)
              .Include(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ModelNo)
              .Include(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ArticleNo)
              .Include(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ArtProcess)
                  .ThenInclude(x => x.Process)
              .ProjectTo<PlanDto>(_configMapper)
              .OrderByDescending(x => x.BuildingName)
              .ToListAsync();
        }

        //Lấy danh sách Plan và phân trang
        public async Task<PagedList<PlanDto>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoPlan.FindAll().ProjectTo<PlanDto>(_configMapper).OrderByDescending(x => x.ID);
            return await PagedList<PlanDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }

        public async Task<List<PlanDto>> GetGlueByBuildingBPFCID(int buildingID, int bpfcID)
        {
            var lists = await _repoGlue.FindAll().Where(x => x.isShow == true && x.BPFCEstablishID == bpfcID).ProjectTo<PlanDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
            return lists.ToList();
        }

        public async Task<object> GetAllPlanByDefaultRange()
        {
            var min = DateTime.Now.Date;
            var max = DateTime.Now.AddDays(15).Date;
            return await _repoPlan.FindAll()
                .Where(x => x.DueDate.Date >= min && x.DueDate <= max)
                .Include(x => x.Building)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.Glues.Where(x => x.isShow == true))
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ModelName)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ModelNo)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ArticleNo)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ArtProcess)
                .ThenInclude(x => x.Process)
                .ProjectTo<PlanDto>(_configMapper)
                .OrderByDescending(x => x.ID)
                .ToListAsync();
        }

        // tooltip data o trang todolist
        public async Task<object> GetBPFCByGlue(TooltipParams tooltip)
        {
            var name = tooltip.Glue.Trim().ToSafetyString();
            var results = new List<string>();
            var plans = await _repoPlan.FindAll()
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
                                .Where(x => x.DueDate.Date == DateTime.Now.Date && !x.BPFCEstablish.IsDelete).ToListAsync();
            foreach (var plan in plans)
            {
                foreach (var glue in plan.BPFCEstablish.Glues.Where(x => x.isShow == true && x.Name.Trim().Equals(name)))
                {
                    var bpfc = $"{plan.BPFCEstablish.ModelName.Name} -> {plan.BPFCEstablish.ModelNo.Name} -> {plan.BPFCEstablish.ArticleNo.Name} -> {plan.BPFCEstablish.ArtProcess.Process.Name}";
                    results.Add(bpfc);
                }
            }
            return results.Distinct();
        }

        public Task<object> GetAllPlansByDate(string from, string to)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<GlueCreateDto1>> GetGlueByBuilding(int buildingID)
        {
            var item = _repoBuilding.FindById(buildingID);
            var lineList = await _repoBuilding.FindAll().Where(x => x.ParentID == item.ID).Select(x => x.ID).ToListAsync();
            List<int> modelNameID = _repoPlan.FindAll().Where(x => lineList.Contains(x.BuildingID)).Select(x => x.BPFCEstablishID).ToList();
            var lists = await _repoGlue.FindAll().Where(x => x.isShow == true).Where(x => x.isShow == true).ProjectTo<GlueCreateDto1>(_configMapper).Where(x => modelNameID.Contains(x.BPFCEstablishID)).OrderByDescending(x => x.ID).Select(x => new GlueCreateDto1
            {
                ID = x.ID,
                Name = x.Name,
                GlueID = x.GlueID,
                Code = x.Code,
                ModelNo = x.ModelNo,
                CreatedDate = x.CreatedDate,
                BPFCEstablishID = x.BPFCEstablishID,
                PartName = x.PartName,
                PartID = x.PartID,
                MaterialID = x.MaterialID,
                MaterialName = x.MaterialName,
                Consumption = x.Consumption,
                Chemical = new GlueDto1 { ID = x.GlueID, Name = x.Name }
            }).ToListAsync();
            return lists.DistinctBy(x => x.Name).ToList();
        }

        public async Task<List<GlueCreateDto1>> GetGlueByBuildingModelName(int buildingID, int bpfc)
        {
            var item = _repoBuilding.FindById(buildingID);
            var lineList = await _repoBuilding.FindAll().Where(x => x.ParentID == item.ID).Select(x => x.ID).ToListAsync();
            List<int> modelNameID = _repoPlan.FindAll().Where(x => lineList.Contains(x.BuildingID)).Select(x => x.BPFCEstablishID).ToList();
            var lists = await _repoGlue.FindAll().Where(x => x.isShow == true).ProjectTo<GlueCreateDto1>(_configMapper).Where(x => x.BPFCEstablishID == bpfc).OrderByDescending(x => x.ID).Select(x => new GlueCreateDto1
            {
                ID = x.ID,
                Name = x.Name,
                GlueID = x.GlueID,
                Code = x.Code,
                ModelNo = x.ModelNo,
                CreatedDate = x.CreatedDate,
                BPFCEstablishID = x.BPFCEstablishID,
                PartName = x.PartName,
                PartID = x.PartID,
                MaterialID = x.MaterialID,
                MaterialName = x.MaterialName,
                Consumption = x.Consumption,
                Chemical = new GlueDto1 { ID = x.GlueID, Name = x.Name }
            }).ToListAsync();
            return lists.DistinctBy(x => x.Name).ToList();
        }

        public async Task<object> GetLines(int buildingID)
        {
            var item = _repoBuilding.FindById(buildingID);
            if (item == null) return new List<BuildingDto>();
            if (item.Level == 2)
            {
                var lineList = _repoBuilding.FindAll().Include(x => x.Kind).Where(x => x.ParentID == item.ID);
                return await lineList.ProjectTo<BuildingDto>(_configMapper).ToListAsync();
            }
            return new List<BuildingDto>();
        }

        public PlanDto FindByID(int ID)
        {

            return _repoPlan.FindAll()
                .Where(x => x.ID == ID)
                .Include(x => x.Building)
                .Include(x => x.ToDoList)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelName)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelNo)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArticleNo)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArtProcess)
                    .ThenInclude(x => x.Process)
                .ProjectTo<PlanDto>(_configMapper)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefault();
        }

        public async Task<int?> FindBuildingByLine(int lineID)
        {
            var model = await _repoBuilding.FindAll(x => x.ID == lineID).FirstOrDefaultAsync();
            return model.ParentID;
        }

        // Lay thoi gian bat dau de tao task
        public async Task<ResponseDetail<PeriodMixing>> GetStartTimeFromPeriod(int buildingID)
        {

            var model = await _repoBuilding.FindAll(x => x.ID == buildingID).Include(x => x.PeriodMixingList).FirstOrDefaultAsync();

            var period = model.PeriodMixingList.OrderBy(x => x.StartTime.TimeOfDay).FirstOrDefault();

            if (period == null)
            {
                return new ResponseDetail<PeriodMixing>()
                {
                    Data = null,
                    Status = false,
                    Message = $"Vui lòng cập nhật period cho buiding {model.Name}!"
                };
            }
            return new ResponseDetail<PeriodMixing>()
            {
                Data = period,
                Status = true,
            };
        }

        // Lay thong tin cua glue khi in
        public async Task<MixingInfo> Print(DispatchParams todolistDto)
        {
            var mixingInfo = await _repoMixingInfo
            .FindAll(x => x.EstimatedTime == todolistDto.EstimatedTime && x.GlueName.Equals(todolistDto.Glue))
            .Include(x => x.Glue)
                .ThenInclude(x => x.GlueIngredients)
                .ThenInclude(x => x.Ingredient)
            .FirstOrDefaultAsync();
            return mixingInfo == null ? new MixingInfo() : mixingInfo;
        }

        //Lấy Plan theo Plan_Id
        public PlanDto GetById(object id)
        {
            return _mapper.Map<Plan, PlanDto>(_repoPlan.FindById(id));
        }

        //Tìm kiếm Plan
        public Task<PagedList<PlanDto>> Search(PaginationParams param, object text)
        {
            throw new System.NotImplementedException();

        }

        public Task<object> Summary(int building)
        {
            throw new System.NotImplementedException();
        }

        public async Task<object> TodolistUndone()
        {
            var currentTime = DateTime.Now;
            var currentDate = DateTime.Now.Date;
            var startLunchTime = new TimeSpan(12, 30, 0);
            var endLunchTime = new TimeSpan(13, 30, 0);

            var model = from b in _repoBPFC.FindAll(x => !x.IsDelete)
                        join p in _repoPlan.GetAll()
                                            .Include(x => x.Building)
                                            .Where(x => x.DueDate == currentDate)
                                            .Select(x => new
                                            {
                                                x.WorkingHour,
                                                x.ID,
                                                x.BPFCEstablishID,
                                                Building = x.Building.Name,
                                                x.HourlyOutput
                                            })
                        on b.ID equals p.BPFCEstablishID
                        join g in _repoGlue.FindAll(x => x.isShow)
                                           .Include(x => x.MixingInfos)
                                           .Include(x => x.GlueIngredients)
                                               .ThenInclude(x => x.Ingredient)
                                               .ThenInclude(x => x.Supplier)
                                            .Select(x => new
                                            {
                                                x.ID,
                                                x.BPFCEstablishID,
                                                x.Name,
                                                x.Consumption,
                                                MixingInfos = x.MixingInfos.Select(x => new MixingInfoTodolistDto { ID = x.ID, Status = x.Status, EstimatedStartTime = x.StartTime, EstimatedFinishTime = x.EndTime }).ToList(),
                                                Ingredients = x.GlueIngredients.Select(x => new { x.Position, x.Ingredient.PrepareTime, Supplier = x.Ingredient.Supplier.Name, x.Ingredient.ReplacementFrequency })
                                            })
                       on b.ID equals g.BPFCEstablishID
                        select new
                        {
                            GlueID = g.ID,
                            GlueName = g.Name,
                            g.Ingredients,
                            g.MixingInfos,
                            Plans = p,
                            p.HourlyOutput,
                            p.WorkingHour,
                            g.Consumption,
                            Line = p.Building,
                        };
            var test = await model.ToListAsync();
            var groupByGlueName = test.GroupBy(x => x.GlueName);
            var result = new List<TodolistDto>();
            foreach (var glue in groupByGlueName)
            {

                foreach (var item in glue)
                {

                    foreach (var mixingInfo in item.MixingInfos)
                    {
                        var itemTodolist = new TodolistDto();
                        double standardConsumption = 0;
                        var supplier = string.Empty;
                        var checmicalA = item.Ingredients.ToList().FirstOrDefault(x => x.Position == "A");
                        double prepareTime = 0;
                        if (checmicalA != null)
                        {
                            supplier = checmicalA.Supplier;
                            prepareTime = checmicalA.PrepareTime;
                        }
                        itemTodolist.Supplier = supplier;
                        itemTodolist.ID = item.GlueID;
                        itemTodolist.MixingInfoTodolistDtos = item.MixingInfos;
                        itemTodolist.Lines = glue.Select(x => x.Line).ToList();
                        itemTodolist.Glue = glue.Key;
                        itemTodolist.DeliveredActual = "-";
                        itemTodolist.Status = mixingInfo.Status;
                        itemTodolist.EstimatedFinishTime = mixingInfo.EstimatedFinishTime;
                        itemTodolist.EstimatedStartTime = mixingInfo.EstimatedStartTime;
                        itemTodolist.EstimatedTime = currentDate.AddHours(7).AddMinutes(30) - TimeSpan.FromHours(prepareTime);
                        foreach (var line in glue)
                        {
                            var kgPair = line.Consumption.ToDouble() / 1000;
                            standardConsumption += kgPair * (double)line.HourlyOutput * checmicalA.ReplacementFrequency;
                        }

                        itemTodolist.StandardConsumption = Math.Round(standardConsumption, 2);
                        result.Add(itemTodolist);
                    }
                }
            }
            return result;
        }

        // k su dung
        public async Task<object> Dispatch(DispatchParams todolistDto)
        {
            var currentDate = DateTime.Now.Date;
            var lines = await _repoBuilding.FindAll(x => todolistDto.Lines.Contains(x.Name)).Select(x => x.ID).ToListAsync();
            var dispatches = await _repoDispatch
                .FindAll(x => x.CreatedTime.Date == DateTime.Now.Date && lines.Contains(x.LineID))
                .ToListAsync();
            var mixingInfo = await _repoMixingInfo
            .FindAll(x => x.EstimatedStartTime == todolistDto.EstimatedStartTime && x.EstimatedFinishTime == todolistDto.EstimatedFinishTime && x.GlueName.Equals(todolistDto.Glue))
            .Include(x => x.Glue)
                .ThenInclude(x => x.GlueIngredients)
                .ThenInclude(x => x.Ingredient)
            .FirstOrDefaultAsync();
            if (mixingInfo == null) return new List<DispatchTodolistDto>();
            var glue = mixingInfo.Glue;
            var list = new List<DispatchTodolistDto>();
            var plans = _repoPlan.FindAll(x => x.DueDate.Date == currentDate)
                .Include(x => x.Building)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueIngredients)
                    .ThenInclude(x => x.Ingredient)
                .Where(x => !x.BPFCEstablish.IsDelete)
                .Where(x => x.BPFCEstablishID == glue.BPFCEstablishID || todolistDto.Lines.Contains(x.Building.Name))
                .ToList();
            if (plans.Count == 0) return new List<DispatchTodolistDto>();

            foreach (var plan in plans)
            {
                var item = new DispatchTodolistDto();
                item.Line = plan.Building.Name;
                item.LineID = plan.Building.ID;
                double standardConsumption = 0;

                foreach (var glueItem in plan.BPFCEstablish.Glues.Where(x => x.isShow && x.Name.Equals(mixingInfo.GlueName)))
                {
                    item.MixingInfoID = mixingInfo.ID;
                    item.EstimatedTime = mixingInfo.EstimatedTime;
                    item.Glue = glue.Name;
                    var checmicalA = glueItem.GlueIngredients.ToList().FirstOrDefault(x => x.Position == "A");
                    var replacementFrequency = checmicalA != null ? checmicalA.Ingredient.ReplacementFrequency : 0;
                    var kgPair = checmicalA != null ? glueItem.Consumption.ToDouble() / 1000 : 0;
                    standardConsumption = kgPair * (double)plan.HourlyOutput * replacementFrequency; // 5kg
                }
                if (standardConsumption > 3)
                {
                    while (standardConsumption > 0)
                    {
                        standardConsumption = standardConsumption - 3; // 2
                        if (standardConsumption > 3)
                        {
                            item.StandardAmount = 3;
                            list.Add(item);

                        }
                        else if (standardConsumption > 0 && standardConsumption < 3)
                        {
                            item.StandardAmount = Math.Round(standardConsumption, 2);

                            list.Add(item);

                        }
                        else
                        {
                            item.StandardAmount = 3;
                            list.Add(item);
                        }
                    }
                }
                else
                {
                    item.StandardAmount = Math.Round(standardConsumption, 2);

                    list.Add(item);

                }
            }
            var result = (from a in list
                          from b in dispatches.Where(x => a.LineID == x.LineID && x.MixingInfoID == a.MixingInfoID)
                         .DefaultIfEmpty()
                          select new DispatchTodolistDto
                          {
                              ID = b == null ? 0 : b.ID,
                              Glue = a.Glue,
                              Line = a.Line,
                              LineID = a.LineID,
                              MixingInfoID = a.MixingInfoID,
                              Real = b == null ? 0 : b.Amount * 1000,
                          });
            return result.OrderBy(x => x.Line).ToList();
            throw new NotImplementedException();
        }

        public async Task<MixingInfo> FindMixingInfo(string glue, DateTime estimatedTime)
        {
            var mixingInfo = await _repoMixingInfo
            .FindAll(x => x.EstimatedTime == estimatedTime && x.GlueName == glue).Include(x => x.MixingInfoDetails).FirstOrDefaultAsync();
            return mixingInfo;
        }

        public async Task<string> FindDeliver(string glue, DateTime estimatedTime)
        {
            var mixingInfo = await FindMixingInfo(glue, estimatedTime);
            if (mixingInfo == null) return "0kg/0kg";
            var buildingGlue = await _repoDispatch.FindAll(x => x.MixingInfoID == mixingInfo.ID).Select(x => x.Amount).ToListAsync();
            var deliver = buildingGlue.Sum();
            return $"{Math.Round(deliver / 1000, 2)}kg/{Math.Round(CalculateGlueTotal(mixingInfo), 2)}";
        }

        #endregion

        #region Helper
        private double CalculateIngredientByPositon(List<GlueIngredient> glueIngredients, IngredientReportDto ingredient, double quantity)
        {
            var count = glueIngredients.Count;
            switch (count)
            {
                case 1: return CalculateA(glueIngredients, ingredient, quantity);
                case 2: return CalculateAB(glueIngredients, ingredient, quantity);
                case 3: return CalculateABC(glueIngredients, ingredient, quantity);
                case 4: return CalculateABCD(glueIngredients, ingredient, quantity);
                case 5: return CalculateABCDE(glueIngredients, ingredient, quantity);
                default:
                    return 0;
            }
        }
        private double CalculateA(List<GlueIngredient> glueIngredients, IngredientReportDto ingredient, double quantity)
        {
            var valueA = quantity;
            return valueA;

        }
        private double CalculateAB(List<GlueIngredient> glueIngredients, IngredientReportDto ingredient, double quantity)
        {
            var glueIngredient = glueIngredients.FirstOrDefault(x => x.IngredientID == ingredient.ID);
            var position = glueIngredient.Position;

            double percentageB = 1 + ((double)FindPercentageByPosition(glueIngredients, "B") / 100);
            double valueA = quantity / percentageB;
            double valueB = quantity - valueA;

            switch (position)
            {
                case "A":
                    return valueA;
                case "B":
                    return valueB;
                default:
                    return 0;
            }
        }
        private double CalculateABC(List<GlueIngredient> glueIngredients, IngredientReportDto ingredient, double quantity)
        {
            var glueIngredient = glueIngredients.FirstOrDefault(x => x.IngredientID == ingredient.ID);
            var position = glueIngredient.Position;
            var percentageB = 1 + ((double)FindPercentageByPosition(glueIngredients, "B") / 100);
            var percentageC = 1 + ((double)FindPercentageByPosition(glueIngredients, "C") / 100);

            var valueB = quantity - (quantity / percentageB);
            var valueC = quantity - valueB - (valueB / percentageB);

            var valueA = quantity - valueB - valueC;
            switch (position)
            {
                case "A": return valueA;
                case "B": return valueB;
                case "C": return valueC;
                default:
                    return 0;
            }

        }
        private double CalculateABCD(List<GlueIngredient> glueIngredients, IngredientReportDto ingredient, double quantity)
        {
            var glueIngredient = glueIngredients.FirstOrDefault(x => x.IngredientID == ingredient.ID);
            var position = glueIngredient.Position;
            var percentageB = 1 + ((double)FindPercentageByPosition(glueIngredients, "B") / 100);
            var percentageC = 1 + ((double)FindPercentageByPosition(glueIngredients, "C") / 100);
            var percentageD = 1 + ((double)FindPercentageByPosition(glueIngredients, "D") / 100);
            var valueB = quantity - (quantity / percentageB);
            var valueC = quantity - valueB - (valueB / percentageB);
            var valueD = quantity - valueB - valueC - (valueC / percentageB);
            var valueA = quantity - valueB - valueC - valueD;
            switch (position)
            {

                case "A": return valueA;
                case "B": return valueB;
                case "C": return valueC;
                case "D": return valueD;
                default:
                    return 0;
            }

        }
        private double CalculateABCDE(List<GlueIngredient> glueIngredients, IngredientReportDto ingredient, double quantity)
        {
            var glueIngredient = glueIngredients.FirstOrDefault(x => x.IngredientID == ingredient.ID);
            var position = glueIngredient.Position;
            var percentageB = 1 + ((double)FindPercentageByPosition(glueIngredients, "B") / 100);
            var percentageC = 1 + ((double)FindPercentageByPosition(glueIngredients, "C") / 100);
            var percentageD = 1 + ((double)FindPercentageByPosition(glueIngredients, "D") / 100);
            var percentageE = 1 + ((double)FindPercentageByPosition(glueIngredients, "E") / 100);
            var valueB = quantity - (quantity / percentageB);
            var valueC = quantity - valueB - (valueB / percentageB);
            var valueD = quantity - valueB - valueC - (valueC / percentageB);
            var valueE = quantity - valueB - valueC - -valueD - (valueC / percentageB);
            var valueA = quantity - valueB - valueC - valueD - valueE;
            switch (position)
            {
                case "A": return valueA;
                case "B": return valueB;
                case "C": return valueC;
                case "D": return valueD;
                case "E": return valueE;
                default:
                    return 0;
            }

        }

        #endregion

        #region Action
        public async Task<bool> Add(PlanDto model)
        {
            var userID = _jwtService.GetUserID();
            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var plan = _mapper.Map<Plan>(model);
                    var checkExist = await _repoPlan.FindAll().OrderByDescending(x=> x.CreatedDate).FirstOrDefaultAsync(x => x.BuildingID == model.BuildingID && x.DueDate.Date == model.DueDate.Date);
                    // Neu ton tai thi kiem tra xem co phai la ngung chuyen khong
                    if (checkExist != null) {
                        if (checkExist.IsOffline) {
                            plan.StartWorkingTime = DateTime.Now.ToRemoveSecond();
                        } else { // Khong phai la ngung chuyen thi thong bao da ton tại
                            return false;
                        }
                    }
                    DateTime dt = DateTime.Now.ToLocalTime().ToRemoveSecond();
                    plan.CreatedDate = dt;
                    plan.CreateBy = userID;
                    plan.BPFCEstablishID = model.BPFCEstablishID;
                    _repoPlan.Add(plan);
                    await _repoPlan.SaveAll();
                    //var stationModel = await _stationService.GetAllByPlanID(plan.ID);
                    //await _stationService.AddRange(stationModel);
                    transaction.Complete();
                    await _hubContext.Clients.All.SendAsync("summaryRecieve", "ok");
                    return true;
                }
                catch
                {
                    transaction.Dispose();
                    return false;
                }
            }
        }

        //Cập nhật Plan
        public async Task<bool> Update(PlanDto model)
        {
            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var planItem = await _repoPlan.FindAll(x => x.ID == model.ID).FirstOrDefaultAsync();
                    if (planItem is null) return false;
                    string token = _accessor.HttpContext.Request.Headers["Authorization"];
                    var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
                    var plan = _mapper.Map<Plan>(model);
                    var oldPlan = _repoPlan.FindAll(x => x.ID == model.ID).AsNoTracking().FirstOrDefault();
                    planItem.BuildingID = model.BuildingID;
                    planItem.BPFCEstablishID = model.BPFCEstablishID;
                    planItem.StartWorkingTime = model.StartWorkingTime;
                    planItem.FinishWorkingTime = model.FinishWorkingTime;
                    planItem.HourlyOutput = model.HourlyOutput;
                    planItem.DueDate = model.DueDate;
                    planItem.ModifyTime = DateTime.Now;
                    planItem.CreateBy = userID;
                    _repoPlan.Update(planItem);
                    await _repoPlan.SaveAll();

                    // Nếu cập nhật lại finishworkingtime thì xóa những cái sau thời gian cập nhật ở bảng todolist và dispatch
                    if (model.FinishWorkingTime.ToRemoveSecond() != oldPlan.FinishWorkingTime.ToRemoveSecond())
                    {
                        var timeOfDay = model.FinishWorkingTime.ToRemoveSecond().TimeOfDay;
                        var todoDelete = await _repoToDoList.FindAll(x => x.EstimatedStartTime.TimeOfDay >= timeOfDay && x.PlanID == oldPlan.ID && x.IsDelete == false).ToListAsync();
                        todoDelete.ForEach(item =>
                        {
                            item.IsDelete = true;
                        });
                        var todoShow = await _repoToDoList.FindAll(x => x.EstimatedStartTime.TimeOfDay < timeOfDay && x.PlanID == oldPlan.ID && x.IsDelete).ToListAsync();
                        todoShow.ForEach(item =>
                        {
                            item.IsDelete = false;
                        });
                        var todo = todoShow.Concat(todoDelete).DistinctBy(x => x.ID).ToList();
                        if (todo.Count > 0)
                        {
                            _repoToDoList.UpdateRange(todo);
                            await _repoToDoList.SaveAll();
                        }


                        // 11:00 >= 10:50 && 10:30 > 10:50
                        var deletingList = await _repoDispatchList.FindAll(x => x.EstimatedStartTime.TimeOfDay >= timeOfDay && x.EstimatedFinishTime.TimeOfDay > timeOfDay && x.PlanID == oldPlan.ID && x.IsDelete == false).ToListAsync();
                        deletingList.ForEach(item =>
                        {
                            item.IsDelete = true;
                        });
                        var showList = await _repoDispatchList.FindAll(x => x.EstimatedStartTime.TimeOfDay < timeOfDay && x.EstimatedFinishTime.TimeOfDay <= timeOfDay && x.PlanID == oldPlan.ID && x.IsDelete).ToListAsync();
                        showList.ForEach(item =>
                        {
                            item.IsDelete = false;
                        });
                        var dispatching = showList.Concat(deletingList).DistinctBy(x => x.ID).ToList();
                        if (dispatching.Count > 0)
                        {
                            _repoDispatchList.UpdateRange(dispatching);
                            await _repoDispatchList.SaveAll();
                        }

                    }
                    transaction.Complete();
                    await _hubContext.Clients.All.SendAsync("summaryRecieve", "ok");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    transaction.Dispose();
                    return false;
                    throw;
                }
            }

        }

        //Xóa Plan
        public async Task<bool> Delete(object id)
        {
            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var model = _repoPlan.FindById(id);
                    var finishWorkingTime = model.FinishWorkingTime.ToRemoveSecond();
                    _repoPlan.Remove(model);
                    await _repoPlan.SaveAll();

                    //await _hubContext.Clients.All.SendAsync("summaryRecieve", "ok");
                    // loai bo cai vua xoa sap xep tg moi cho den cu -> lay cai bi thay doi trk do -> cap nhat lai finsishWorkingTime
                    var oldPlan = await _repoPlan.FindAll(x => x.ID != model.ID && x.BuildingID == model.BuildingID && x.DueDate == model.DueDate).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                    // Neu xoa thi cap nhat lai cai vua thay doi
                    if (oldPlan != null)
                    {
                        var timeOfDay = finishWorkingTime.TimeOfDay;
                        oldPlan.FinishWorkingTime = finishWorkingTime;
                        oldPlan.IsChangeBPFC = false;
                        oldPlan.IsOvertime = false;
                        _repoPlan.Update(oldPlan);
                        await _repoPlan.SaveAll();
                        var todoShow = await _repoToDoList.FindAll(x => x.EstimatedStartTime.TimeOfDay < timeOfDay && x.PlanID == oldPlan.ID && x.IsDelete).ToListAsync();
                        todoShow.ForEach(item =>
                        {
                            item.IsDelete = false;
                        });
                        if (todoShow.Count() > 0)
                        {
                            _repoToDoList.UpdateRange(todoShow);
                            await _repoToDoList.SaveAll();
                        }

                        var showList = await _repoDispatchList.FindAll(x => x.EstimatedStartTime.TimeOfDay < timeOfDay && x.EstimatedFinishTime.TimeOfDay <= timeOfDay && x.PlanID == oldPlan.ID && x.IsDelete).ToListAsync();
                        showList.ForEach(item =>
                        {
                            item.IsDelete = false;
                        });
                        if (showList.Count() > 0)
                        {
                            _repoDispatchList.UpdateRange(showList);
                            await _repoDispatchList.SaveAll();
                        }
                    }
                    transaction.Complete();
                    return true;
                }
                catch
                {
                    transaction.Dispose();
                    return false;
                }
            }

        }

        public async Task<object> DeleteRange(List<int> plansDto)
        {
            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var plans = await _repoPlan.FindAll().Where(x => plansDto.Contains(x.ID)).ToListAsync();
                    foreach (var item in plans)
                    {
                        var finishWorkingTime = item.FinishWorkingTime.ToRemoveSecond();
                        _repoPlan.Remove(item);
                        await _repoPlan.SaveAll();

                        //await _hubContext.Clients.All.SendAsync("summaryRecieve", "ok");
                        // loai bo cai vua xoa sap xep tg moi cho den cu -> lay cai bi thay doi trk do -> cap nhat lai finsishWorkingTime
                        var oldPlan = await _repoPlan.FindAll(x => x.ID != item.ID && x.BuildingID == item.BuildingID && x.DueDate.Date == item.DueDate.Date).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                        // Neu xoa thi cap nhat lai cai vua thay doi
                        if (oldPlan != null)
                        {
                            var timeOfDay = finishWorkingTime.TimeOfDay;
                            oldPlan.FinishWorkingTime = finishWorkingTime;
                            oldPlan.IsChangeBPFC = false;
                            oldPlan.IsOvertime = item.IsOvertime;
                            _repoPlan.Update(oldPlan);
                            await _repoPlan.SaveAll();
                            var todoShow = await _repoToDoList.FindAll(x => x.EstimatedStartTime.TimeOfDay < timeOfDay && x.PlanID == oldPlan.ID && x.IsDelete).ToListAsync();
                            todoShow.ForEach(item =>
                            {
                                item.IsDelete = false;
                            });
                            if (todoShow.Count() > 0)
                            {
                                _repoToDoList.UpdateRange(todoShow);
                                await _repoToDoList.SaveAll();
                            }

                            var showList = await _repoDispatchList.FindAll(x => x.EstimatedStartTime.TimeOfDay < timeOfDay && x.EstimatedFinishTime.TimeOfDay <= timeOfDay && x.PlanID == oldPlan.ID && x.IsDelete).ToListAsync();
                            showList.ForEach(item =>
                            {
                                item.IsDelete = false;
                            });
                            if (showList.Count() > 0)
                            {
                                _repoDispatchList.UpdateRange(showList);
                                await _repoDispatchList.SaveAll();
                            }
                        }
                    }

                    transaction.Complete();
                    return true;
                }
                catch
                {
                    transaction.Dispose();
                    return false;
                }
            }


        }

        public async Task<bool> DeletePlan(int id)
        {
            try
            {
                var userID = _jwtService.GetUserID();

                var model = _repoPlan.FindById(id);
                if (model is null) return false;
                model.IsDelete = true;
                model.DeleteBy = userID;
                model.DeleteTime = DateTime.Now;
                _repoPlan.Update(model);
                return await _repoPlan.SaveAll();
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteRangePlan(List<int> plans)
        {

            var flag = new List<bool>();
            foreach (var id in plans)
            {
                try
                {
                    var model = _repoPlan.FindById(id);
                    if (model is null)
                    {
                        flag.Add(false);
                    }
                    else
                    {
                        var userID = _jwtService.GetUserID();
                        model.IsDelete = true;
                        model.DeleteBy = userID;
                        model.DeleteTime = DateTime.Now;
                        _repoPlan.Update(model);
                        _repoPlan.Save();
                    }
                    flag.Add(true);
                }
                catch
                {
                    flag.Add(false);
                }
            }
            return flag.All(x => x == true);
        }

        public async Task<bool> EditQuantity(int id, int qty)
        {
            try
            {
                var item = _repoPlan.FindById(id);
                item.Quantity = qty;
                return await _repoPlan.SaveAll();
            }
            catch
            {
                return false;
            }
        }

        public async Task<object> Finish(int mixingÌnoID)
        {
            var mixingInfo = await _repoMixingInfo
            .FindAll(x => x.ID == mixingÌnoID).FirstOrDefaultAsync();
            if (mixingInfo == null) return false;
            mixingInfo.Status = true;
            return await _repoMixingInfo.SaveAll();
        }

        public async Task<bool> CheckExistTimeRange(int lineID, DateTime statTime, DateTime endTime, DateTime dueDate)
        {
            var item = await _repoPlan.FindAll(x =>
                                 x.BuildingID == lineID
                              && x.DueDate.Date == dueDate.Date
                              ).FirstOrDefaultAsync();
            if (item == null) return false;
            // Neu ton tai thi return ve true
            //oldtatTime 9:30 - 14:00 ==> newstatTime 12:00 - 16: 30
            if (statTime >= item.FinishWorkingTime)
            {
                return false;
            }
            return true;
        }

        public async Task<object> ClonePlan(List<PlanForCloneDto> plansDto)
        {
            var plans = _mapper.Map<List<Plan>>(plansDto);
            var flag = new List<bool>();
            try
            {
                foreach (var item in plans)
                {
                    var checkExist = await _repoPlan.FindAll().AllAsync(x => x.BuildingID == item.BuildingID && x.BPFCEstablishID == item.BPFCEstablishID && x.DueDate.Date == item.DueDate.Date);
                    if (!checkExist)
                    {
                        //var todolist = _repoToDoList.FindAll(x => x.PlanID == item.ID).ToList();

                        using var scope = new TransactionScopeAsync().Create();
                        {
                            try
                            {
                                item.ID = 0;
                                item.DueDate = item.DueDate.Date;
                                item.StartWorkingTime = new DateTime(item.DueDate.Year, item.DueDate.Month, item.DueDate.Day, 7, 00, 00);
                                item.FinishWorkingTime = new DateTime(item.DueDate.Year, item.DueDate.Month, item.DueDate.Day, 16, 30, 00);
                                _repoPlan.Add(item);
                                await _repoPlan.SaveAll();

                                var stationModel = await _stationService.GetAllByPlanID(item.ID);
                                await _stationService.AddRange(stationModel);
                                //todolist.ForEach(todo =>
                                //{
                                //    todo.ID = 0;
                                //    todo.PlanID = item.ID;
                                //    var startTime = new TimeSpan(todo.EstimatedStartTime.Hour, todo.EstimatedStartTime.Minute, todo.EstimatedStartTime.Second);
                                //    var finishTime = new TimeSpan(todo.EstimatedFinishTime.Hour, todo.EstimatedFinishTime.Minute, todo.EstimatedFinishTime.Second);
                                //    todo.EstimatedStartTime = item.DueDate.Date.Add(startTime);
                                //    todo.EstimatedFinishTime = item.DueDate.Date.Add(finishTime);
                                //    todo.StartMixingTime = null;
                                //    todo.FinishMixingTime = null;
                                //    todo.StartStirTime = null;
                                //    todo.FinishStirTime = null;
                                //    todo.FinishDispatchingTime = null;
                                //    todo.FinishDispatchingTime = null;
                                //    todo.PrintTime = null;
                                //    todo.Status = false;
                                //    todo.AbnormalStatus = false;
                                //    todo.MixedConsumption = 0;
                                //    todo.DeliveredConsumption = 0;
                                //    todo.MixingInfoID = 0;
                                //});
                                //_repoToDoList.AddRange(todolist);
                                //_repoToDoList.Save();
                                scope.Complete();
                                flag.Add(true);
                            }
                            catch
                            {
                                scope.Dispose();
                                flag.Add(false);
                            }
                        }
                    }
                }
                return flag.All(x => x is true);
            }
            catch
            {
                return false;
            }

        }

        public async Task<bool> CheckDuplicate(int lineID, int BPFCEstablishID, DateTime dueDate)
        {
            // neu bi trung lap thi return ve true
            return await _repoPlan.FindAll().AnyAsync(x => x.BuildingID == lineID && x.BPFCEstablishID == BPFCEstablishID && x.DueDate.Date == dueDate.Date);

        }

        public async Task<List<TodolistDto>> CheckTodolistAllBuilding()
        {
            var currentTime = DateTime.Now;
            var currentDate = DateTime.Now.Date;
            var buildingModel = await _repoBuilding.FindAll()
                .Include(x => x.LunchTime).ToListAsync();

            var buildings = buildingModel.Where(x => x.Level == 2 && x.LunchTime != null).ToList();
            var lunchTimes = buildings.Select(x => x.LunchTime).ToList();
            var buildingIDList = buildings.Select(x => x.ID).ToList();
            var lines = new List<int>();
            lines = buildingModel.Where(x => x.ParentID != null && buildingIDList.Contains(x.ParentID.Value)).Select(x => x.ID).ToList();

            var model = from b in _repoBPFC.FindAll(x => !x.IsDelete)
                        join p in _repoPlan.GetAll()
                                            .Include(x => x.Building)
                                            .Where(x => x.DueDate.Date == currentDate && lines.Contains(x.BuildingID))
                                            .Select(x => new
                                            {
                                                x.HourlyOutput,
                                                x.WorkingHour,
                                                x.ID,
                                                x.BPFCEstablishID,
                                                Building = x.Building.Name,
                                                BuildingID = x.Building.ID
                                            })
                        on b.ID equals p.BPFCEstablishID
                        join g in _repoGlue.FindAll(x => x.isShow)
                                           .Include(x => x.MixingInfos)
                                           .Include(x => x.GlueIngredients)
                                               .ThenInclude(x => x.Ingredient)
                                               .ThenInclude(x => x.Supplier)
                                            .Select(x => new
                                            {
                                                x.ID,
                                                x.BPFCEstablishID,
                                                x.Name,
                                                x.Consumption,
                                                MixingInfos = x.MixingInfos.Select(x => new MixingInfoTodolistDto { ID = x.ID, Status = x.Status, EstimatedStartTime = x.StartTime, EstimatedFinishTime = x.EndTime }).ToList(),
                                                Ingredients = x.GlueIngredients.Select(x => new { x.Position, x.Ingredient.PrepareTime, Supplier = x.Ingredient.Supplier.Name, x.Ingredient.ReplacementFrequency })
                                            })
                       on b.ID equals g.BPFCEstablishID
                        select new
                        {
                            GlueID = g.ID,
                            GlueName = g.Name,
                            g.Ingredients,
                            g.MixingInfos,
                            Plans = p,
                            p.HourlyOutput,
                            p.WorkingHour,
                            g.Consumption,
                            Line = p.Building,
                            LineID = p.BuildingID,
                        };


            var mixingInfoModel = await _repoMixingInfo
           .FindAll(x => x.CreatedTime.Date == currentDate).ToListAsync();
            var dispatchModel = await _repoDispatch
          .FindAll(x => x.CreatedTime.Date == currentDate).ToListAsync();

            var test = await model.ToListAsync();
            var groupByGlueName = test.GroupBy(x => x.GlueName).Distinct().ToList();
            var resAll = new List<TodolistDto>();
            foreach (var building in buildings)
            {
                var linesList = buildingModel.Where(x => x.ParentID == building.ID).Select(x => x.ID).ToList();

                var startLunchTimeBuilding = building.LunchTime.StartTime;
                var endLunchTimeBuilding = building.LunchTime.EndTime;

                var startLunchTime = currentDate.Add(new TimeSpan(startLunchTimeBuilding.Hour, startLunchTimeBuilding.Minute, 0));
                var endLunchTime = currentDate.Add(new TimeSpan(endLunchTimeBuilding.Hour, endLunchTimeBuilding.Minute, 0));

                var plans = test.Where(x => linesList.Contains(x.LineID)).ToList();
                var groupBy = test.GroupBy(x => x.GlueName).Distinct().ToList();
                var res = new List<TodolistDto>();
                foreach (var glue in groupBy)
                {
                    var itemTodolist = new TodolistDto();
                    itemTodolist.Lines = glue.Select(x => x.Line).ToList();
                    itemTodolist.Glue = glue.Key;
                    double standardConsumption = 0;

                    foreach (var item in glue)
                    {
                        itemTodolist.GlueID = item.GlueID;
                        var supplier = string.Empty;
                        var checmicalA = item.Ingredients.ToList().FirstOrDefault(x => x.Position == "A");
                        double prepareTime = 0;
                        if (checmicalA != null)
                        {
                            supplier = checmicalA.Supplier;
                            prepareTime = checmicalA.PrepareTime;
                        }
                        itemTodolist.Supplier = supplier;
                        itemTodolist.MixingInfoTodolistDtos = item.MixingInfos;
                        itemTodolist.DeliveredActual = "-";
                        itemTodolist.Status = false;
                        var estimatedTime = currentDate.Add(new TimeSpan(7, 30, 00)) - TimeSpan.FromHours(prepareTime);
                        itemTodolist.EstimatedTime = estimatedTime;
                        var estimatedTimes = new List<DateTime>();
                        estimatedTimes.Add(estimatedTime);
                        int cycle = 8 / (int)(checmicalA.ReplacementFrequency * 60);
                        for (int i = 1; i <= cycle; i++)
                        {
                            var estimatedTimeTemp = estimatedTimes.Last().AddHours(checmicalA.ReplacementFrequency);
                            if (estimatedTimeTemp >= startLunchTime && estimatedTimeTemp <= endLunchTime)
                            {
                                estimatedTimes.Add(endLunchTime);
                            }
                            else
                            {
                                estimatedTimes.Add(estimatedTimeTemp);
                            }
                        }
                        itemTodolist.EstimatedTimes = estimatedTimes;

                        var kgPair = item.Consumption.ToDouble() / 1000;
                        standardConsumption += kgPair * (double)item.HourlyOutput * checmicalA.ReplacementFrequency;

                    }
                    itemTodolist.StandardConsumption = Math.Round(standardConsumption, 2);

                    res.Add(itemTodolist);
                    standardConsumption = 0;
                }
                var res2 = new List<TodolistDto>();
                foreach (var item in res)
                {
                    foreach (var estimatedTime in item.EstimatedTimes)
                    {
                        var mixing = mixingInfoModel.Where(x => x.EstimatedTime == estimatedTime && x.GlueName == item.Glue).FirstOrDefault();
                        var deliverAndActual = string.Empty;
                        if (mixing == null) deliverAndActual = "0kg/0kg";
                        var buildingGlue = dispatchModel.Where(x => x.MixingInfoID == mixing.ID).Select(x => x.Amount).ToList();
                        var deliver = buildingGlue.Sum();
                        deliverAndActual = $"{Math.Round(deliver / 1000, 2)}kg/{Math.Round(CalculateGlueTotal(mixing), 2)}";

                        var itemTodolist = new TodolistDto();
                        itemTodolist.Supplier = item.Supplier;
                        itemTodolist.GlueID = item.GlueID;
                        itemTodolist.ID = mixing == null ? 0 : mixing.ID;
                        itemTodolist.EstimatedStartTime = mixing == null ? DateTime.MinValue : mixing.StartTime;
                        itemTodolist.EstimatedFinishTime = mixing == null ? DateTime.MinValue : mixing.EndTime;
                        itemTodolist.MixingInfoTodolistDtos = item.MixingInfoTodolistDtos;
                        itemTodolist.Lines = item.Lines;
                        itemTodolist.Glue = item.Glue;
                        itemTodolist.StandardConsumption = item.StandardConsumption;
                        itemTodolist.DeliveredActual = deliverAndActual;
                        itemTodolist.Status = mixing == null ? false : mixing.Status;
                        itemTodolist.EstimatedTime = estimatedTime;
                        res2.Add(itemTodolist);
                    }
                }
                res2 = res2.OrderBy(x => x.Glue).Where(x => x.EstimatedTime >= currentTime && x.Status == false).ToList();
                resAll.AddRange(res2);
            }

            return resAll;
        }

        public MixingInfo PrintGlue(int mixingÌnoID)
        {
            return _toDoListService.PrintGlue(mixingÌnoID);
        }

        // Doi giay o workplan page
        public async Task<ResponseDetail<object>> ChangeBPFC(int planID, int bpfcID)
        {
            var userID = _jwtService.GetUserID();
            var plan = await _repoPlan.FindAll(x => x.ID == planID).FirstOrDefaultAsync();
            if (plan == null) return new ResponseDetail<object>(null, false, "Không có kế hoạch làm việc nào tồn tại!");

            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var fisnishWorkingTime = plan.FinishWorkingTime;
                    var ct = DateTime.Now.ToRemoveSecond();
                    // var ct = new DateTime(2021, 2,3, 15,45,0);
                    plan.FinishWorkingTime = ct;
                    plan.IsChangeBPFC = true;

                    _repoPlan.Update(plan);
                    await _repoPlan.SaveAll();

                    var planLine = await _repoPlan.FindAll(x => ct.Date == x.DueDate.Date && x.BuildingID == plan.BuildingID).ToListAsync();
                    var startWorkingTime = ct;
                    var check = planLine.Any(x => startWorkingTime > x.StartWorkingTime && startWorkingTime >= x.FinishWorkingTime);
                    foreach (var item in planLine)
                    {
                        if (startWorkingTime < item.FinishWorkingTime)
                        {
                            return new ResponseDetail<object>(null, false, "Kế hoạch làm việc này đã trùng lắp với khoảng thời gian khác!");
                        }
                    }
                    var model = new Plan();
                    model.StartWorkingTime = ct;
                    model.CreatedDate = ct;
                    model.ModifyTime = ct;
                    model.IsOvertime = plan.IsOvertime;

                    model.BPFCEstablishID = bpfcID;
                    model.BuildingID = plan.BuildingID;
                    model.DueDate = plan.DueDate;
                    model.HourlyOutput = plan.HourlyOutput;
                    model.FinishWorkingTime = fisnishWorkingTime;
                    _repoPlan.Add(model);
                    await _repoPlan.SaveAll();

                    var timeOfDay = ct.ToRemoveSecond().TimeOfDay;
                    var todoDelete = await _repoToDoList.FindAll(x => x.EstimatedStartTime.TimeOfDay >= timeOfDay && x.PlanID == planID && x.IsDelete == false).ToListAsync();
                    todoDelete.ForEach(item =>
                    {
                        item.IsDelete = true;
                    });
                    var todoShow = await _repoToDoList.FindAll(x => x.EstimatedStartTime.TimeOfDay < timeOfDay && x.PlanID == planID && x.IsDelete).ToListAsync();
                    todoShow.ForEach(item =>
                    {
                        item.IsDelete = false;
                    });
                    var todo = todoShow.Concat(todoDelete).DistinctBy(x => x.ID).ToList();

                    if (todo.Count > 0)
                    {
                        _repoToDoList.UpdateRange(todo);
                        await _repoToDoList.SaveAll();
                    }

                    var dispatchModel = new List<Dispatch>();

                    // 10:30 > 10:50 && 11:00 >= 10:50 
                    var deletingList = await _repoDispatchList.FindAll(x => x.EstimatedStartTime.TimeOfDay >= timeOfDay && x.EstimatedFinishTime.TimeOfDay > timeOfDay && x.PlanID == planID && x.IsDelete == false).ToListAsync();
                    deletingList.ForEach(item =>
                    {
                        item.IsDelete = true;
                        var dispatch = _repoDispatch.FindAll(x => x.EstimatedStartTime == item.EstimatedStartTime
                            && x.EstimatedFinishTime == item.EstimatedStartTime && x.GlueNameID == item.GlueNameID).ToList();
                        dispatch.ForEach(item =>
                        {
                            item.IsDelete = true;
                        });
                        dispatchModel.AddRange(dispatch);

                    });
                    if (dispatchModel.Count > 0)
                    {
                        _repoDispatch.UpdateRange(dispatchModel);
                        await _repoDispatch.SaveAll();
                    }
                    var showList = await _repoDispatchList.FindAll(x => x.EstimatedStartTime.TimeOfDay < timeOfDay && x.EstimatedFinishTime.TimeOfDay <= timeOfDay && x.PlanID == planID && x.IsDelete).ToListAsync();
                    showList.ForEach(item =>
                    {
                        item.IsDelete = false;
                    });
                    var dispatching = showList.Concat(deletingList).DistinctBy(x => x.ID).ToList();
                    if (dispatching.Count > 0)
                    {
                        _repoDispatchList.UpdateRange(dispatching);
                        await _repoDispatchList.SaveAll();
                    }

                    transaction.Complete();
                    return new ResponseDetail<object>(null, true, "Tạo kế hoạch làm việc thành công!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    transaction.Dispose();
                    return new ResponseDetail<object>(null, false, "Không tạo được kế hoạch làm việc!");
                }
            }


        }

        public async Task<ResponseDetail<object>> Online(int planID)
        {
            // B1: Túm cổ từ trong database ra

            // B2: Cập nhật lại isOfflineStatus = true

            // B3: Vào bảng Building lấy period ra lấy ra giờ kết thúc làm việc

            // B4: Vào bảng Todolist lấy hết danh sách ra theo planID điều kiện là isdelete = true Neu khong tang ca thi khoi mo tang ca ra, Cập nhật lại là false

            // B5: Vào bảng DispatchList lấy hết danh sách ra theo planID điều kiện là isdelete = true Neu khong tang ca thi khoi mo tang ca ra, Cập nhật lại là false
            // -------------------------------------------
            // B1: Túm cổ từ trong database ra
            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var userID = _jwtService.GetUserID();
                    var ct = DateTime.Now.ToRemoveSecond();
                    var currentDate = ct.Date;
                    var planUpdate = await _repoPlan.FindAll(x => planID == x.ID).Include(x => x.Building).FirstOrDefaultAsync();
                    if (planUpdate == null) return new ResponseDetail<object>
                    {
                        Status = false,
                        Message = "Không có danh sách keo nào cho kế hoạch làm việc này!"
                    };
                    planUpdate.UpdatedOnline = ct;
                    planUpdate.UpdatedOnlineBy = userID;
                    planUpdate.IsOffline = false;
                    _repoPlan.Update(planUpdate);
                    await _repoPlan.SaveAll();

                    // B3: Vào bảng Building lấy period ra lấy ra giờ kết thúc làm việc

                    var building = await _repoBuilding.FindAll(x => x.ID == planUpdate.Building.ParentID)
                      .Include(x => x.PeriodMixingList)
                      .FirstOrDefaultAsync();

                    var periods = building.PeriodMixingList;

                    // Chi tao gio tang ca
                    periods = periods.Where(x => x.IsOvertime == true).ToList();
                    var finishWorkingTimeOfWorkplan = periods.OrderBy(x => x.EndTime).FirstOrDefault().StartTime;

                    // B4: Vào bảng Todolist lấy hết danh sách ra theo planID điều kiện là isdelete = true nếu có tăng ca thì mở lại, Cập nhật lại là false

                    var timeOfDay = ct.ToRemoveSecond().TimeOfDay;
                    var todoDelete = await _repoToDoList.FindAll(x => x.EstimatedStartTime.TimeOfDay >= timeOfDay && x.PlanID == planID && x.IsDelete == true).ToListAsync();
                    // Neu khong tang ca thi khoi mo tang ca ra
                    if (planUpdate.IsOvertime == false)
                    {
                        todoDelete = todoDelete.Where(x => x.EstimatedStartTime.TimeOfDay < finishWorkingTimeOfWorkplan.TimeOfDay).ToList();
                    }
                    todoDelete.ForEach(item =>
                    {
                        item.IsDelete = false;
                    });
                    _repoToDoList.UpdateRange(todoDelete);
                    await _repoToDoList.SaveAll();

                    // B5: Vào bảng DispatchList lấy hết danh sách ra theo planID điều kiện là isdelete = true Neu khong tang ca thi khoi mo tang ca ra, Cập nhật lại là false
                    var deletingList = await _repoDispatchList.FindAll(x => x.EstimatedStartTime.TimeOfDay >= timeOfDay && x.EstimatedFinishTime.TimeOfDay > timeOfDay && x.PlanID == planID && x.IsDelete == true).ToListAsync();
                    // Neu khong tang ca thi khoi mo tang ca ra
                    if (planUpdate.IsOvertime == false)
                    {
                        deletingList = deletingList.Where(x => x.EstimatedStartTime.TimeOfDay < finishWorkingTimeOfWorkplan.TimeOfDay).ToList();
                    }
                    deletingList.ForEach(item =>
                    {
                        item.IsDelete = false;
                    });
                    //cap nhat dispatchlist
                    _repoDispatchList.UpdateRange(deletingList);
                    await _repoDispatchList.SaveAll();

                    transaction.Complete();
                    return new ResponseDetail<object>
                    {
                        Status = true,
                        Message = $"Đã ngưng chuyền {planUpdate.Building.Name}!"
                    };
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                    return new ResponseDetail<object>
                    {
                        Status = false,
                        Message = ex.Message
                    };
                }
            }
        }

        public async Task<ResponseDetail<object>> Offline(int planID)
        {
            // B1: Túm cổ từ trong database ra

            // B2: Cập nhật lại isOfflineStatus = true

            // B3: Vào bảng Todolist lấy hết danh sách ra theo planID điều kiện là isdelete = false, Cập nhật lại là true

            // B4: Vào bảng DispatchList lấy hết danh sách ra theo planID điều kiện là isdelete = false, Cập nhật lại là true
            // ------------------------------------------------------------------
            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var userID = _jwtService.GetUserID();
                    var ct = DateTime.Now.ToRemoveSecond();
                    var currentDate = ct.Date;
                    // B1: Túm cổ từ trong database ra
                    var planUpdate = await _repoPlan.FindAll(x => planID == x.ID)
                                            .Include(x => x.Building).FirstOrDefaultAsync();
                    if (planUpdate == null) return new ResponseDetail<object>
                    {
                        Status = false,
                        Message = "Không có danh sách keo nào cho kế hoạch làm việc này!"
                    };
                    planUpdate.UpdatedOffline = ct;
                    planUpdate.UpdatedOfflineBy = userID;
                    planUpdate.IsOffline = true;
                    // cap nhat workplan
                    _repoPlan.Update(planUpdate);
                    await _repoPlan.SaveAll();



                    var timeOfDay = ct.ToRemoveSecond().TimeOfDay;


                    // B3: Vào bảng Todolist lấy hết danh sách ra theo planID điều kiện là isdelete = false, Cập nhật lại là true

                    var todoDelete = await _repoToDoList.FindAll(x => x.EstimatedStartTime.TimeOfDay >= timeOfDay && x.PlanID == planID && x.IsDelete == false).ToListAsync();
                    todoDelete.ForEach(item =>
                    {
                        item.IsDelete = true;
                    });
                    _repoToDoList.UpdateRange(todoDelete);
                    await _repoToDoList.SaveAll();

                    // B4: Vào bảng DispatchList lấy hết danh sách ra theo planID điều kiện là isdelete = false, Cập nhật lại là true
                    var deletingList = await _repoDispatchList.FindAll(x => x.EstimatedStartTime.TimeOfDay >= timeOfDay && x.EstimatedFinishTime.TimeOfDay > timeOfDay && x.PlanID == planID && x.IsDelete == false).ToListAsync();
                    deletingList.ForEach(item =>
                    {
                        item.IsDelete = true;
                    });

                    _repoDispatchList.UpdateRange(deletingList);
                    await _repoDispatchList.SaveAll();

                    transaction.Complete();
                    return new ResponseDetail<object>
                    {
                        Status = true,
                        Message = $"Chuyền {planUpdate.Building.Name} đã online!"
                    };
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                    return new ResponseDetail<object>
                    {
                        Status = false,
                        Message = $"{ex.Message}"
                    };
                }
            }
        }

        // Khong su dung
        public Task<bool> EditDelivered(int id, string qty)
        {
            throw new System.NotImplementedException();

        }

        public Task<object> DispatchGlue(BuildingGlueForCreateDto obj)
        {
            throw new System.NotImplementedException();
        }

        // Khong su dung
        public Task<bool> DeleteDelivered(int id)
        {
            throw new System.NotImplementedException();

        }

        #endregion

        #region Report
        // LoadData
        public async Task<byte[]> Report(DateTime startDate, DateTime endDate)
        {
            var plans = await _repoPlan.FindAll()
                .Where(x => x.DueDate.Date >= startDate.Date && x.DueDate.Date <= endDate.Date)
                .Include(x => x.Building)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelName)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelNo)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueIngredients)
                    .ThenInclude(x => x.Ingredient)
                .Select(x => new
                {
                    Glues = x.BPFCEstablish.Glues.Where(x => x.isShow),
                    GlueIngredients = x.BPFCEstablish.Glues.Where(x => x.isShow).SelectMany(x => x.GlueIngredients),
                    ModelName = x.BPFCEstablish.ModelName.Name,
                    ModelNo = x.BPFCEstablish.ModelNo.Name,
                    x.Quantity,
                    Line = x.Building.Name,
                    LineID = x.Building.ID,
                    x.DueDate,
                    x.BPFCEstablishID
                }).OrderBy(x => x.DueDate.Date)
                .ToListAsync();

            //var buildingGlues = await _repoBuildingGlue.FindAll()
            //    .Where(x => x.CreatedDate.Date >= startDate.Date && x.CreatedDate.Date <= endDate.Date)
            //    .ToListAsync();
            var dispatchList = await _repoDispatch.FindAll()
                .Include(x => x.MixingInfo)
                .ThenInclude(x => x.Glue)
               .Where(x => x.CreatedTime.Date >= startDate.Date && x.CreatedTime.Date <= endDate.Date)
               .ToListAsync();
            var buildingGlueModel = from a in dispatchList
                                    join b in _repoGlue.FindAll().Include(x => x.GlueIngredients).ToList() on a.MixingInfo.Glue.GlueNameID equals b.GlueNameID
                                    select new
                                    {
                                        Qty = a.Amount,
                                        BuildingID = a.LineID,
                                        CreatedDate = a.CreatedTime,
                                        b.BPFCEstablishID,
                                        IngredientIDList = b.GlueIngredients.Select(x => x.IngredientID)
                                    };
            var ingredients = plans.SelectMany(x => x.GlueIngredients).Select(x => new IngredientReportDto
            {
                CBD = x.Ingredient.CBD,
                Real = x.Ingredient.Real,
                Name = x.Ingredient.Name,
                ID = x.IngredientID,
                Position = x.Position
            }).DistinctBy(x => x.Name);

            var ingredientsHeader = ingredients.Select(x => x.Name).ToList();

            var headers = new ReportHeaderDto();
            headers.Ingredients = ingredientsHeader;
            var bodyList = new List<ReportBodyDto>();
            var planModel = plans.OrderBy(x => x.DueDate.Date).ThenBy(x => x.Line).ToList();
            foreach (var plan in planModel)
            {
                var body = new ReportBodyDto
                {
                    Day = plan.DueDate.Day,
                    CBD = 0,
                    Real = 0,
                    ModelName = plan.ModelName,
                    ModelNo = plan.ModelNo,
                    Quantity = plan.Quantity,
                    Line = plan.Line,
                    LineID = plan.LineID,
                    Date = plan.DueDate.Date,
                };
                var ingredientsBody2 = new List<IngredientBodyReportDto>();
                foreach (var ingredient in ingredients)
                {
                    foreach (var glue in plan.Glues)
                    {
                        if (glue.GlueIngredients.Any(x => x.IngredientID == ingredient.ID) && plan.BPFCEstablishID == glue.BPFCEstablishID)
                        {
                            var buildingGlue = buildingGlueModel.Where(x => x.BuildingID == body.LineID && x.IngredientIDList.Contains(ingredient.ID) && x.CreatedDate.Date == plan.DueDate.Date && x.BPFCEstablishID == glue.BPFCEstablishID)
                            .Distinct().ToList();

                            var quantity = buildingGlue.Select(x => x.Qty).ToList().ConvertAll<double>(Convert.ToDouble).Sum();
                            var glueIngredients = glue.GlueIngredients.DistinctBy(x => x.Position).ToList();
                            double value = CalculateIngredientByPositon(glueIngredients, ingredient, quantity);
                            ingredientsBody2.Add(new IngredientBodyReportDto { Value = value, Name = ingredient.Name, Line = body.Line });
                        }
                    }
                }
                body.Ingredients2 = ingredientsBody2;
                var ingredientsBody = new List<double>();

                foreach (var ingredientName in ingredientsHeader)
                {
                    var model = ingredientsBody2.FirstOrDefault(x => x.Name.Equals(ingredientName));
                    if (model != null)
                    {
                        ingredientsBody.Add(model.Value);
                    }
                    else
                    {
                        ingredientsBody.Add(0);

                    }

                }
                body.Ingredients = ingredientsBody;

                bodyList.Add(body);
            }

            return ExportExcel(headers, bodyList, ingredients.ToList());
        }

        public async Task<List<ConsumtionDto>> ConsumptionByLineCase1(ReportParams reportParams)
        {
            var res = await ConsumptionReportByBuilding(reportParams);
            return res.OrderByDescending(x => x.Percentage).ToList();
        }

        public async Task<List<ConsumtionDto>> ConsumptionByLineCase2(ReportParams reportParams)
        {
            var res = await ConsumptionReportByBuilding(reportParams);

            return res.OrderByDescending(x => x.DueDate).OrderBy(x => x.DueDate).ThenBy(x => x.Line).ThenBy(x => x.ID).ThenByDescending(x => x.Percentage).ToList();
        }

        private async Task<List<ConsumtionDto>> ConsumptionReportByBuilding(ReportParams reportParams)
        {
            var startDate = reportParams.StartDate.Date;
            var endDate = reportParams.EndDate.Date;
            var buildingID = reportParams.BuildingID;
            var lines = new List<int>();
            if (buildingID == 0)
            {
                lines = await _repoBuilding.FindAll(x => x.Level == LINE_LEVEL).Select(x => x.ID).ToListAsync();
            }
            else
            {
                lines = await _repoBuilding.FindAll(x => x.ParentID == buildingID).Select(x => x.ID).ToListAsync();
            }
            //var buildingGlueModel = await _repoBuildingGlue.FindAll(x => x.CreatedDate.Date >= startDate && x.CreatedDate.Date <= endDate && lines.Contains(x.BuildingID)).Include(x => x.MixingInfo).ToListAsync();
            var dispatchModel = await _repoDispatch.FindAll(x => x.EstimatedFinishTime.Date >= startDate && x.EstimatedFinishTime.Date <= endDate && lines.Contains(x.LineID)).Include(x => x.MixingInfo).ToListAsync();
            var model = await _repoPlan.FindAll()
                 .Include(x => x.BPFCEstablish)
                 .ThenInclude(x => x.Glues)
                 .Include(x => x.BPFCEstablish)
                 .ThenInclude(x => x.Plans)
                 .ThenInclude(x => x.Building)
                 .Include(x => x.BPFCEstablish)
                 .ThenInclude(x => x.ModelName)
                 .Include(x => x.BPFCEstablish)
                 .ThenInclude(x => x.ModelNo)
                 .Include(x => x.BPFCEstablish)
                 .ThenInclude(x => x.ArticleNo)
                 .Include(x => x.BPFCEstablish)
                 .ThenInclude(x => x.ArtProcess)
                 .ThenInclude(x => x.Process)
                 .Where(x => !x.BPFCEstablish.IsDelete)
                 .Select(x => new
                 {
                     x.BPFCEstablishID,
                     x.Quantity,
                     x.DueDate.Date,
                     x.BuildingID,
                     Line = x.Building.Name,
                     ModelName = x.BPFCEstablish.ModelName.Name,
                     ModelNo = x.BPFCEstablish.ModelNo.Name,
                     ArticleNo = x.BPFCEstablish.ArticleNo.Name,
                     Process = x.BPFCEstablish.ArtProcess.Process.Name,
                     Plans = x.BPFCEstablish.Plans,
                     Glues = x.BPFCEstablish.Glues.ToList()
                 }).Where(x => x.Plans.Any(x => lines.Contains(x.BuildingID)) && x.Date >= startDate && x.Date <= endDate)
                 .ToListAsync();
            var list = new List<ConsumtionDto>();
            foreach (var item in model)
            {
                foreach (var glue in item.Glues.Where(x => x.isShow))
                {
                    var std = glue.Consumption.ToFloat();
                    var buildingGlue = dispatchModel.FirstOrDefault(x => x.MixingInfo.Glue.GlueNameID.Equals(glue.GlueNameID) && x.EstimatedFinishTime.Date == item.Date && item.BuildingID == x.LineID);
                    var totalConsumption = buildingGlue == null ? 0 : buildingGlue.Amount.ToFloat();
                    var realConsumption = totalConsumption > 0 && item.Quantity > 0 ? Math.Round(totalConsumption / item.Quantity, 2).ToFloat() : 0;
                    var diff = std > 0 && realConsumption > 0 ? Math.Round(realConsumption - std, 2).ToFloat() : 0;
                    var percentage = std > 0 ? Math.Round((diff / std) * 100).ToFloat() : 0;
                    list.Add(new ConsumtionDto
                    {
                        ModelName = item.ModelName,
                        ModelNo = item.ModelNo,
                        ArticleNo = item.ArticleNo,
                        Process = item.Process,
                        Line = item.Line,
                        Glue = glue.Name,
                        Std = std,
                        Qty = item.Quantity,
                        TotalConsumption = totalConsumption > 0 ? (float)Math.Round(totalConsumption / 1000, 2) : 0,
                        RealConsumption = realConsumption,
                        Diff = diff,
                        ID = item.BPFCEstablishID,
                        Percentage = percentage,
                        DueDate = item.Date,
                        MixingDate = buildingGlue == null || buildingGlue.MixingInfo == null ? DateTime.MinValue : buildingGlue.MixingInfo.CreatedTime
                    });
                }
            }
            return list.ToList();
            throw new NotImplementedException();
        }

        public async Task<byte[]> ReportConsumptionCase2(ReportParams reportParams)
        {
            var res = await ConsumptionReportByBuilding(reportParams);
            return ExportExcelConsumptionCase2(res);
        }

        public async Task<byte[]> ReportConsumptionCase1(ReportParams reportParams)
        {
            var res = await ConsumptionReportByBuilding(reportParams);
            return ExportExcelConsumptionCase1(res);
        }

        public async Task<byte[]> GetReportByBuilding(DateTime startDate, DateTime endDate, int building)
        {
            var lineList = await _repoBuilding.FindAll(x => x.ParentID == building).Select(x => x.ID).ToListAsync();
            var plans = await _repoPlan.FindAll()
                 .Where(x => x.DueDate.Date >= startDate.Date && x.DueDate.Date <= endDate.Date && lineList.Contains(x.BuildingID))
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
                    .ThenInclude(x => x.GlueIngredients)
                    .ThenInclude(x => x.Ingredient)
                .Select(x => new
                {
                    Glues = x.BPFCEstablish.Glues.Where(x => x.isShow),
                    GlueIngredients = x.BPFCEstablish.Glues.Where(x => x.isShow).SelectMany(x => x.GlueIngredients),
                    ModelName = x.BPFCEstablish.ModelName.Name,
                    ModelNo = x.BPFCEstablish.ModelNo.Name,
                    ArticleNo = x.BPFCEstablish.ArticleNo.Name,
                    Process = x.BPFCEstablish.ArtProcess.Process.Name,
                    x.Quantity,
                    Line = x.Building.Name,
                    LineID = x.Building.ID,
                    x.DueDate,
                    x.BPFCEstablishID
                }).OrderBy(x => x.DueDate.Date)
                .ToListAsync();


            var dispatchList = await _repoDispatch.FindAll()
                .Include(x => x.MixingInfo)
                .ThenInclude(x => x.Glue)
               .Where(x => x.CreatedTime.Date >= startDate.Date && x.CreatedTime.Date <= endDate.Date)
               .ToListAsync();
            var buildingGlueModel = from a in dispatchList
                                    join b in _repoGlue.FindAll().Include(x => x.GlueIngredients).ToList() on a.MixingInfo.Glue.GlueNameID equals b.GlueNameID
                                    select new
                                    {
                                        Qty = a.Amount,
                                        BuildingID = a.LineID,
                                        CreatedDate = a.CreatedTime,
                                        b.BPFCEstablishID,
                                        IngredientIDList = b.GlueIngredients.Select(x => x.IngredientID)
                                    };
            var ingredients = plans.SelectMany(x => x.GlueIngredients).Select(x => new IngredientReportDto
            {
                CBD = x.Ingredient.CBD,
                Real = x.Ingredient.Real,
                Name = x.Ingredient.Name,
                ID = x.IngredientID,
                Position = x.Position
            }).DistinctBy(x => x.Name);

            var ingredientsHeader = ingredients.Select(x => x.Name).ToList();

            var headers = new ReportHeaderDto();
            headers.Ingredients = ingredientsHeader;
            var bodyList = new List<ReportBodyDto>();
            var planModel = plans.OrderBy(x => x.DueDate.Date).ThenBy(x => x.Line).ToList();
            foreach (var plan in planModel)
            {
                var body = new ReportBodyDto
                {
                    Day = plan.DueDate.Day,
                    CBD = 0,
                    Real = 0,
                    ModelName = plan.ModelName,
                    ModelNo = plan.ModelNo,
                    ArticleNO = plan.ArticleNo,
                    Process = plan.Process,
                    Quantity = plan.Quantity,
                    Line = plan.Line,
                    LineID = plan.LineID,
                    Date = plan.DueDate.Date,
                };
                var ingredientsBody2 = new List<IngredientBodyReportDto>();
                foreach (var ingredient in ingredients)
                {
                    foreach (var glue in plan.Glues)
                    {
                        if (glue.GlueIngredients.Any(x => x.IngredientID == ingredient.ID) && plan.BPFCEstablishID == glue.BPFCEstablishID)
                        {
                            var buildingGlue = buildingGlueModel.Where(x => x.BuildingID == body.LineID && x.IngredientIDList.Contains(ingredient.ID) && x.CreatedDate.Date == plan.DueDate.Date && x.BPFCEstablishID == glue.BPFCEstablishID)
                            .Distinct().ToList();

                            var quantity = buildingGlue.Select(x => x.Qty).ToList().ConvertAll<double>(Convert.ToDouble).Sum();
                            var glueIngredients = glue.GlueIngredients.DistinctBy(x => x.Position).ToList();
                            double value = CalculateIngredientByPositon(glueIngredients, ingredient, quantity);
                            ingredientsBody2.Add(new IngredientBodyReportDto { Value = value, Name = ingredient.Name, Line = body.Line });
                        }
                    }
                }
                body.Ingredients2 = ingredientsBody2;
                var ingredientsBody = new List<double>();

                foreach (var ingredientName in ingredientsHeader)
                {
                    var model = ingredientsBody2.FirstOrDefault(x => x.Name.Equals(ingredientName));
                    if (model != null)
                    {
                        ingredientsBody.Add(model.Value);
                    }
                    else
                    {
                        ingredientsBody.Add(0);

                    }

                }
                body.Ingredients = ingredientsBody;

                bodyList.Add(body);
            }

            return ExportExcel(headers, bodyList, ingredients.ToList());
            throw new NotImplementedException();
        }

        public async Task<byte[]> GetNewReportByBuilding(DateTime startDate, DateTime endDate, int building)
        {
            var lineList = await _repoBuilding.FindAll(x => x.ParentID == building).Select(x => x.ID).ToListAsync();
            var plans = await _repoPlan.FindAll()
                 .Where(x => x.DueDate.Date >= startDate.Date && x.DueDate.Date <= endDate.Date && lineList.Contains(x.BuildingID))
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
                     .ThenInclude(x => x.GlueIngredients)
                     .ThenInclude(x => x.Ingredient)
                 .Select(x => new
                 {
                     Glues = x.BPFCEstablish.Glues.Where(x => x.isShow),
                     GlueIngredients = x.BPFCEstablish.Glues.Where(x => x.isShow).SelectMany(x => x.GlueIngredients),
                     ModelName = x.BPFCEstablish.ModelName.Name,
                     ModelNo = x.BPFCEstablish.ModelNo.Name,
                     ArticleNo = x.BPFCEstablish.ArticleNo.Name,
                     Process = x.BPFCEstablish.ArtProcess.Process.Name,
                     x.Quantity,
                     Line = x.Building.Name,
                     LineID = x.Building.ID,
                     x.DueDate,
                     x.BPFCEstablishID
                 })
                 .OrderBy(x => x.ModelName)
                 .ThenBy(x => x.ModelNo)
                 .ToListAsync();

            var dispatchList = await _repoDispatch.FindAll()
                .Include(x => x.MixingInfo)
                .ThenInclude(x => x.Glue)
               .Where(x => lineList.Contains(x.LineID) && x.CreatedTime.Date >= startDate.Date && x.CreatedTime.Date <= endDate.Date)
               .ToListAsync();
            var dispatchModel = from a in dispatchList
                                join b in _repoGlue.FindAll().Include(x => x.GlueIngredients).ToList() on a.MixingInfo.Glue.GlueNameID equals b.GlueNameID
                                select new
                                {
                                    Qty = a.Amount,
                                    BuildingID = a.LineID,
                                    CreatedDate = a.CreatedTime,
                                    b.BPFCEstablishID,
                                    IngredientIDList = b.GlueIngredients.Select(x => x.IngredientID)
                                };
            var ingredients = plans.SelectMany(x => x.GlueIngredients).Select(x => new IngredientReportDto
            {
                CBD = x.Ingredient.CBD,
                Real = x.Ingredient.Real,
                Name = x.Ingredient.Name,
                ID = x.IngredientID,
                Position = x.Position
            }).DistinctBy(x => x.Name);

            var ingredientsHeader = ingredients.Select(x => x.Name).ToList();

            var headers = new ReportHeaderDto();
            headers.Ingredients = ingredientsHeader;
            var bodyList = new List<ReportBodyDto>();
            var groupByPlan = plans.GroupBy(x => new
            {
                x.ModelName,
                x.ModelNo,
                x.ArticleNo,
                x.Process,

            }).ToList();
            var planModel = plans.OrderBy(x => x.DueDate.Date).ThenBy(x => x.Line).ToList();
            foreach (var planItem in groupByPlan)
            {
                var body = new ReportBodyDto
                {
                    ModelName = planItem.Key.ModelName,
                    ModelNo = planItem.Key.ModelNo,
                    ArticleNO = planItem.Key.ArticleNo,
                    Process = planItem.Key.Process,
                    CBD = 0,
                    Real = 0,
                    Quantity = planItem.Sum(x => x.Quantity),
                    Line = planItem.First().Line,
                    LineID = planItem.First().LineID,
                };
                foreach (var plan in planItem)
                {
                    var ingredientsBody2 = new List<IngredientBodyReportDto>();
                    foreach (var ingredient in ingredients)
                    {
                        foreach (var glue in plan.Glues)
                        {
                            if (glue.GlueIngredients.Any(x => x.IngredientID == ingredient.ID) && plan.BPFCEstablishID == glue.BPFCEstablishID)
                            {
                                var dispatch = dispatchModel.Where(x => x.BuildingID == body.LineID
                                && x.IngredientIDList.Contains(ingredient.ID)
                                && x.CreatedDate.Date >= startDate.Date && x.CreatedDate.Date <= endDate.Date
                                && x.BPFCEstablishID == glue.BPFCEstablishID
                                )
                                .Distinct().ToList();

                                var quantity = dispatch.Select(x => x.Qty).Sum();
                                var glueIngredients = glue.GlueIngredients.DistinctBy(x => x.Position).ToList();
                                double value = CalculateIngredientByPositon(glueIngredients, ingredient, quantity);
                                ingredientsBody2.Add(new IngredientBodyReportDto { Value = value, Name = ingredient.Name, Line = body.Line });
                            }
                        }
                    }
                    body.Ingredients2 = ingredientsBody2;
                    var ingredientsBody = new List<double>();

                    foreach (var ingredientName in ingredientsHeader)
                    {
                        var model = ingredientsBody2.FirstOrDefault(x => x.Name.Equals(ingredientName));
                        if (model != null)
                        {
                            ingredientsBody.Add(model.Value);
                        }
                        else
                        {
                            ingredientsBody.Add(0);
                        }
                    }
                    body.Ingredients = ingredientsBody;
                }
                bodyList.Add(body);
            }

            return ExportExcelNewReport(headers, bodyList, ingredients.ToList(), startDate, endDate);
            throw new NotImplementedException();
        }

        private Byte[] ExportExcelNewReport(ReportHeaderDto header, List<ReportBodyDto> bodyList, List<IngredientReportDto> ingredients, DateTime startDate, DateTime endDate)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var memoryStream = new MemoryStream();
                using (ExcelPackage p = new ExcelPackage(memoryStream))
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "Henry Pham";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "Report";
                    //Tạo một sheet để làm việc trên đó
                    p.Workbook.Worksheets.Add("Report");

                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = p.Workbook.Worksheets["Report"];

                    // đặt tên cho sheet
                    ws.Name = "Report";
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 11;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";



                    int ingredientRealRowIndex = 1;
                    int ingredientCBDRowIndex = 2;
                    int startIngredientCostingIndex = 7;
                    int ingredientCBDColIndex = startIngredientCostingIndex;
                    int ingredientRealColIndex = startIngredientCostingIndex;

                    ws.Cells[ingredientRealRowIndex, ingredientRealColIndex++].Value = "REAL";
                    ws.Cells[ingredientCBDRowIndex, ingredientCBDColIndex++].Value = "CBD";

                    foreach (var ingredient in ingredients)
                    {
                        int cbdColumn = ingredientCBDColIndex++;
                        int realColumn = ingredientRealColIndex++;
                        ws.Cells[ingredientCBDRowIndex, cbdColumn].Value = ingredient.CBD;
                        ws.Cells[ingredientRealRowIndex, realColumn].Value = ingredient.Real;

                        ws.Cells[ingredientCBDRowIndex, cbdColumn].Style.TextRotation = 90;
                        ws.Cells[ingredientRealRowIndex, realColumn].Style.TextRotation = 90;
                        ws.Cells[ingredientCBDRowIndex, cbdColumn].AutoFitColumns(5);
                        ws.Cells[ingredientRealRowIndex, realColumn].AutoFitColumns(5);

                        ws.Cells[ingredientRealRowIndex, realColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[ingredientRealRowIndex, realColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        ws.Cells[ingredientCBDRowIndex, cbdColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[ingredientCBDRowIndex, cbdColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    int headerRowIndex = 3;
                    int headerColIndex = 1;

                    for (headerColIndex = 1; headerColIndex <= startIngredientCostingIndex; headerColIndex++)
                    {
                        var modelNameExcelRange = ws.Cells[headerRowIndex, headerColIndex++];
                        var modelNOExcelRange = ws.Cells[headerRowIndex, headerColIndex++];
                        var articleNOExcelRange = ws.Cells[headerRowIndex, headerColIndex++];
                        var processExcelRange = ws.Cells[headerRowIndex, headerColIndex++];
                        var quantityExcelRange = ws.Cells[headerRowIndex, headerColIndex++];
                        var CBDExcelRange = ws.Cells[headerRowIndex, headerColIndex++];
                        var realExcelRange = ws.Cells[headerRowIndex, headerColIndex++];
                        modelNameExcelRange.Value = header.ModelName;
                        modelNOExcelRange.Value = header.ModelNo;
                        articleNOExcelRange.Value = header.ArticleNO;
                        processExcelRange.Value = header.Process;
                        quantityExcelRange.Value = header.Quantity;
                        CBDExcelRange.Value = header.CBD;
                        realExcelRange.Value = header.Real;
                        // Style Header
                        SetStyleEachCell(modelNameExcelRange);
                        SetStyleEachCell(modelNOExcelRange);
                        SetStyleEachCell(articleNOExcelRange);
                        SetStyleEachCell(processExcelRange);
                        SetStyleEachCell(quantityExcelRange);
                        SetStyleEachCell(CBDExcelRange);
                        SetStyleEachCell(realExcelRange);
                    }
                    // end Style
                    int ingredientColIndex = 8;
                    foreach (var ingredient in header.Ingredients)
                    {
                        int col = ingredientColIndex++;
                        ws.Cells[headerRowIndex, col].Value = ingredient;
                        ws.Cells[headerRowIndex, col].Style.Fill.PatternType = ExcelFillStyle.Solid;

                        ws.Cells[headerRowIndex, col].Style.TextRotation = 90;
                        ws.Cells[headerRowIndex, col].Style.Font.Color.SetColor(Color.White);
                        ws.Cells[headerRowIndex, col].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#808080"));

                    }
                    int colIndex = 1;
                    int rowIndex = 3;
                    // với mỗi item trong danh sách sẽ ghi trên 1 dòng
                    foreach (var body in bodyList)
                    {
                        // bắt đầu ghi từ cột 1. Excel bắt đầu từ 1 không phải từ 0 #c0514d
                        colIndex = 1;

                        // rowIndex tương ứng từng dòng dữ liệu
                        rowIndex++;


                        //gán giá trị cho từng cell                      
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelName;
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelNo;
                        ws.Cells[rowIndex, colIndex++].Value = body.ArticleNO;
                        ws.Cells[rowIndex, colIndex++].Value = body.Process;
                        ws.Cells[rowIndex, colIndex++].Value = body.Quantity == 0 ? string.Empty : body.Quantity.ToString();

                        var cbds = ingredients.Select(x => x.CBD).ToArray();
                        var reals = ingredients.Select(x => x.Real).ToArray();

                        var cbdRowTotal = body.Ingredients.ToArray();
                        var realRowTotal = body.Ingredients.ToArray();
                        var value = body.Ingredients.Sum();
                        double CBD = 0, real = 0;

                        if (value > 0 && body.Quantity > 0)
                            CBD = Math.Round(SumProduct(cbdRowTotal, cbds) / body.Quantity, 3, MidpointRounding.AwayFromZero);
                        if (value > 0 && body.Quantity > 0)
                            real = Math.Round(SumProduct(realRowTotal, reals) / body.Quantity, 3, MidpointRounding.AwayFromZero);

                        ws.Cells[rowIndex, colIndex++].Value = CBD == 0 ? string.Empty : CBD.ToString();
                        ws.Cells[rowIndex, colIndex++].Value = real == 0 ? string.Empty : real.ToString();

                        foreach (var ingredient in body.Ingredients)
                        {
                            int col = colIndex++;
                            ws.Cells[rowIndex, col].Value = ingredient > 0 ? Math.Round(ingredient, 2, MidpointRounding.AwayFromZero).ToString() : string.Empty;
                            ws.Cells[rowIndex, col].Style.Font.Size = 8;
                            ws.Cells[rowIndex, col].Style.Font.Color.SetColor(Color.DarkRed);
                            ws.Cells[rowIndex, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[rowIndex, col].AutoFitColumns(5);
                        }

                    }
                    //Make all text fit the cells
                    //ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Cells[ws.Dimension.Address].Style.Font.Bold = true;

                    //make the borders of cell F6 thick
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;

                    int modelNameCol = 1, modelNoCol = 2, articleNOCol = 3, Process = 4, qtyCol = 5, cbdCol = 7, realCol = 8;
                    ws.Column(articleNOCol).AutoFit(12);
                    ws.Column(Process).AutoFit(12);
                    ws.Column(modelNameCol).AutoFit(30);
                    ws.Column(modelNoCol).AutoFit(12);
                    ws.Column(qtyCol).AutoFit(8);
                    ws.Column(cbdCol).AutoFit(8);
                    ws.Column(realCol).AutoFit(10);

                    ws.Column(8).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Column(articleNOCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(Process).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(modelNoCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(qtyCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(cbdCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(realCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Row(1).Height = 40;
                    ws.Row(2).Height = 40;
                    //ws.Column(realCol).AutoFit(10);
                    var endMergeIndex = startIngredientCostingIndex - 1;
                    var titleRange = ws.Cells[1, 1, 2, endMergeIndex];
                    titleRange.Merge = true;
                    titleRange.Style.Font.Size = 22;
                    titleRange.Value = $"Consumption-Cost Breakdown Report {startDate.ToString("MM/dd/yyyy")}-{endDate.ToString("MM/dd/yyyy")}";
                    titleRange.Style.WrapText = true;
                    titleRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    titleRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    // freeze row and col
                    int rowCount = 5;
                    for (int i = 1; i < rowCount; i++)
                    {
                        ws.View.FreezePanes(i, modelNameCol);
                        ws.View.FreezePanes(i, modelNoCol);
                        ws.View.FreezePanes(i, articleNOCol);
                        ws.View.FreezePanes(i, Process);
                        ws.View.FreezePanes(i, qtyCol);
                        ws.View.FreezePanes(i, cbdCol);
                        ws.View.FreezePanes(i, realCol);
                        ws.View.FreezePanes(i, startIngredientCostingIndex + 1);
                    }

                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    return bin;
                }
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                Console.Write(mes);
                return new Byte[] { };
            }
        }

        private void SetStyleEachCell(ExcelRange excelRange)
        {
            excelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            excelRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            excelRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            excelRange.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#9bc2e6"));
        }

        double SumProduct(double[] arrayA, double[] arrayB)
        {
            double result = 0;
            for (int i = 0; i < arrayA.Count(); i++)
                result += arrayA[i] * arrayB[i];
            return result;
        }

        double CalculateGlueTotal(MixingInfo mixingInfo)
        {
            return mixingInfo.MixingInfoDetails.Select(x => x.Amount).Sum();
        }

        private Byte[] ExportExcelConsumptionCase2(List<ConsumtionDto> consumtionDtos)
        {
            try
            {
                consumtionDtos = consumtionDtos.OrderByDescending(x => x.DueDate).OrderBy(x => x.DueDate).ThenBy(x => x.Line).ThenBy(x => x.ID).ThenByDescending(x => x.Percentage).ToList();
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var memoryStream = new MemoryStream();
                using (ExcelPackage p = new ExcelPackage(memoryStream))
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "Henry Pham";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "ReportConsumption";
                    //Tạo một sheet để làm việc trên đó
                    p.Workbook.Worksheets.Add("ReportConsumption");

                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = p.Workbook.Worksheets["ReportConsumption"];

                    // đặt tên cho sheet
                    ws.Name = "ReportConsumption";
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 12;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";
                    var headers = new string[]{
                        "Line", "Model Name", "Model No.", "Article No.",
                        "Process", "Qty", "Glue", "Std.(g)", "Real Consumption(g)pr.", "Diff.", "%"
                    };

                    int headerRowIndex = 1;
                    int headerColIndex = 1;
                    foreach (var header in headers)
                    {
                        int col = headerRowIndex++;
                        ws.Cells[headerColIndex, col].Value = header;
                        ws.Cells[headerColIndex, col].Style.Font.Bold = true;
                        ws.Cells[headerColIndex, col].Style.Font.Size = 12;
                    }

                    // end Style
                    int colIndex = 1;
                    int rowIndex = 1;
                    // với mỗi item trong danh sách sẽ ghi trên 1 dòng
                    foreach (var body in consumtionDtos)
                    {
                        // bắt đầu ghi từ cột 1. Excel bắt đầu từ 1 không phải từ 0 #c0514d
                        colIndex = 1;

                        // rowIndex tương ứng từng dòng dữ liệu
                        rowIndex++;


                        //gán giá trị cho từng cell                      
                        ws.Cells[rowIndex, colIndex++].Value = body.Line;
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelName;
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelNo;
                        ws.Cells[rowIndex, colIndex++].Value = body.ArticleNo;
                        ws.Cells[rowIndex, colIndex++].Value = body.Process;
                        ws.Cells[rowIndex, colIndex++].Value = body.Qty;
                        ws.Cells[rowIndex, colIndex++].Value = body.Glue;
                        ws.Cells[rowIndex, colIndex++].Value = body.Std;
                        ws.Cells[rowIndex, colIndex++].Value = Math.Round(body.RealConsumption, 2);
                        ws.Cells[rowIndex, colIndex++].Value = body.Diff;
                        ws.Cells[rowIndex, colIndex++].Value = body.Percentage + "%";
                    }
                    int colPatternIndex = 1;
                    int rowPatternIndex = 1;

                    int colColorIndex = 1;
                    int rowColorIndex = 1;
                    foreach (var body in consumtionDtos)
                    {
                        rowColorIndex++;
                        rowPatternIndex++;

                        if (body.Percentage > 0)
                        {
                            colPatternIndex = 7;
                            colColorIndex = 7;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;

                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                        }
                    }
                    int mergeFromColIndex = 1;
                    int mergeToColIndex = 1;
                    int mergeFromRowIndex = 2;
                    int mergeToRowIndex = 1;
                    foreach (var item in consumtionDtos.GroupBy(x => new
                    {
                        x.ID,
                        x.Line,
                        x.ModelName,
                        x.ModelNo,
                        x.ArticleNo,
                        x.Process,
                        x.Qty
                    }))
                    {
                        mergeToRowIndex += item.Count();
                        ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeToColIndex].Merge = true;
                        ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeToColIndex].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeToColIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws.Cells[mergeFromRowIndex, 2, mergeToRowIndex, 2].Merge = true;
                        ws.Cells[mergeFromRowIndex, 2, mergeToRowIndex, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[mergeFromRowIndex, 2, mergeToRowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws.Cells[mergeFromRowIndex, 3, mergeToRowIndex, 3].Merge = true;
                        ws.Cells[mergeFromRowIndex, 3, mergeToRowIndex, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[mergeFromRowIndex, 3, mergeToRowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws.Cells[mergeFromRowIndex, 4, mergeToRowIndex, 4].Merge = true;
                        ws.Cells[mergeFromRowIndex, 4, mergeToRowIndex, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[mergeFromRowIndex, 4, mergeToRowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws.Cells[mergeFromRowIndex, 5, mergeToRowIndex, 5].Merge = true;
                        ws.Cells[mergeFromRowIndex, 5, mergeToRowIndex, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[mergeFromRowIndex, 5, mergeToRowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                        ws.Cells[mergeFromRowIndex, 6, mergeToRowIndex, 6].Merge = true;
                        ws.Cells[mergeFromRowIndex, 6, mergeToRowIndex, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[mergeFromRowIndex, 6, mergeToRowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        mergeFromRowIndex = mergeToRowIndex + 1;
                    }
                    //make the borders of cell F6 thick
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    foreach (var item in headers.Select((x, i) => new { Value = x, Index = i }))
                    {
                        var col = item.Index + 1;
                        ws.Column(col).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Column(col).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        if (col == 2 || col == 7)
                        {
                            ws.Column(col).AutoFit(30);
                        }
                        else
                        {
                            ws.Column(col).AutoFit();
                        }
                    }

                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    return bin;
                }
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                Console.Write(mes);
                return new Byte[] { };
            }
        }

        private Byte[] ExportExcelConsumptionCase1(List<ConsumtionDto> consumtionDtos)
        {
            try
            {
                consumtionDtos = consumtionDtos.OrderByDescending(x => x.Percentage).ToList();
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var memoryStream = new MemoryStream();
                using (ExcelPackage p = new ExcelPackage(memoryStream))
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "Henry Pham";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "ReportConsumption";
                    //Tạo một sheet để làm việc trên đó
                    p.Workbook.Worksheets.Add("ReportConsumption");

                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = p.Workbook.Worksheets["ReportConsumption"];

                    // đặt tên cho sheet
                    ws.Name = "ReportConsumption";
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 12;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";
                    var headers = new string[]{
                        "Model Name", "Model No.", "Article No.",
                        "Process", "Glue", "Std.(g)", "Glue Mixing Date", "Line", "Qty",
                        "Total Consumption(kg)", "Real Consumption(g)pr.", "Diff.", "%"
                    };

                    int headerRowIndex = 1;
                    int headerColIndex = 1;
                    foreach (var header in headers)
                    {
                        int col = headerRowIndex++;
                        ws.Cells[headerColIndex, col].Value = header;
                        ws.Cells[headerColIndex, col].Style.Font.Bold = true;
                        ws.Cells[headerColIndex, col].Style.Font.Size = 12;
                    }
                    // end Style
                    int colIndex = 1;
                    int rowIndex = 1;
                    // với mỗi item trong danh sách sẽ ghi trên 1 dòng
                    foreach (var body in consumtionDtos)
                    {
                        // bắt đầu ghi từ cột 1. Excel bắt đầu từ 1 không phải từ 0 #c0514d
                        colIndex = 1;

                        // rowIndex tương ứng từng dòng dữ liệu
                        rowIndex++;


                        //gán giá trị cho từng cell                      
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelName;
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelNo;
                        ws.Cells[rowIndex, colIndex++].Value = body.ArticleNo;
                        ws.Cells[rowIndex, colIndex++].Value = body.Process;
                        ws.Cells[rowIndex, colIndex++].Value = body.Glue;
                        ws.Cells[rowIndex, colIndex++].Value = body.Std;
                        ws.Cells[rowIndex, colIndex++].Value = body.MixingDate == DateTime.MinValue ? "N/A" : body.MixingDate.ToString("dd/MM/yyyy");
                        ws.Cells[rowIndex, colIndex++].Value = body.Line;
                        ws.Cells[rowIndex, colIndex++].Value = body.Qty;
                        ws.Cells[rowIndex, colIndex++].Value = Math.Round(body.TotalConsumption, 2);
                        ws.Cells[rowIndex, colIndex++].Value = Math.Round(body.RealConsumption, 2);
                        ws.Cells[rowIndex, colIndex++].Value = body.Diff;
                        ws.Cells[rowIndex, colIndex++].Value = body.Percentage + "%";
                    }

                    int colPatternIndex = 1;
                    int rowPatternIndex = 1;

                    int colColorIndex = 1;
                    int rowColorIndex = 1;
                    foreach (var body in consumtionDtos)
                    {
                        rowColorIndex++;
                        rowPatternIndex++;
                        colPatternIndex = 1;
                        colColorIndex = 1;
                        if (body.Percentage > 0)
                        {
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));

                        }
                    }

                    //make the borders of cell F6 thick
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    foreach (var item in headers.Select((x, i) => new { Value = x, Index = i }))
                    {
                        var col = item.Index + 1;
                        ws.Column(col).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Column(col).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        if (col == 5 || col == 1)
                        {
                            ws.Column(col).AutoFit(30);
                        }
                        else
                        {
                            ws.Column(col).AutoFit();
                        }
                    }
                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    return bin;
                }
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                Console.Write(mes);
                return new Byte[] { };
            }
        }

        private Byte[] ExportExcel(ReportHeaderDto header, List<ReportBodyDto> bodyList, List<IngredientReportDto> ingredients)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var memoryStream = new MemoryStream();
                using (ExcelPackage p = new ExcelPackage(memoryStream))
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "Henry Pham";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "Report";
                    //Tạo một sheet để làm việc trên đó
                    p.Workbook.Worksheets.Add("Report");

                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = p.Workbook.Worksheets["Report"];

                    // đặt tên cho sheet
                    ws.Name = "Report";
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 11;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";



                    int ingredientRealRowIndex = 1;
                    int ingredientCBDRowIndex = 2;
                    int startIngredientCostingIndex = 10;
                    int ingredientCBDColIndex = startIngredientCostingIndex;
                    int ingredientRealColIndex = startIngredientCostingIndex;

                    ws.Cells[ingredientRealRowIndex, ingredientRealColIndex++].Value = "REAL";
                    ws.Cells[ingredientCBDRowIndex, ingredientCBDColIndex++].Value = "CBD";

                    foreach (var ingredient in ingredients)
                    {
                        int cbdColumn = ingredientCBDColIndex++;
                        int realColumn = ingredientRealColIndex++;
                        ws.Cells[ingredientCBDRowIndex, cbdColumn].Value = ingredient.CBD;
                        ws.Cells[ingredientRealRowIndex, realColumn].Value = ingredient.Real;

                        ws.Cells[ingredientCBDRowIndex, cbdColumn].Style.TextRotation = 90;
                        ws.Cells[ingredientRealRowIndex, realColumn].Style.TextRotation = 90;
                        ws.Cells[ingredientCBDRowIndex, cbdColumn].AutoFitColumns(5);
                        ws.Cells[ingredientRealRowIndex, realColumn].AutoFitColumns(5);

                        ws.Cells[ingredientRealRowIndex, realColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[ingredientRealRowIndex, realColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        ws.Cells[ingredientCBDRowIndex, cbdColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[ingredientCBDRowIndex, cbdColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    int headerRowIndex = 3;
                    int headerColIndex = 1;

                    int patternTypeColIndex = 1;
                    int backgroundColorColIndex = 1;

                    for (headerColIndex = 1; headerColIndex < startIngredientCostingIndex; headerColIndex++)
                    {
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.Day;
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.Date;
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.ModelName;
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.ModelNo;
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.ArticleNO;
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.Process;

                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.Quantity;
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.Line;
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.CBD;
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.Real;
                        // Style Header
                        ws.Cells[headerRowIndex, patternTypeColIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[headerRowIndex, patternTypeColIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[headerRowIndex, patternTypeColIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[headerRowIndex, patternTypeColIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[headerRowIndex, patternTypeColIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[headerRowIndex, patternTypeColIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;

                        ws.Cells[headerRowIndex, backgroundColorColIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#4f81bd"));
                        ws.Cells[headerRowIndex, backgroundColorColIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#4f81bd"));
                        ws.Cells[headerRowIndex, backgroundColorColIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#4f81bd"));
                        ws.Cells[headerRowIndex, backgroundColorColIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#8db5e2"));
                        ws.Cells[headerRowIndex, backgroundColorColIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#ffffff"));
                        ws.Cells[headerRowIndex, backgroundColorColIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#9bbb59"));
                    }


                    // end Style
                    int ingredientColIndex = startIngredientCostingIndex + 1;
                    foreach (var ingredient in header.Ingredients)
                    {
                        int col = ingredientColIndex++;
                        ws.Cells[headerRowIndex, col].Value = ingredient;
                        ws.Cells[headerRowIndex, col].Style.Fill.PatternType = ExcelFillStyle.Solid;

                        ws.Cells[headerRowIndex, col].Style.TextRotation = 90;
                        ws.Cells[headerRowIndex, col].Style.Font.Color.SetColor(Color.White);
                        ws.Cells[headerRowIndex, col].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#808080"));

                    }
                    int colIndex = 1;
                    int rowIndex = 3;
                    // với mỗi item trong danh sách sẽ ghi trên 1 dòng
                    foreach (var body in bodyList)
                    {
                        // bắt đầu ghi từ cột 1. Excel bắt đầu từ 1 không phải từ 0 #c0514d
                        colIndex = 1;

                        // rowIndex tương ứng từng dòng dữ liệu
                        rowIndex++;


                        //gán giá trị cho từng cell                      
                        ws.Cells[rowIndex, colIndex++].Value = body.Day;
                        ws.Cells[rowIndex, colIndex++].Value = body.Date.ToString("M/d");
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelName;
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelNo;
                        ws.Cells[rowIndex, colIndex++].Value = body.ArticleNO;
                        ws.Cells[rowIndex, colIndex++].Value = body.Process;
                        ws.Cells[rowIndex, colIndex++].Value = body.Quantity == 0 ? string.Empty : body.Quantity.ToString();
                        ws.Cells[rowIndex, colIndex++].Value = body.Line;

                        var cbds = ingredients.Select(x => x.CBD).ToArray();
                        var reals = ingredients.Select(x => x.Real).ToArray();

                        var cbdRowTotal = body.Ingredients.ToArray();
                        var realRowTotal = body.Ingredients.ToArray();
                        var value = body.Ingredients.Sum();
                        double CBD = 0, real = 0;

                        if (value > 0 && body.Quantity > 0)
                            CBD = Math.Round(SumProduct(cbdRowTotal, cbds) / body.Quantity, 3, MidpointRounding.AwayFromZero);
                        if (value > 0 && body.Quantity > 0)
                            real = Math.Round(SumProduct(realRowTotal, reals) / body.Quantity, 3, MidpointRounding.AwayFromZero);

                        ws.Cells[rowIndex, colIndex++].Value = CBD == 0 ? string.Empty : CBD.ToString();
                        ws.Cells[rowIndex, colIndex++].Value = real == 0 ? string.Empty : real.ToString();

                        foreach (var ingredient in body.Ingredients)
                        {
                            int col = colIndex++;
                            ws.Cells[rowIndex, col].Value = ingredient > 0 ? Math.Round(ingredient, 2, MidpointRounding.AwayFromZero).ToString() : string.Empty;
                            ws.Cells[rowIndex, col].Style.Font.Size = 8;
                            ws.Cells[rowIndex, col].Style.Font.Color.SetColor(Color.DarkRed);
                            ws.Cells[rowIndex, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[rowIndex, col].AutoFitColumns(5);
                        }

                    }
                    int mergeFromColIndex = 1;
                    int mergeFromRowIndex = 4;
                    int mergeToRowIndex = 3;
                    foreach (var item in bodyList.GroupBy(x => x.Day))
                    {
                        mergeToRowIndex += item.Count();

                        ws.Cells[mergeFromRowIndex, 6, mergeToRowIndex, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[mergeFromRowIndex, 6, mergeToRowIndex, 8].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#9bbb59"));


                        ws.Cells[mergeFromRowIndex, 2, mergeFromRowIndex, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[mergeFromRowIndex, 2, mergeFromRowIndex, 8].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#c0514d"));

                        ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeFromColIndex].Merge = true;
                        ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeFromColIndex].Style.Font.Size = 36;
                        ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeFromColIndex].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeFromColIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        mergeFromRowIndex = mergeToRowIndex + 1;
                    }
                    //Make all text fit the cells
                    //ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Cells[ws.Dimension.Address].Style.Font.Bold = true;

                    //make the borders of cell F6 thick
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;

                    int dayCol = 1, dateCol = 2, modelNameCol = 3, modelNoCol = 4, qtyCol = 5, lineCol = 6, cbdCol = 7, realCol = 8;
                    ws.Column(dayCol).AutoFit(12);
                    ws.Column(dateCol).AutoFit(12);
                    ws.Column(modelNameCol).AutoFit(30);
                    ws.Column(modelNoCol).AutoFit(12);
                    ws.Column(qtyCol).AutoFit(8);
                    ws.Column(lineCol).AutoFit(8);
                    ws.Column(cbdCol).AutoFit(8);
                    ws.Column(realCol).AutoFit(10);

                    ws.Column(8).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Column(dayCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(dateCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(modelNoCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(qtyCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(lineCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(cbdCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(realCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Row(1).Height = 40;
                    ws.Row(2).Height = 40;
                    //ws.Column(realCol).AutoFit(10);
                    var endMergeIndex = startIngredientCostingIndex - 1;
                    var mergeRangeTitle = ws.Cells[1, 1, 2, endMergeIndex];
                    mergeRangeTitle.Merge = true;
                    mergeRangeTitle.Style.Font.Size = 22;
                    mergeRangeTitle.Value = "Consumption-Cost Breakdown Report";
                    mergeRangeTitle.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    mergeRangeTitle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    mergeRangeTitle.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    mergeRangeTitle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    // freeze row and col
                    int rowCount = 5;
                    for (int i = 1; i < rowCount; i++)
                    {
                        ws.View.FreezePanes(i, dayCol);
                        ws.View.FreezePanes(i, dateCol);
                        ws.View.FreezePanes(i, modelNameCol);
                        ws.View.FreezePanes(i, modelNoCol);
                        ws.View.FreezePanes(i, qtyCol);
                        ws.View.FreezePanes(i, lineCol);
                        ws.View.FreezePanes(i, cbdCol);
                        ws.View.FreezePanes(i, realCol);
                        ws.View.FreezePanes(i, startIngredientCostingIndex + 1);
                    }

                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    return bin;
                }
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                Console.Write(mes);
                return new Byte[] { };
            }
        }

        private double FindPercentageByPosition(List<GlueIngredient> glueIngredients, string position)
        {
            var glueIngredient = glueIngredients.FirstOrDefault(x => x.Position == position);

            return glueIngredient == null ? 0 : glueIngredient.Percentage;
        }

        public async Task<ResponseDetail<Byte[]>> ExportExcelWorkPlanWholeBuilding(int buildingID, DateTime startDate, DateTime endDate)
        {
            var buildingModel = await _repoBuilding.FindAll().FirstOrDefaultAsync(x => x.ID == buildingID);
            var _buildings = await _repoBuilding.FindAll().Where(x => x.BuildingTypeID == buildingModel.BuildingTypeID).OrderBy(x => x.Name).ToListAsync();
            var data = new List<ExportExcelPlanDto>();
            foreach (var building in _buildings)
            {
                var lines = _repoBuilding.FindAll(x => building.ID == x.ParentID.Value).Select(a => a.ID).ToList();
                var plans = await _repoPlan.FindAll()
                 .Where(x => lines.Contains(x.BuildingID) && x.DueDate >= startDate && x.DueDate <= endDate)
                 .Include(x => x.BPFCEstablish)
                     .ThenInclude(x => x.ModelName)
                 .Include(x => x.BPFCEstablish)
                     .ThenInclude(x => x.ModelNo)
                 .Include(x => x.BPFCEstablish)
                     .ThenInclude(x => x.ArticleNo)
                 .Select(x => new ExportExcelPlanDto
                 {
                     ModelName = x.BPFCEstablish.ModelName.Name,
                     ModelNo = x.BPFCEstablish.ModelNo.Name,
                     ArticleNO = x.BPFCEstablish.ArticleNo.Name,
                     Line = x.Building.Name,
                     DueDate = x.DueDate,
                     CreatedDate = x.CreatedDate
                 }).OrderBy(x => x.Line)
                 .ThenBy(x => x.CreatedDate)
                 .ToListAsync();
                foreach (var item in plans)
                {
                    item.Building = building.Name;
                }
                data.AddRange(plans);
            }

            var groupBy = data.GroupBy(g => g.Building).ToList();
            try
            {
                var currentDateTime = DateTime.Now;
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var memoryStream = new MemoryStream();
                using (ExcelPackage p = new ExcelPackage(memoryStream))
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "Henry Pham";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = $"{currentDateTime.ToString("YYYYMMdd")}_Workplan";

                    if (groupBy.Count == 0) return null;
                    foreach (var groupbyItem in groupBy)
                    {
                        var sheet = groupbyItem.Key;
                        //Tạo một sheet để làm việc trên đó
                        p.Workbook.Worksheets.Add(sheet);
                        // lấy sheet vừa add ra để thao tác
                        ExcelWorksheet ws = p.Workbook.Worksheets[sheet];

                        // đặt tên cho sheet
                        ws.Name = sheet;
                        // fontsize mặc định cho cả sheet
                        ws.Cells.Style.Font.Size = 11;
                        // font family mặc định cho cả sheet
                        ws.Cells.Style.Font.Name = "Calibri";
                        string[] headers = new string[] { "Model Name", "Model NO", "Article NO", "Line", "Due Date" };
                        int headerRowIndex = 1;
                        int headerColIndex = 1;
                        int patternTypeColIndex = 1;
                        // int backgroundColorColIndex = 1;
                        foreach (var item in headers.Select((value, index) => new { value, index }))
                        {
                            ws.Cells[headerRowIndex, headerColIndex++].Value = headers[item.index];

                            // Style Header
                            var pattern = ws.Cells[headerRowIndex, patternTypeColIndex++];
                            pattern.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            pattern.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF00"));
                        }

                        // end Style

                        int colIndex = 1;
                        int rowIndex = 1;
                        // với mỗi item trong danh sách sẽ ghi trên 1 dòng
                        foreach (var body in groupbyItem.OrderBy(x=> x.DueDate))
                        {
                            // bắt đầu ghi từ cột 1. Excel bắt đầu từ 1 không phải từ 0 #c0514d
                            colIndex = 1;

                            // rowIndex tương ứng từng dòng dữ liệu
                            rowIndex++;


                            //gán giá trị cho từng cell                      
                            ws.Cells[rowIndex, colIndex++].Value = body.ModelName;
                            ws.Cells[rowIndex, colIndex++].Value = body.ModelNo;
                            ws.Cells[rowIndex, colIndex++].Value = body.ArticleNO;
                            ws.Cells[rowIndex, colIndex++].Value = body.Line;
                            ws.Cells[rowIndex, colIndex++].Value = body.DueDate != null ? body.DueDate.ToString("MM/dd/yyyy") : "N/A";
                        }

                    }

                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    return new ResponseDetail<Byte[]>(bin, true, string.Empty);
                }
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                Console.Write(mes);
                return new ResponseDetail<Byte[]>(new Byte[] { }, false, string.Empty);
            }
        }
        public async Task<ResponseDetail<Byte[]>> ExportExcel(ExcelExportDto dto)
        {
            var plans = await _repoPlan.FindAll()
                .Where(x => dto.Plans.Contains(x.ID))
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelName)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelNo)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArticleNo)
                .Select(x => new
                {
                    Glues = x.BPFCEstablish.Glues.Where(x => x.isShow),
                    GlueIngredients = x.BPFCEstablish.Glues.Where(x => x.isShow).SelectMany(x => x.GlueIngredients),
                    ModelName = x.BPFCEstablish.ModelName.Name,
                    ModelNo = x.BPFCEstablish.ModelNo.Name,
                    ArticleNO = x.BPFCEstablish.ArticleNo.Name,
                    Line = x.Building.Name,
                    x.DueDate,
                    x.CreatedDate
                }).OrderBy(x => x.Line)
                .ThenBy(x => x.CreatedDate)
                .ToListAsync();

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var memoryStream = new MemoryStream();
                using (ExcelPackage p = new ExcelPackage(memoryStream))
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "Henry Pham";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "Report";
                    //Tạo một sheet để làm việc trên đó
                    p.Workbook.Worksheets.Add("Report");

                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = p.Workbook.Worksheets["Report"];

                    // đặt tên cho sheet
                    ws.Name = "Report";
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 11;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";
                    string[] headers = new string[] { "Model Name", "Model NO", "Article NO", "Line", "Due Date" };
                    int headerRowIndex = 1;
                    int headerColIndex = 1;
                    int patternTypeColIndex = 1;
                    // int backgroundColorColIndex = 1;
                    foreach (var item in headers.Select((value, index) => new { value, index }))
                    {
                        ws.Cells[headerRowIndex, headerColIndex++].Value = headers[item.index];

                        // Style Header
                        var pattern = ws.Cells[headerRowIndex, patternTypeColIndex++];
                        pattern.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        pattern.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF00"));
                    }


                    // end Style

                    int colIndex = 1;
                    int rowIndex = 1;
                    // với mỗi item trong danh sách sẽ ghi trên 1 dòng
                    foreach (var body in plans)
                    {
                        // bắt đầu ghi từ cột 1. Excel bắt đầu từ 1 không phải từ 0 #c0514d
                        colIndex = 1;

                        // rowIndex tương ứng từng dòng dữ liệu
                        rowIndex++;


                        //gán giá trị cho từng cell                      
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelName;
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelNo;
                        ws.Cells[rowIndex, colIndex++].Value = body.ArticleNO;
                        ws.Cells[rowIndex, colIndex++].Value = body.Line;
                        ws.Cells[rowIndex, colIndex++].Value = body.DueDate != null ? body.DueDate.ToString("MM/dd/yyyy") : "N/A";
                    }

                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    return new ResponseDetail<Byte[]>(bin, true, string.Empty);
                }
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                Console.Write(mes);
                return new ResponseDetail<Byte[]>(new Byte[] { }, false, string.Empty);
            }
        }

        #endregion

        #region TroubleShooting

        public async Task<object> GetBatchByIngredientID(int ingredientID)
        {
            try
            {
                var item = (await _repoIngredientInfo.FindAll().Where(x => x.IngredientID == ingredientID).ToListAsync()).Select(x => new BatchDto
                {
                    ID = x.ID,
                    BatchName = x.Batch
                }).DistinctBy(x => x.BatchName);

                return item;
            }
            catch
            {
                throw;
            }

        }

        public async Task<object> TroubleShootingSearch(string value, string batchValue)
        {
            try
            {
                var ingredientName = value.ToSafetyString();
                var from = DateTime.Now.Date.AddDays(-3).Date;
                var to = DateTime.Now.Date.Date;
                var plans = _repoPlan.FindAll()
                    .Include(x => x.Building)
                    .Include(x => x.BPFCEstablish)
                        .ThenInclude(x => x.Glues)
                        .ThenInclude(x => x.GlueIngredients)
                        .ThenInclude(x => x.Ingredient)
                    .Include(x => x.BPFCEstablish)
                        .ThenInclude(x => x.ModelName)
                    .Include(x => x.BPFCEstablish)
                        .ThenInclude(x => x.ModelNo)
                    .Include(x => x.BPFCEstablish)
                        .ThenInclude(x => x.ArticleNo)
                     .Include(x => x.BPFCEstablish)
                        .ThenInclude(x => x.ArtProcess)
                        .ThenInclude(x => x.Process)
                    .Where(x => x.DueDate.Date >= from && x.DueDate.Date <= to && !x.BPFCEstablish.IsDelete)
                    .Select(x => new
                    {
                        x.BPFCEstablish.Glues,
                        ModelName = x.BPFCEstablish.ModelName.Name,
                        ModelNo = x.BPFCEstablish.ModelNo.Name,
                        ArticleNo = x.BPFCEstablish.ArticleNo.Name,
                        Process = x.BPFCEstablish.ArtProcess.Process.Name,
                        Line = x.Building.Name,
                        LineID = x.Building.ID,
                        x.DueDate
                    });
                var troubleshootings = new List<TroubleshootingDto>();

                foreach (var plan in plans)
                {
                    // lap nhung bpfc chua ingredient search
                    foreach (var glue in plan.Glues.Where(x => x.isShow == true))
                    {
                        foreach (var item in glue.GlueIngredients.Where(x => x.Ingredient.Name.Trim().Contains(ingredientName)))
                        {
                            var buildingGlue = await _repoDispatch.FindAll().Where(x => x.LineID == plan.LineID && x.CreatedTime.Date == plan.DueDate.Date).OrderByDescending(x => x.CreatedTime).FirstOrDefaultAsync();
                            var mixingID = 0;
                            if (buildingGlue != null)
                            {
                                mixingID = buildingGlue.MixingInfoID;
                            }
                            var mixingInfo = _repoMixingInfo.FindAll(x => x.ID == mixingID).Include(x => x.MixingInfoDetails).FirstOrDefault();
                            var batch = "";
                            var mixDate = new DateTime();
                            if (mixingInfo != null)
                            {
                                var mixingInfoDetail = mixingInfo.MixingInfoDetails.FirstOrDefault(x => x.IngredientID == item.IngredientID && x.Position == item.Position);

                                batch = mixingInfoDetail is null ? "" : mixingInfoDetail.Batch;
                                mixDate = mixingInfo.CreatedTime;
                            }
                            var detail = new TroubleshootingDto
                            {
                                Ingredient = item.Ingredient.Name,
                                GlueName = item.Glue.Name,
                                ModelName = plan.ModelName,
                                ModelNo = plan.ModelNo,
                                ArticleNo = plan.ArticleNo,
                                Process = plan.Process,
                                Line = plan.Line,
                                DueDate = plan.DueDate.Date,
                                Batch = batch,
                                MixDate = mixDate
                            };
                            troubleshootings.Add(detail);
                        }
                    }
                }
                return troubleshootings.Where(x => x.Batch.Equals(batchValue)).OrderByDescending(x => x.MixDate).DistinctBy(x => x.Line).ToList();
            }
            catch
            {
                return new List<TroubleshootingDto>();
            }
        }

        public async Task<ResponseDetail<object>> AchievementRate(int building)
        {
            var today = DateTime.Now.Date;
            var tomorrow = DateTime.Now.AddDays(1).Date;
            var lines = await _repoBuilding.FindAll(x => x.ParentID == building).ToListAsync();
            var linesID = lines.Select(x => x.ID);
            var model = _repoPlan.FindAll(x => linesID.Contains(x.BuildingID) 
             && x.DueDate.Date == tomorrow)
            .Include(x => x.ToDoList)
            .Include(x => x.DispatchList)
            .DistinctBy(x => x.BuildingID)
            .ToList();

            var lineTotal = lines.Count();
            var planTotal = model.Where(x=> x.ToDoList.Count() > 0 || x.DispatchList.Count() > 0
                                         && x.UpdatedTime.Value.Date == today 
                                         && x.CreatedDate.Date == today).Count();
            var rateTemp = planTotal != 0 && lineTotal != 0 ? (planTotal / (double)lineTotal) * 100 : 0;
            var rate = Math.Round(rateTemp);
            var data = new
            {
                Text = $"{planTotal}/{lineTotal}    {rate}%",
                UpdateOnTime = planTotal,
                Total = lineTotal,
                Rate = rate
            };
            return new ResponseDetail<object> { Data = data, Status = true };
        }
        public async Task<ResponseDetail<Byte[]>> AchievementRateExcelExport()
        {
            var today = DateTime.Now.Date;
            var tomorrow = DateTime.Now.AddDays(1).Date;
            var buildings = await _repoBuilding.FindAll(x => x.Level == 2).ToListAsync();
            var data = new List<AchievementDto>();

            foreach (var item in buildings)
            {
                var lines = await _repoBuilding.FindAll(x => x.ParentID == item.ID).ToListAsync();
                var linesID = lines.Select(x => x.ID);
                var model = _repoPlan.FindAll(x => linesID.Contains(x.BuildingID)
                 && x.DueDate.Date == tomorrow)
                .Include(x => x.ToDoList)
                .Include(x => x.DispatchList)
                .DistinctBy(x => x.BuildingID)
                .ToList();

                var lineTotal = lines.Count();
                var planTotal = model.Where(x => x.ToDoList.Count() > 0 || x.DispatchList.Count() > 0
                                             && x.UpdatedTime.Value.Date == today
                                             && x.CreatedDate.Date == today).Count();
                var rateTemp = planTotal != 0 && lineTotal != 0 ? (planTotal / (double)lineTotal) * 100 : 0;
                var rate = Math.Round(rateTemp);
                data.Add(new AchievementDto
                { 
                    Building = item.Name,
                    UpdateOnTime = planTotal, // Update Before 18:00
                    Total = lineTotal,
                    AchievementRate = rate
                });
                
            }
            var UpdateOnTimeTotal = data.Select(x => x.UpdateOnTime).Sum();
            var planTotal2 = data.Select(x => x.Total).Sum();
            var all = new AchievementDto
            {
                Building = "All",
                UpdateOnTime = UpdateOnTimeTotal, // Update Before 18:00
                Total = planTotal2,
                AchievementRate = planTotal2 != 0 && UpdateOnTimeTotal != 0 ? Math.Round((UpdateOnTimeTotal / (double)planTotal2) * 100) : 0
            };
            data.Add(all);
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var memoryStream = new MemoryStream();
                using (ExcelPackage p = new ExcelPackage(memoryStream))
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "Henry Pham";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "AchievementReport";
                    //Tạo một sheet để làm việc trên đó
                    p.Workbook.Worksheets.Add("AchievementReport");

                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = p.Workbook.Worksheets["AchievementReport"];

                    // đặt tên cho sheet
                    ws.Name = "AchievementReport";
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 11;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";
                    string[] headers = new string[] { "Building", "Update befor 18:00", "Total", "Achievement %" };
                    int headerRowIndex = 1;
                    int headerColIndex = 1;
                    int patternTypeColIndex = 1;
                    // int backgroundColorColIndex = 1;
                    foreach (var item in headers.Select((value, index) => new { value, index }))
                    {
                        ws.Cells[headerRowIndex, headerColIndex++].Value = headers[item.index];

                        // Style Header
                        var pattern = ws.Cells[headerRowIndex, patternTypeColIndex++];
                        pattern.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        pattern.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF00"));
                    }


                    // end Style

                    int colIndex = 1;
                    int rowIndex = 1;
                    // với mỗi item trong danh sách sẽ ghi trên 1 dòng
                    foreach (var body in data)
                    {
                        // bắt đầu ghi từ cột 1. Excel bắt đầu từ 1 không phải từ 0 #c0514d
                        colIndex = 1;

                        // rowIndex tương ứng từng dòng dữ liệu
                        rowIndex++;


                        //gán giá trị cho từng cell                      
                        ws.Cells[rowIndex, colIndex++].Value = body.Building;
                        ws.Cells[rowIndex, colIndex++].Value = body.UpdateOnTime;
                        ws.Cells[rowIndex, colIndex++].Value = body.Total;
                        ws.Cells[rowIndex, colIndex++].Value = body.AchievementRate + " %";
                    }

                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    return new ResponseDetail<Byte[]>(bin, true, string.Empty);
                }
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                Console.Write(mes);
                return new ResponseDetail<Byte[]>(new Byte[] { }, false, string.Empty);
            }
        }

        private async Task<List<PlanDto>> GetAllPlanToDay(int buildingTypeID)
        {
            var _buildings = await _repoBuilding.FindAll().Where(x => x.BuildingTypeID == buildingTypeID).ToListAsync();
            var lines = _repoBuilding.FindAll(x => _buildings.Select(a => a.ID).Contains(x.ParentID.Value)).Select(a => a.ID).ToList();
            var model = await _repoPlan.FindAll()
              .Where(x => x.DueDate.Date == DateTime.Now.Date && lines.Contains(x.BuildingID))
              .Include(x => x.Building)
              .Include(x => x.ToDoList)
              .Include(x => x.DispatchList)
              .Include(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ModelName)
              .Include(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ModelNo)
              .Include(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ArticleNo)
              .Include(x => x.BPFCEstablish)
                  .ThenInclude(x => x.ArtProcess)
                  .ThenInclude(x => x.Process)
              .ProjectTo<PlanDto>(_configMapper)
              .OrderByDescending(x => x.BuildingName)
              .ToListAsync();
            return model;
        }
        #endregion

    }
}