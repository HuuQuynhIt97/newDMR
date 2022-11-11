using AutoMapper;
using AutoMapper.QueryableExtensions;
using dmr_api.Models;
using DMR_API._Repositories.Interface;
using DMR_API._Services.Interface;
using DMR_API.Constants;
using DMR_API.DTO;
using DMR_API.Enums;
using DMR_API.Helpers;
using DMR_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace DMR_API._Services.Services
{
    public class BottomFactoryService : IBottomFactoryService
    {
        private readonly IToDoListRepository _repoToDoList;
        private readonly IHttpContextAccessor _accessor;
        private readonly IJWTService _jwtService;
        private readonly IBuildingRepository _repoBuilding;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IIngredientRepository _ingredientRepository;
        private readonly IBuildingUserRepository _repoBuildingUser;
        private readonly IDispatchRepository _repoDispatch;
        private readonly IPlanRepository _repoPlan;
        private readonly IGlueRepository _repoGlue;
        private readonly ISubpackageCapacityRepository _repoSubpackageCapacity;
        private readonly IMixingInfoRepository _mixingInfoRepository;
        private readonly IMixingInfoDetailRepository _mixingInfoDetailRepository;
        private readonly ISubpackageRepository _subpackageRepository;
        private readonly IDispatchListRepository _repoDispatchList;
        private readonly IDispatchListDetailRepository _repoDispatchListDetail;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;

        public BottomFactoryService(
            IToDoListRepository repoToDoList,
            IHttpContextAccessor accessor,
            IJWTService jwtService,
            IBuildingRepository repoBuilding,
            IUserRoleRepository userRoleRepository,
            IIngredientRepository ingredientRepository,
            IBuildingUserRepository repoBuildingUser,
            IDispatchRepository repoDispatch,
            IPlanRepository repoPlan,
            IGlueRepository repoGlue,
            ISubpackageCapacityRepository repoSubpackageCapacity,
            IMixingInfoRepository mixingInfoRepository,
            IMixingInfoDetailRepository mixingInfoDetailRepository,
            ISubpackageRepository subpackageRepository,
            IDispatchListRepository repoDispatchList,
            IDispatchListDetailRepository repoDispatchListDetail,
            IMapper mapper,
            MapperConfiguration configMapper
            )
        {
            _repoToDoList = repoToDoList;
            _accessor = accessor;
            _jwtService = jwtService;
            _repoBuilding = repoBuilding;
            _userRoleRepository = userRoleRepository;
            _ingredientRepository = ingredientRepository;
            _repoBuildingUser = repoBuildingUser;
            _repoDispatch = repoDispatch;
            _repoPlan = repoPlan;
            _repoGlue = repoGlue;
            _repoSubpackageCapacity = repoSubpackageCapacity;
            _mixingInfoRepository = mixingInfoRepository;
            _mixingInfoDetailRepository = mixingInfoDetailRepository;
            _subpackageRepository = subpackageRepository;
            _repoDispatchList = repoDispatchList;
            _repoDispatchListDetail = repoDispatchListDetail;
            _mapper = mapper;
            _configMapper = configMapper;
        }

        public async Task<ResponseDetail<object>> AddDispatch(AddDispatchParams bfparams)
        {
            var userID = _jwtService.GetUserID();
            var line = await _repoBuilding.FindAll(x => x.Name == bfparams.LineName).FirstOrDefaultAsync();
            if (line == null)
            {
                return new ResponseDetail<object>("", false, $"Tên chuyền {bfparams.LineName} không có trong hệ thống!");
            }
            var subpackage = await _subpackageRepository.FindAll(x => x.MixingInfoID == bfparams.MixingInfoID).OrderByDescending(x => x.CreatedTime).FirstOrDefaultAsync();
            if (subpackage == null)
            {
                return new ResponseDetail<object>("", false, "Vui lòng chia keo trước!");
            }
            // Chon Default
            if (bfparams.Option == "Default")
            {
                var dispatch = await _repoDispatch.FindAll(x => x.MixingInfoID == bfparams.MixingInfoID)
                    .OrderByDescending(x => x.CreatedTime)
                    .ToListAsync();
                if (dispatch.Count == 0)
                {
                    var dispatchModel = new Dispatch
                    {
                        CreatedTime = DateTime.Now,
                        CreatedBy = userID,
                        Amount = subpackage.Amount,
                        LineID = line.ID,
                        GlueNameID = bfparams.GlueNameID,
                        MixingInfoID = bfparams.MixingInfoID,
                        EstimatedStartTime = bfparams.EstimatedStartTime,
                        EstimatedFinishTime = bfparams.EstimatedFinishTime

                    };
                    _repoDispatch.Add(dispatchModel);
                    await _repoDispatch.SaveAll();
                    return new ResponseDetail<object>("", true, "Tạo Thành Công!");
                }
                else
                {
                    var lastAmount = dispatch.First().RemainingAmount;
                    if (lastAmount == null)
                        return new ResponseDetail<object>("", false, "Vui lòng giao keo trước!");
                    if (lastAmount == 0)
                    {
                        var dispatchModel = new Dispatch
                        {
                            CreatedTime = DateTime.Now,
                            CreatedBy = userID,
                            GlueNameID = bfparams.GlueNameID,
                            LineID = line.ID,

                            Amount = subpackage.Amount,
                            MixingInfoID = bfparams.MixingInfoID,
                            EstimatedStartTime = bfparams.EstimatedStartTime,
                            EstimatedFinishTime = bfparams.EstimatedFinishTime
                        };
                        _repoDispatch.Add(dispatchModel);
                        await _repoDispatch.SaveAll();
                        return new ResponseDetail<object>("", true, "Tạo Thành Công!");
                    }
                    else
                    {
                        var dispatchModel = new Dispatch
                        {
                            CreatedTime = DateTime.Now,
                            CreatedBy = userID,
                            GlueNameID = bfparams.GlueNameID,
                            LineID = line.ID,

                            Amount = dispatch.First().RemainingAmount.Value,
                            MixingInfoID = bfparams.MixingInfoID,
                            EstimatedStartTime = bfparams.EstimatedStartTime,
                            EstimatedFinishTime = bfparams.EstimatedFinishTime
                        };
                        _repoDispatch.Add(dispatchModel);
                        await _repoDispatch.SaveAll();
                        return new ResponseDetail<object>("", true, "Tạo Thành Công!");
                    }
                }

            }
            else // Chon Reset
            {
                var dispatchModel = new Dispatch
                {
                    CreatedTime = DateTime.Now,
                    CreatedBy = userID,
                    LineID = line.ID,
                    Amount = subpackage.Amount,
                    GlueNameID = bfparams.GlueNameID,
                    MixingInfoID = bfparams.MixingInfoID,
                };
                _repoDispatch.Add(dispatchModel);
                await _repoDispatch.SaveAll();
                return new ResponseDetail<object>("", true, "Tạo Thành Công!");
            }
        }

        public async Task<ToDoListForReturnDto> DelayList(int buildingID)
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

                var EVA_UVTotal = dispatchListResult.Where(x => x.IsEVA_UV).Count();
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

        public async Task<ToDoListForReturnDto> DoneList(int buildingID)
        {
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

            var EVA_UVTotal = dispatchListResult.Where(x => x.IsEVA_UV).Count();
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

            var response = new ToDoListForReturnDto();
            int jobTypeOfTodo = 1, jobTypeOfDispatch = 2;
            var data = doneList.Concat(dispatchListResult).Where(x => x.PrintTime != null && x.JobType == jobTypeOfTodo || x.FinishDispatchingTime != null && x.JobType == jobTypeOfDispatch).ToList();
            var recaculatetotal = todoTotal + delayTotal + doneTotal + todoDispatchTotal + delayDispatchTotal + doneDispatchTotal;
            response.TodoDetail(data, doneTotal + doneDispatchTotal, todoTotal, delayTotal, recaculatetotal);
            response.DispatcherDetail(data, 0, todoDispatchTotal, delayDispatchTotal, dispatchTotal);

            return response;

        }

        public async Task<ToDoListForReturnDto> EVA_UVList(int buildingID)
        {
            var userID = _jwtService.GetUserID();
            var role = await _userRoleRepository.FindAll(x => x.UserID == userID).FirstOrDefaultAsync();
            var currentTime = DateTime.Now.ToLocalTime();
            currentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, 0);
            var currentDate = currentTime.Date;
            var start = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 00, 00);
            var model = await _repoToDoList.FindAll(x =>
                   x.IsDelete == false
                   && x.IsEVA_UV
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
            // map dto
            var result = MapToTodolistEVA_UVDto(model);

            // caculate

            var total = result.Count();
            var doneTotal = result.Where(x => x.PrintTime != null).Count();
            var todoTotal = result.Where(x => x.PrintTime is null
                                            && x.EstimatedFinishTime.TimeOfDay >= currentTime.TimeOfDay
                                         ).Count();

            var delayTotal = result.Where(x => x.PrintTime is null
                                                && x.EstimatedFinishTime.TimeOfDay < currentTime.TimeOfDay
                                                ).Count();


            var EVA_UVTotal = result.Where(x => x.IsEVA_UV).Count();
            var response = new ToDoListForReturnDto();
            var data = result.Where(x => x.PrintTime is null).ToList();
            var dispatchListModel = await _repoDispatchList.FindAll(x =>
                                                            x.IsDelete == false
                                                            && x.EstimatedStartTime.Date == currentDate
                                                            && x.EstimatedFinishTime.Date == currentDate
                                                            && x.BuildingID == buildingID).ToListAsync();

            // map to dto
            var dispatchList = MapToDispatchListDto(dispatchListModel);

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
        }

        private async Task<string> GenatateCode(string code)
        {
            int lenght = 8;
            if (code.IsNullOrEmpty())
            {
                code = CodeUtility.RandomString(lenght);
            }
            if (await _subpackageRepository.FindAll().AnyAsync(x => x.Code.Equals(code)) == true)
            {
                var newCode = CodeUtility.RandomString(lenght);
                return await GenatateCode(newCode);
            }
            return code;

        }
        private async Task<string> GenatateCodeForMixingInfo(string code)
        {
            int lenght = 8;
            if (code.IsNullOrEmpty())
            {
                code = CodeUtility.RandomString(lenght);
            }
            if (await _mixingInfoRepository.FindAll().AnyAsync(x => x.Code.Equals(code)) == true)
            {
                var newCode = CodeUtility.RandomString(lenght);
                return await GenatateCodeForMixingInfo(newCode);
            }
            return code;

        }

        public async Task<ResponseDetail<List<SubpackageDto>>> GenerateScanByNumber(GenerateSubpackageParams obj)
        {
            var userID = _jwtService.GetUserID();
            var subpackageList = new List<Subpackage>();
            var capacity = _repoSubpackageCapacity.FindAll().FirstOrDefault();
            var mixingInfo = _mixingInfoRepository.FindAll(x => x.ID == obj.MixingInfoID).Include(x => x.MixingInfoDetails).FirstOrDefault();
            var detail = mixingInfo.MixingInfoDetails.FirstOrDefault();
            var batch = detail.Batch;
            var subpackages = _subpackageRepository.FindAll(x => x.Code.Contains(batch)).OrderByDescending(x => x.Position).FirstOrDefault();
            var position = subpackages == null ? 0 : subpackages.Position;
            for (int i = 0; i <= obj.Can - 1; i++)
            {
                position = position + 1;
                var subpackage = new Subpackage
                {
                    Code = $"{mixingInfo.Code}_{batch}_{position}",
                    GlueName = obj.GlueName,
                    Name = $"{mixingInfo.GlueName}_{position}",
                    GlueNameID = obj.GlueNameID,
                    Position = position,
                    MixingInfoID = obj.MixingInfoID,
                    CreatedBy = userID,
                    CreatedTime = DateTime.Now,
                    Amount = capacity.Capacity
                };
                subpackageList.Add(subpackage);
            }
            _subpackageRepository.AddRange(subpackageList);
            try
            {
                await _subpackageRepository.SaveAll();
                var data = subpackageList.AsQueryable().ProjectTo<SubpackageDto>(_configMapper).ToList();
                return new ResponseDetail<List<SubpackageDto>>(data, true, "Thành công!");
            }
            catch (Exception ex)
            {
                await _subpackageRepository.SaveAll();
                return new ResponseDetail<List<SubpackageDto>>(null, false, $"Thất bại! {ex.Message}");
            }
        }

        public async Task<ResponseDetail<BottomFactoryForReturnDto>> ScanQRCode(ScanQRCodeParams scanQRCodeParams)
        {
            var mixBy = _jwtService.GetUserID();
            //if (scanQRCodeParams.MixingInfoID > 0)
            //{
            //    var mixingInfoModel = await _mixingInfoRepository.FindAll(x => x.ID == scanQRCodeParams.MixingInfoID)
            //          .Include(x => x.MixingInfoDetails)
            //          .ThenInclude(x => x.Ingredient)
            //          .FirstOrDefaultAsync();

            //    if (mixingInfoModel == null)
            //        return new ResponseDetail<BottomFactoryForReturnDto>(null, false, $"Không tìm thấy hóa chất {scanQRCodeParams.GlueName} trong hệ thống!");

            //    var details = mixingInfoModel.MixingInfoDetails.FirstOrDefault();
            //    if (details == null) return new ResponseDetail<BottomFactoryForReturnDto>(null, false,
            //        $"Hóa chất {scanQRCodeParams.GlueName} bị lỗi. Vui lòng liên hệ lab-team!");

            //    var subpackageList = await _subpackageRepository.FindAll(x => x.MixingInfoID == scanQRCodeParams.MixingInfoID).ToListAsync();
            //    return new ResponseDetail<BottomFactoryForReturnDto>(new BottomFactoryForReturnDto(details.Ingredient, subpackageList, scanQRCodeParams.MixingInfoID), true, "Thành Công!");
            //}
            var ingredient = await _ingredientRepository.FindAll(x => x.MaterialNO == scanQRCodeParams.PartNO).FirstOrDefaultAsync();
            if (ingredient == null) return new ResponseDetail<BottomFactoryForReturnDto>(new BottomFactoryForReturnDto(ingredient, null, 0), false, "Không tìm thấy hóa chất này! Vui lòng thử lại!");
            var code = await GenatateCodeForMixingInfo("");
            var todolist = _repoToDoList.FindAll(x =>
                                    x.BuildingID == scanQRCodeParams.BuildingID
                                 && x.GlueNameID == scanQRCodeParams.GlueNameID
                                 && x.IsEVA_UV
                                 && x.EstimatedFinishTime.TimeOfDay == scanQRCodeParams.EstimatedFinishTime.TimeOfDay
                                 && x.EstimatedStartTime.TimeOfDay == scanQRCodeParams.EstimatedStartTime.TimeOfDay
                              ).ToList();
            try
            {
                var ct = DateTime.Now;
                var mixingInfo = new MixingInfo
                {
                    GlueID = scanQRCodeParams.GlueID,
                    GlueName = scanQRCodeParams.GlueName,
                    BuildingID = scanQRCodeParams.BuildingID,
                    GlueNameID = scanQRCodeParams.GlueNameID,
                    MixBy = mixBy,
                    Code = code,
                    ExpiredTime = ct.AddHours(ingredient.ExpiredTime),
                    CreatedTime = ct,
                    EstimatedFinishTime = scanQRCodeParams.EstimatedFinishTime,
                    EstimatedStartTime = scanQRCodeParams.EstimatedStartTime
                };
                _mixingInfoRepository.Add(mixingInfo);
                await _mixingInfoRepository.SaveAll();

                var mixingInfoDetail = new MixingInfoDetail();
                mixingInfoDetail.Amount = ingredient.Unit;
                mixingInfoDetail.IngredientID = ingredient.ID;
                mixingInfoDetail.Position = "A";
                mixingInfoDetail.MixingInfoID = mixingInfo.ID;
                mixingInfoDetail.Time_Start = DateTime.Now;
                mixingInfoDetail.Batch = scanQRCodeParams.BatchNO;
                _mixingInfoDetailRepository.Add(mixingInfoDetail);
                await _mixingInfoDetailRepository.SaveAll();

                todolist.ForEach(x =>
                {
                    x.MixingInfoID = mixingInfo.ID;
                });

                _repoToDoList.UpdateRange(todolist);
                await _repoToDoList.SaveAll();
                return new ResponseDetail<BottomFactoryForReturnDto>(new BottomFactoryForReturnDto(ingredient, null, mixingInfo.ID), true, "Thành Công!");

            }
            catch (Exception ex)
            {
                return new ResponseDetail<BottomFactoryForReturnDto>(null, false, $"Thất Bại! {ex.Message}");
            }
        }

        public async Task<ResponseDetail<BottomFactoryForReturnDto>> ScanQRCodeV110(ScanQRCodeParams scanQRCodeParams)
        {
            var mixBy = _jwtService.GetUserID();
            var ingredient = await _ingredientRepository.FindAll(x => x.PartNO == scanQRCodeParams.PartNO).FirstOrDefaultAsync();
            if (ingredient == null) return new ResponseDetail<BottomFactoryForReturnDto>(new BottomFactoryForReturnDto(ingredient, null, 0), false, "Không tìm thấy hóa chất này! Vui lòng thử lại!");
            var glue = await _repoGlue.FindAll()
            .Include(x => x.GlueIngredients)
                .ThenInclude(x => x.Ingredient)
            .FirstOrDefaultAsync(x => x.ID == scanQRCodeParams.GlueID);

            var materialNO = glue.GlueIngredients.FirstOrDefault().Ingredient.PartNO;
            if (!materialNO.Equals(scanQRCodeParams.PartNO))
                return new ResponseDetail<BottomFactoryForReturnDto>(
                    new BottomFactoryForReturnDto(ingredient, null, 0),
                    false,
                    $"Vui lòng quét đúng mã QR của hóa chất {glue.Name}! Vui lòng thử lại!"
                   );
            var code = await GenatateCodeForMixingInfo("");
            var result = new BottomFactoryForReturnDto(ingredient, null, 0);
            result.Code = code;
            result.Batch = scanQRCodeParams.BatchNO;
            return new ResponseDetail<BottomFactoryForReturnDto>(result, true, "Không tìm thấy hóa chất này! Vui lòng thử lại!");

        }

        public async Task<ToDoListForReturnDto> UndoneList(int buildingID)
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
            //if (role.RoleID == (int)Enums.Role.Worker)
            //{
            //    var data = result.Where(x => x.PrintTime is null
            //                                 && x.EstimatedFinishTime.TimeOfDay < currentTime.TimeOfDay
            //                             ).ToList();
            //    var response = new ToDoListForReturnDto();
            //    response.TodoDetail(data, doneTotal, todoTotal, delayTotal, total);
            //    return response;
            //}
            if (role.RoleID == (int)Enums.Role.Admin || role.RoleID == (int)Enums.Role.Supervisor || role.RoleID == (int)Enums.Role.Staff || role.RoleID == (int)Enums.Role.Worker || role.RoleID == (int)Enums.Role.Dispatcher)
            {
                var EVA_UVTotal = result.Where(x => x.IsEVA_UV).Count();
                var response = new ToDoListForReturnDto();
                var data = result.Where(x => x.PrintTime is null && x.EstimatedFinishTime.TimeOfDay < currentTime.TimeOfDay).ToList();
                var dispatchListModel = await _repoDispatchList.FindAll(x =>
                                                                x.IsDelete == false
                                                                && x.EstimatedStartTime.Date == currentDate
                                                                && x.EstimatedFinishTime.Date == currentDate
                                                                && x.BuildingID == buildingID)
                                                                 .ToListAsync();
                //if (role.RoleID == (int)Enums.Role.Dispatcher)
                //{
                //    dispatchListModel = dispatchListModel.Where(x => lines.Contains(x.LineID)).ToList();
                //}
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
            }
            return new ToDoListForReturnDto(result.Where(x => x.PrintTime is null).ToList(), doneTotal, todoTotal, delayTotal, total);
        }

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
                itemTodolist.KindID = item.KindID;

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

                //var glue = _repoGlue.FindById(item.GlueID);
                //itemTodolist.KindID = glue != null && glue.KindID != null ? glue.KindID.Value : 0;

                itemTodolist.AbnormalStatus = item.AbnormalStatus;
                itemTodolist.IsDelete = item.IsDelete;

                itemTodolist.LineNames = lineList;
                itemTodolist.BuildingID = item.BuildingID;
                itemTodolist.IsEVA_UV = item.IsEVA_UV;

                todolist.Add(itemTodolist);
            }

            // GroupBy period and then by glueName
            var modelTemp = todolist.Where(x => x.KindID != (int)Enums.Kind.EVA_UV).OrderBy(x => x.EstimatedStartTime).GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime }).ToList();
            var result = new List<ToDoListDto>();
            foreach (var item in modelTemp)
            {
                result.AddRange(item.OrderByDescending(x => x.GlueName));
            }

            return result;
        }
        List<ToDoListDto> MapToTodolistEVA_UVDto(List<ToDoList> model)
        {
            var groupBy = model.GroupBy(x => new { x.GlueNameID, x.PrintTime });
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

                var glue = _repoGlue.FindById(item.GlueID);
                itemTodolist.KindID = glue != null && glue.KindID != null ? glue.KindID.Value : 0;

                itemTodolist.AbnormalStatus = item.AbnormalStatus;
                itemTodolist.IsDelete = item.IsDelete;

                itemTodolist.LineNames = lineList;
                itemTodolist.BuildingID = item.BuildingID;
                itemTodolist.IsEVA_UV = item.IsEVA_UV;

                todolist.Add(itemTodolist);
            }

            // GroupBy period and then by glueName
            var modelTempEVA_UV = todolist.Where(x => x.KindID == (int)Enums.Kind.EVA_UV).OrderBy(x => x.EstimatedStartTime).GroupBy(x => new { x.GlueNameID }).ToList();
            var result = new List<ToDoListDto>();
            foreach (var item in modelTempEVA_UV)
            {
                result.AddRange(item.OrderByDescending(x => x.GlueName));
            }
            return result;
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

        public async Task<ToDoListForReturnDto> ToDoList(int buildingID)
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
                //var model2 = await _repoToDoList.FindAll(x =>
                //       x.IsDelete == false
                //       && x.EstimatedStartTime.Date == currentDate
                //       && x.EstimatedFinishTime.Date == currentDate
                //       && x.GlueName.Contains(" + ")
                //       && x.AbnormalStatus == true
                //       && x.BuildingID == buildingID)
                //   .ToListAsync();
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

                //if (role.RoleID == (int)Enums.Role.Worker)
                //{
                //    var response = new ToDoListForReturnDto();
                //    response.TodoDetail(data, doneTotal, todoTotal, delayTotal, total);
                //    return response;
                //}
                if (role.RoleID == (int)Enums.Role.Admin || role.RoleID == (int)Enums.Role.Supervisor || role.RoleID == (int)Enums.Role.Staff || role.RoleID == (int)Enums.Role.Worker || role.RoleID == (int)Enums.Role.Dispatcher)
                {
                    var EVA_UVTotal = result.Where(x => x.IsEVA_UV).Count();
                    var dispatchListModel = await _repoDispatchList.FindAll(x =>
                                                               x.IsDelete == false
                                                               && x.EstimatedStartTime.Date == currentDate
                                                               && x.EstimatedFinishTime.Date == currentDate
                                                               && x.BuildingID == buildingID)
                                                               .ToListAsync();
                    //if (role.RoleID == (int)Enums.Role.Dispatcher)
                    //{
                    //    dispatchListModel = dispatchListModel.Where(x => lines.Contains(x.LineID)).ToList();
                    //}
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
                return new ToDoListForReturnDto(data, doneTotal, todoTotal, delayTotal, total);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }

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
                    .ThenInclude(x => x.Kind)
                    .ThenInclude(x => x.KindType)
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
                        GlueKindID = glue.KindID ?? 0,
                        KindTypeCode = glue.KindID.HasValue ? glue.Kind.KindType.Code : string.Empty,
                        KindID = plan.Building.KindID != null && glue.KindID != null && glue.KindID == plan.Building.KindID ? glue.KindID ?? 0 : 0,
                        GlueNameID = glue.GlueNameID,
                        GlueName = glue.Name,
                        ChemicalA = glue.GlueIngredients.FirstOrDefault(x => x.Position == "A").Ingredient,
                    }).Where(x => x.BuildingKindID > 0).ToListAsync();

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

            if (item.GlueKindID == 0) return new ResponseDetail<object>
            {
                Status = false,
                Message = $"Keo {item.GlueName} chưa được gán kind!"
            };

            if (item.ChemicalA is null) return new ResponseDetail<object>
            {
                Status = false,
                Message = $"Keo {item.GlueName} không có hóa chất A! Đã bị lỗi cài đặt keo. Vui lòng kiểm tra lại BPFC"
            };

            var hourlyOutput = item.HourlyOutput;
            if (hourlyOutput == 0) return new ResponseDetail<object>
            {
                Status = false,
                Message = $"Vui lòng thêm sản lượng hàng giờ cho chuyền {item.Building.Name}!"
            };
            return new ResponseDetail<object>(null, true, "");
        }
        private ResponseDetail<List<Period>> ValidatePeriod(Building data)
        {

            var building = data;

            if (building is null) return new ResponseDetail<List<Period>>
            {
                Status = false,
                Message = "Không tìm thấy tòa nhà nào trong hệ thống!"
            };

            if (building.LunchTime is null) return new ResponseDetail<List<Period>>
            {
                Status = false,
                Message = $"Tòa nhà {building.Name} chưa cài đặt giờ ăn trưa!"
            };

            // sua ngay 3/15/2021 2:28pm
            //if (building.LunchTime.Periods == null) return new ResponseDetail<List<Period>>
            //{
            //    Status = false,
            //    Message = $"Tòa nhà {building.Name} chưa cài đặt period!"
            //};

            return new ResponseDetail<List<Period>>(null, true, "");
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
                    if (validate.Status == false) return validate;

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


                    var gluesEVA_UV = data.Where(x => x.BuildingKindID == (int)Enums.Kind.EVA_UV
                                                    && x.GlueKindID == (int)Enums.Kind.EVA_UV)
                                            .GroupBy(x => new { x.GlueName, x.GlueKindID, x.BuildingKindID })
                                            .Where(x => x.Key.GlueKindID == x.Key.BuildingKindID)
                                            .ToList();
                    // Tao Todo cho STF
                    var glues = data.Where(x => x.GlueName.Contains("+")
                                            && x.BuildingKindID != (int)Enums.Kind.EVA_UV
                                            && x.GlueKindID != (int)Enums.Kind.EVA_UV)
                                    .GroupBy(x => new { x.GlueName, x.GlueKindID, x.BuildingKindID })
                                    .Where(x => x.Key.GlueKindID == x.Key.BuildingKindID)
                                    .ToList();
                    if (glues.Count > 0)
                    {
                        /* B5: Tạo nhiệm vụ
                  * Th1: Lên kế hoạch cho ngày hôm sau
                  * Th2: Lên kế hoạch trong ngày
                    Mac dinh khong tao gio tang ca
                  */
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

                                // 17/2/2021 != 17/2/2021 && 16/2/2021 != 17/2/2021
                                // Len ke hoach tu ngay hom truoc
                                // TH1: Tao ke hoach lam viec cho ngay mai nhung ngay mai moi bam cap nhat
                                // duedate != currentdate && createDate == currentDate
                                // 20 != 19 && 19 == 19
                                // 20 == 20 && 19 != 20
                                // TH2: Tao ke hoach lam viec cho ngay mai sau do nhan cap nhat luon
                                // Th3: Tao ke hoach cua ngay hom nay va bam cap nhat luon
                                var startPeriod = new TimeSpan(7, 30, 0);
                                var endPeriod = new TimeSpan(8, 30, 0);

                                if (item.DueDate.Date != currentDate && item.CreatedDate.Date == currentDate
                                    || item.DueDate.Date == currentDate && item.CreatedDate.Date != currentDate
                                    )
                                {
                                    for (int index = 0; index < periods.Count; index++)
                                    {
                                        var EstimatedStartTime = item.DueDate.Date.Add(new TimeSpan(periods[index].StartTime.Hour, periods[index].StartTime.Minute, 0));
                                        var EstimatedFinishTime = item.DueDate.Date.Add(new TimeSpan(periods[index].EndTime.Hour, periods[index].EndTime.Minute, 0));
                                        double replacementFrequency = (periods[index].EndTime - periods[index].StartTime).TotalHours;
                                        if (index == 0)
                                        {
                                            replacementFrequency = (periods[index].EndTime.TimeOfDay - startPeriod).TotalHours;
                                        }
                                        var todo = new ToDoListDto();
                                        todo.GlueName = item.GlueName;
                                        todo.GlueID = item.GlueID;
                                        todo.PlanID = item.PlanID;
                                        todo.LineID = item.Building.ID;
                                        todo.LineName = item.Building.Name;
                                        todo.PlanID = item.PlanID;
                                        todo.BPFCID = item.BPFCID;
                                        todo.KindID = item.GlueKindID;
                                        todo.IsEVA_UV = item.GlueKindID == (int)Enums.Kind.EVA_UV;
                                        todo.Supplier = item.ChemicalA.Supplier.Name;
                                        todo.PlanID = item.PlanID;
                                        todo.GlueNameID = item.GlueNameID.Value;
                                        todo.BuildingID = item.Building.ParentID.Value;
                                        todo.StandardConsumption = kgPair * (double)hourlyOutput * replacementFrequency;
                                        todo.EstimatedStartTime = EstimatedStartTime;
                                        todo.EstimatedFinishTime = EstimatedFinishTime;
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
                                        var todo = new ToDoListDto();
                                        todo.GlueName = item.GlueName;
                                        todo.GlueID = item.GlueID;
                                        todo.PlanID = item.PlanID;
                                        todo.LineID = item.Building.ID;
                                        todo.LineName = item.Building.Name;
                                        todo.PlanID = item.PlanID;
                                        todo.BPFCID = item.BPFCID;
                                        todo.KindID = item.KindID;
                                        todo.IsEVA_UV = item.KindID == (int)Enums.Kind.EVA_UV;
                                        todo.Supplier = item.ChemicalA.Supplier.Name;
                                        todo.PlanID = item.PlanID;
                                        todo.GlueNameID = item.GlueNameID.Value;
                                        todo.BuildingID = item.Building.ParentID.Value;
                                        todo.StandardConsumption = kgPair * (double)hourlyOutput * replacementFrequency;
                                        todo.EstimatedStartTime = estimatedStartTime; // 11:00
                                        todo.EstimatedFinishTime = estimatedFinishTime;//12:30

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
                    }


                    // EVA_UV không cần dựa theo Period để tạo.
                    if (gluesEVA_UV.Count > 0)
                    {
                        foreach (var glue in gluesEVA_UV)
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

                                var estimatedStartTime = DateTime.Now.ToRemoveSecond();
                                var estimatedFinishTime = DateTime.Now.ToRemoveSecond();
                                var todo = new ToDoListDto();
                                todo.GlueName = item.GlueName;
                                todo.GlueID = item.GlueID;
                                todo.PlanID = item.PlanID;
                                todo.LineID = item.Building.ID;
                                todo.LineName = item.Building.Name;
                                todo.PlanID = item.PlanID;
                                todo.KindID = item.GlueKindID;
                                todo.BPFCID = item.BPFCID;
                                todo.IsEVA_UV = item.GlueKindID == (int)Enums.Kind.EVA_UV;
                                todo.Supplier = item.ChemicalA.Supplier.Name;
                                todo.PlanID = item.PlanID;
                                todo.GlueNameID = item.GlueNameID.Value;
                                todo.BuildingID = item.Building.ParentID.Value;
                                todo.StandardConsumption = 0;
                                todo.EstimatedStartTime = estimatedStartTime;
                                todo.EstimatedFinishTime = estimatedFinishTime;
                                todolist.Add(todo);
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

                    await AddDispatchList_V105(plans);
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
        private async Task AddDispatchList_V105(List<int> plans)
        {
            var userID = _jwtService.GetUserID();
            var currentTime = DateTime.Now;
            var currentDate = currentTime.Date;

            var plansModel = await GetAllMultipleGlueByToday(plans);

            var value = plansModel.FirstOrDefault();

            var line = await _repoBuilding.FindAll(x => x.ID == value.Building.ID).FirstOrDefaultAsync();

            var building = await _repoBuilding.FindAll(x => x.ID == line.ParentID)
                 .Include(x => x.LunchTime)
                   .Include(x => x.PeriodMixingList)
                   .ThenInclude(x => x.PeriodDispatchList)
               .FirstOrDefaultAsync();

            var periods = building.PeriodMixingList.Where(x => x.IsDelete == false).ToList();
            var dispatchlistOvertime = periods.Where(x => x.IsOvertime).ToList();
            var startLunchTimeBuilding = building.LunchTime.StartTime;
            var endLunchTimeBuilding = building.LunchTime.EndTime;

            var dispatchlist = new List<DispatchListDto>();

            var specialglues = plansModel.Where(x => x.KindTypeCode == KindTypeOption.SPE)
                .GroupBy(x => new { x.GlueName, x.GlueKindID, x.BuildingKindID })
                .Where(x => x.Key.GlueKindID == x.Key.BuildingKindID).ToList();

            var normalGlues = plansModel.Where(x => x.KindTypeCode == KindTypeOption.NOR)
               .GroupBy(x => new { x.GlueName, x.GlueKindID, x.BuildingKindID })
               .Where(x => x.Key.GlueKindID == x.Key.BuildingKindID).ToList();
            foreach (var glue in normalGlues)
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

            foreach (var glue in specialglues)
            {
                foreach (var item in glue)
                {
                    var colorCode = 0;
                    var startTime = DateTime.Now.TimeOfDay;
                    var endTime = DateTime.Now.TimeOfDay;
                    var startTimeOfPeriod = item.DueDate.Date.Add(startTime);
                    var finishTimeOfPeriod = item.DueDate.Date.Add(endTime);

                    var estimatedStartTime = item.DueDate.Date.Add(startTime);
                    var estimatedFinishTime = item.DueDate.Date.Add(endTime);

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
                        KindID = glue.KindID ?? 0,
                        BuildingKindID = plan.Building.KindID ?? 0,
                        GlueKindID = glue.KindID ?? 0,
                        glue.GlueNameID,
                        GlueName = glue.Name,
                        ChemicalA = glue.GlueIngredients.FirstOrDefault(x => x.Position == "A").Ingredient,
                    }).Where(x => x.BuildingKindID > 0).ToListAsync();

            var value = plansModel.FirstOrDefault();

            var line = await _repoBuilding.FindAll(x => x.ID == value.Building.ID).FirstOrDefaultAsync();

            var building = await _repoBuilding.FindAll(x => x.ID == line.ParentID)
               .Include(x => x.PeriodMixingList)
               .FirstOrDefaultAsync();

            var periods = building.PeriodMixingList.ToList();
            var endWorkTime = new TimeSpan(16, 30, 0);
            var dispatchlistOvertime = periods.Where(x => x.IsOvertime || (x.StartTime.TimeOfDay >= endWorkTime && x.EndTime.TimeOfDay >= endWorkTime)).ToList();
            var startLunchTimeBuilding = building.LunchTime.StartTime;
            var endLunchTimeBuilding = building.LunchTime.EndTime;

            var dispatchlist = new List<DispatchListDto>();

            var glues = plansModel.Where(x => x.GlueKindID != (int)Enums.Kind.EVA_UV)
                .GroupBy(x => new { x.GlueName, x.GlueKindID, x.BuildingKindID })
                .Where(x => x.Key.GlueKindID == x.Key.BuildingKindID).ToList();
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
                                todo.EstimatedStartTime = startDispatchTimeTemp; // 7:30
                                todo.EstimatedFinishTime = endDispatchTime;
                                todo.CreatedTime = currentTime;
                                todo.CreatedBy = userID;
                                todo.StartTimeOfPeriod = swt;
                                todo.FinishTimeOfPeriod = finishTimeOfPeriod;

                                dispatchlist.Add(todo);
                                startDispatchTimeTemp = endDispatchTime; // 8:30
                            }
                        }
                    }
                    else// Nếu có đổi modelName trong ngày
                    {
                        for (int index = 0; index < periods.Count; index++)
                        {
                            var startTime = periods[index].StartTime.TimeOfDay;
                            var endTime = periods[index].EndTime.TimeOfDay;
                            var swt = item.DueDate.Date.Add(startTime);

                            var defaulHour = TimeSpan.FromHours(1);

                            if (index == 0)
                            {
                                startTime = startWorkingTime; // Bắt đầu làm việc từ 7:30
                                defaulHour = TimeSpan.FromHours(1);
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

                                // chi tao nhung thoi gian trong tuong lai
                                var finsihTimeOfWorkplan = item.FinishWorkingTime.TimeOfDay;
                                var startTimeOfWorkplan = item.StartWorkingTime.TimeOfDay;
                                // 16:30 >= 7:30 && 7:30 
                                // 16:30 >= 16:30 && 11:00 >= 10:50
                                if (finsihTimeOfWorkplan > startDispatchTimeTemp.TimeOfDay && endDispatchTime.TimeOfDay >= startTimeOfWorkplan)
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
        public async Task<bool> Print(List<int> subpackages)
        {
            var subpackage = await _subpackageRepository.FindAll(x =>
                                       subpackages.Contains(x.ID)).ToListAsync();
            subpackage.ForEach(x => { x.PrintTime = DateTime.Now; });
            _subpackageRepository.UpdateRange(subpackage);
            try
            {
                return await _repoToDoList.SaveAll();
            }
            catch
            {
                return false;
            }
        }

        public async Task<BottomFactoryForDispatchDto> GetAllDispatch(DispatchParamsDto obj)
        {
            var lines = await _repoBuilding.FindAll(x => x.ParentID == obj.BuildingID).ToListAsync();
            var model = await _repoDispatch.FindAll(x =>
                                                x.CreatedTime.Date == DateTime.Now.Date
                                                && x.GlueNameID == obj.GlueNameID
                                                && lines.Select(x => x.ID).Contains(x.LineID)
                                                ).Include(x => x.Building).ToListAsync();
            var data = new BottomFactoryForDispatchDto();
            data.Dispatches = model;
            var groupByLine = model.GroupBy(x => x.LineID).ToList();
            var lineForReturn = new List<LineForReturnDto>();
            foreach (var item in groupByLine)
            {
                var lineName = lines.Where(x => x.ID == item.Key).FirstOrDefault();
                lineForReturn.Add(new LineForReturnDto
                {
                    Name = lineName != null ? lineName.Name : "N/A",
                    AmountTotal = item.Count() > 0 ? item.Sum(x => x.Amount) * 1000 : 0
                });
            }
            data.Lines = lineForReturn;
            return data;
        }

        public async Task<ResponseDetail<object>> UpdateDispatch(UpdateDispatchParams update)
        {
            var item = await _repoDispatch.FindAll(x => x.ID == update.ID).FirstOrDefaultAsync();

            var amount = item.Amount - update.RemaningAmount;
            if (amount == 0)
            {
                return new ResponseDetail<object>(null, false, "Vui lòng nhập số lượng còn lại bằng 0!");
            }
            item.RemainingAmount = update.RemaningAmount; // 2.5
            item.Amount = amount; // 1.5
            _repoDispatch.Update(item);
            try
            {
                await _repoDispatch.SaveAll();
                return new ResponseDetail<object>(null, true, "Thành Công!");
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<MixingInfo> CreateMixingInfo(SubpackageParam obj)
        {
            var mixBy = _jwtService.GetUserID();
            var todolist = _repoToDoList.FindAll(x =>
                                x.BuildingID == obj.BuildingID
                             && x.GlueNameID == obj.GlueNameID
                             && x.IsEVA_UV
                             && x.EstimatedFinishTime.TimeOfDay == obj.EstimatedFinishTime.TimeOfDay
                             && x.EstimatedStartTime.TimeOfDay == obj.EstimatedStartTime.TimeOfDay
                          ).ToList();
            try
            {
                var ct = DateTime.Now;
                var mixingInfo = new MixingInfo
                {
                    GlueID = obj.GlueID,
                    GlueName = obj.GlueName,
                    BuildingID = obj.BuildingID,
                    GlueNameID = obj.GlueNameID,
                    MixBy = mixBy,
                    Code = obj.MixingInfoCode,
                    ExpiredTime = ct.AddHours(obj.Ingredient.ExpiredTime),
                    CreatedTime = ct,
                    EstimatedFinishTime = obj.EstimatedFinishTime,
                    EstimatedStartTime = obj.EstimatedStartTime
                };
                _mixingInfoRepository.Add(mixingInfo);
                await _mixingInfoRepository.SaveAll();

                var mixingInfoDetail = new MixingInfoDetail();
                mixingInfoDetail.Amount = obj.Ingredient.Unit;
                mixingInfoDetail.IngredientID = obj.Ingredient.ID;
                mixingInfoDetail.Position = "A";
                mixingInfoDetail.MixingInfoID = mixingInfo.ID;
                mixingInfoDetail.Time_Start = DateTime.Now;
                mixingInfoDetail.Batch = obj.BatchNO;
                _mixingInfoDetailRepository.Add(mixingInfoDetail);
                await _mixingInfoDetailRepository.SaveAll();

                todolist.ForEach(x =>
                {
                    x.MixingInfoID = mixingInfo.ID;
                });

                _repoToDoList.UpdateRange(todolist);
                await _repoToDoList.SaveAll();
                return mixingInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private async Task CreateSubpackage(SubpackageParam obj, MixingInfo mixingInfo)
        {
            var subpackages = obj.Subpackages;
            subpackages.ForEach(x =>
            {
                x.MixingInfoID = mixingInfo.ID;
                x.PrintTime = DateTime.Now;
                x.CreatedTime = DateTime.Now;
            });
            _subpackageRepository.AddRange(subpackages);
            await _subpackageRepository.SaveAll();
        }
        public async Task<bool> SaveSubpackage(SubpackageParam obj)
        {
            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var mixingInfo = await CreateMixingInfo(obj);
                    await CreateSubpackage(obj, mixingInfo);
                    transaction.Complete();
                    return true;
                }
                catch (System.Exception ex)
                {
                    // TODO
                    transaction.Dispose();
                    var message = ex.Message;
                    return false;
                    throw;
                }

            }

        }

        public async Task<MixingInfo> GetMixingInfo(int mixingInfoID)
        {
            var mixingInfo = await _mixingInfoRepository.FindAll(x => x.ID == mixingInfoID).Include(x => x.MixingInfoDetails).FirstOrDefaultAsync();
            return mixingInfo;
        }
        public async Task<Subpackage> GetSubpackageLatestSequence(string batch, int glueNameID, int buildingID)
        {
            var subpackages = await _subpackageRepository.FindAll()
            .Include(x => x.MixingInfo)
            .Where(x => x.Code.Contains(batch)
                    && x.GlueNameID == glueNameID
                    && x.MixingInfo.BuildingID == buildingID
                    && x.CreatedTime.Date == DateTime.Now.Date)
                    .OrderByDescending(x => x.Position).FirstOrDefaultAsync();
            return subpackages;
        }

        public async Task<SubpackageCapacity> GetSubpackageCapacity()
        {
            var capacity = await _repoSubpackageCapacity.FindAll().FirstOrDefaultAsync();
            return capacity;
        }
    }
}
