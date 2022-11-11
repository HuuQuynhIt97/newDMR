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
using DMR_API.Data;

namespace DMR_API._Services.Services
{
    public class StirService : IStirService
    {
        private readonly IMapper _mapper;
        private readonly IScaleMachineService _scaleMachineService;
        private readonly ISettingService _settingService;
        private readonly ISettingRepository _repoSetting;
        private readonly IToDoListRepository _repoTodolist;
        private readonly IMixingInfoRepository _repoMixingInfo;
        private readonly IStirRepository _repoStir;
        private readonly IStirRawDataRepository _repoStirStirRawData;
        private readonly IMongoRepository<Data.MongoModels.RawData> _repoRawData;
        private readonly MapperConfiguration _configMapper;

        public StirService(IMapper mapper,
            IScaleMachineService scaleMachineService,
            ISettingService settingService,
            ISettingRepository repoSetting,
            IToDoListRepository repoTodolist,
            IMixingInfoRepository repoMixingInfo,
            IStirRepository repoStir,
            IStirRawDataRepository repoStirStirRawData,
            IMongoRepository<DMR_API.Data.MongoModels.RawData> repoRawData,
            MapperConfiguration configMapper)
        {
            _mapper = mapper;
            _scaleMachineService = scaleMachineService;
            _settingService = settingService;
            _repoSetting = repoSetting;
            _repoTodolist = repoTodolist;
            _repoMixingInfo = repoMixingInfo;
            _repoStir = repoStir;
            _repoStirStirRawData = repoStirStirRawData;
            _repoRawData = repoRawData;
            _configMapper = configMapper;
        }

        public async Task<bool> Add(StirDTO model)
        {
            var ct = DateTime.Now;
            var item = _mapper.Map<Stir>(model);
            item.StartScanTime = ct;
            var stirModel = await _repoStir.FindAll(x => x.MixingInfoID == model.MixingInfoID).CountAsync();
            var setting = await _repoSetting.FindAll(x => x.ID == model.SettingID)
                .Include(x=>x.GlueType)
                .FirstOrDefaultAsync();

            if (stirModel == 0)
            {
                var mixingInfo = await _repoMixingInfo.FindAll(x => x.ID == model.MixingInfoID)
                                  .Include(x => x.Glue)
                                  .ThenInclude(x => x.GlueIngredients)
                                  .ThenInclude(x => x.Ingredient)
                                  .ThenInclude(x => x.GlueType).FirstOrDefaultAsync();
                var minute = mixingInfo.Glue.GlueIngredients
                        .FirstOrDefault(x => x.Position == "A")
                        .Ingredient.GlueType.Minutes;
                item.StartStiringTime = ct;
                item.StandardDuration = (int)(minute * 60);
            }
            _repoStir.Add(item);
            return await _repoStir.SaveAll();
        }

        public async Task<bool> Delete(object id)
        {
            var item = _repoStir.FindById(id);
            _repoStir.Remove(item);
            return await _repoStir.SaveAll();
        }

        public async Task<List<StirDTO>> GetAllAsync()
        {
            return await _repoStir.FindAll().ProjectTo<StirDTO>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        public StirDTO GetById(object id)
        {
            return _mapper.Map<Stir, StirDTO>(_repoStir.FindById(id));
        }

        public async Task<List<StirDTO>> GetStirByMixingInfoID(int MixingInfoID)
        {
            return await _repoStir.FindAll(x => x.MixingInfoID == MixingInfoID)
                .Include(x => x.MixingInfo)
                .ThenInclude(x => x.Glue)
                .ThenInclude(x => x.GlueIngredients)
                .ThenInclude(x => x.Ingredient)
                .ThenInclude(x => x.GlueType)
                .ProjectTo<StirDTO>(_configMapper).OrderBy(x => x.CreatedTime).ToListAsync();
        }

        public async Task<PagedList<StirDTO>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoStir.FindAll().ProjectTo<StirDTO>(_configMapper).OrderByDescending(x => x.ID);
            return await PagedList<StirDTO>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }

        public async Task<Setting> ScanMachine(int buildingID, string scanValue)
        {
            var setting = await _settingService.CheckMachine(buildingID, scanValue);
            if (setting is null)
            {
                return new Setting();
            }
            return setting;
        }

        public async Task<PagedList<StirDTO>> Search(PaginationParams param, object text)
        {
            var lists = _repoStir.FindAll().ProjectTo<StirDTO>(_configMapper)
           .Where(x => x.GlueName.Contains(text.ToString()))
           .OrderByDescending(x => x.ID);
            return await PagedList<StirDTO>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        public async Task<bool> Update(StirDTO model)
        {
            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var currentTime = DateTime.Now;
                    var item = await _repoStir.FindAll(x => x.ID == model.ID)
                        .Include(x => x.Setting)
                        .ThenInclude(x => x.Building)
                        .FirstOrDefaultAsync();

                    var machineID = item.Setting.MachineCode.ToInt();
                    var end = item.StartScanTime.AddMinutes(model.GlueType.Minutes);
                    var start = item.StartScanTime;

                    var rawDataModel = _repoStirStirRawData
                        .FindAll()
                        .Where(x => x.Building == item.Setting.Building.Name && x.MachineID == machineID && x.CreatedTime >= start && x.CreatedTime <= end)
                        .Select(x => new { x.RPM, x.CreatedTime, x.Sequence })
                        .OrderByDescending(x => x.CreatedTime).ToArray();

                    int temp = 0;
                    int standardDuration = item.StandardDuration == 0 ? (int)model.GlueType.Minutes * 60 : item.StandardDuration;
                    //if (rawData.Count == 0) return false;
                    // Neu = 0 thì lấy dữ liệu giả
                    if (rawDataModel.Length == 0)
                    {
                        temp = (int)Math.Round((model.EndTime - model.StartTime).TotalMinutes, 0);
                        item.StandardDuration = standardDuration;
                        item.ActualDuration = temp;
                        item.StartTime = model.StartTime;
                        item.EndTime = model.EndTime;
                        item.FinishStiringTime = model.EndTime;
                        item.Status = true;
                        _repoStir.Update(item);
                        //var stirList = await _repoStir.FindAll(x => x.MixingInfoID == model.MixingInfoID)
                        //    .OrderBy(x => x.CreatedTime).ToListAsync();

                        var todolist = await _repoTodolist.FindAll(x => x.MixingInfoID == model.MixingInfoID).ToListAsync();
                        todolist.ForEach(todo =>
                        {
                            todo.StartStirTime = item.StartTime;
                            todo.FinishStirTime = item.FinishStiringTime;
                        });
                        await _repoStir.SaveAll();
                    }
                    else
                    {
                        foreach (var raw in rawDataModel)
                        {
                            if (raw.RPM >= model.GlueType.RPM)
                            {
                                temp++;
                            }
                        }
                        // Neu khuay du thoi gian va du toc do thi pass
                        if (standardDuration <= temp)
                        {
                            item.StandardDuration = standardDuration;
                            item.ActualDuration = temp;
                            item.StartTime = rawDataModel.LastOrDefault().CreatedTime;
                            item.EndTime = rawDataModel.FirstOrDefault().CreatedTime;
                            item.FinishStiringTime = currentTime;
                            item.Status = true;
                            _repoStir.Update(item);
                            //var stirList = await _repoStir.FindAll(x => x.MixingInfoID == model.MixingInfoID)
                            //    .OrderBy(x => x.CreatedTime).ToListAsync();

                            var todolist = await _repoTodolist.FindAll(x => x.MixingInfoID == model.MixingInfoID).ToListAsync();
                            todolist.ForEach(todo =>
                            {
                                todo.StartStirTime = item.StartTime;
                                todo.FinishStirTime = item.FinishStiringTime;
                            });
                            await _repoStir.SaveAll();
                        }
                        else // nguoc lai thi khuay them
                        {
                            item.StandardDuration = standardDuration;
                            item.ActualDuration = temp;
                            item.StartTime = rawDataModel.LastOrDefault().CreatedTime;
                            item.EndTime = rawDataModel.FirstOrDefault().CreatedTime;
                            item.Status = false;
                            _repoStir.Update(item);
                            await _repoStir.SaveAll();
                            var stir = new Stir()
                            {
                                MixingInfoID = model.MixingInfoID,
                                StandardDuration = standardDuration - temp,
                                Status = false,
                                CreatedTime = DateTime.Now,
                                GlueName = model.GlueName,
                                SettingID = model.SettingID
                            };
                            _repoStir.Add(stir);
                            await _repoStir.SaveAll();
                        }
                    }

                    transaction.Complete();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Dispose();
                    return false;
                }
            }

        }

        public async Task<bool> UpdateStartScanTime(int mixingInfoID)
        {
            var item = await _repoStir.FindAll(x => x.MixingInfoID == mixingInfoID && x.StartScanTime == DateTime.MinValue).FirstOrDefaultAsync();
            if (item is null) return false;
            item.StartScanTime = DateTime.Now;
            _repoStir.Update(item);
            return await _repoStir.SaveAll();
        }

        public async Task<Stir> UpdateStir(StirDTO model)
        {
            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var currentTime = DateTime.Now;
                    var item = await _repoStir.FindAll(x => x.ID == model.ID)
                        .Include(x => x.Setting)
                        .ThenInclude(x => x.Building)
                        .FirstOrDefaultAsync();
                  
                    var machineID = item.Setting.MachineCode.ToInt();
                    var end = item.StartScanTime.AddMinutes(model.GlueType.Minutes);
                    var start = item.StartScanTime;
                
                    var rawDataModel = await _repoStirStirRawData
                        .FindAll(x => x.Building == item.Setting.Building.Name && x.MachineID == machineID && x.CreatedTime >= start && x.CreatedTime <= end)
                        .FirstOrDefaultAsync();

                    int temp = 0;
                    int standardDuration = item.StandardDuration == 0 ? (int)model.GlueType.Minutes * 60 : item.StandardDuration;
                    //if (rawData.Count == 0) return false;
                    // Neu = 0 thì lấy dữ liệu giả
                    if (rawDataModel == null)
                    {
                        temp = (int)Math.Round((model.EndTime - model.StartTime).TotalMinutes, 0);
                        item.StandardDuration = standardDuration;
                        item.ActualDuration = temp;
                        item.StartTime = model.StartTime;
                        item.EndTime = model.EndTime;
                        item.FinishStiringTime = model.EndTime;
                        item.Status = true;
                        _repoStir.Update(item);
                        //var stirList = await _repoStir.FindAll(x => x.MixingInfoID == model.MixingInfoID)
                        //    .OrderBy(x => x.CreatedTime).ToListAsync();

                        var todolist = await _repoTodolist.FindAll(x => x.MixingInfoID == model.MixingInfoID).ToListAsync();
                        todolist.ForEach(todo =>
                        {
                            todo.StartStirTime = item.StartTime;
                            todo.FinishStirTime = item.FinishStiringTime;
                        });
                        await _repoStir.SaveAll();
                    }
                    else
                    {
                        var sequence = rawDataModel.Sequence;
                        var rawData = await _repoStirStirRawData
                        .FindAll(x => x.Building == item.Setting.Building.Name && x.MachineID == machineID && x.Sequence == sequence)
                        .Select(x => new { x.RPM, x.CreatedTime, x.Sequence })
                        .OrderByDescending(x => x.CreatedTime).ToListAsync();

                        foreach (var raw in rawData)
                        {
                            if (raw.RPM >= model.GlueType.RPM)
                            {
                                temp++;
                            }
                        }
                        // Neu khuay du thoi gian va du toc do thi pass
                        if (standardDuration <= temp)
                        {
                            item.StandardDuration = standardDuration;
                            item.ActualDuration = temp;
                            item.StartTime = rawData.LastOrDefault().CreatedTime;
                            item.EndTime = rawData.FirstOrDefault().CreatedTime;
                            item.FinishStiringTime = currentTime;
                            item.Status = true;
                            _repoStir.Update(item);
                            //var stirList = await _repoStir.FindAll(x => x.MixingInfoID == model.MixingInfoID)
                            //    .OrderBy(x => x.CreatedTime).ToListAsync();

                            var todolist = await _repoTodolist.FindAll(x => x.MixingInfoID == model.MixingInfoID).ToListAsync();
                            todolist.ForEach(todo =>
                            {
                                todo.StartStirTime = item.StartTime;
                                todo.FinishStirTime = item.FinishStiringTime;
                            });
                            await _repoStir.SaveAll();
                        }
                        else // nguoc lai thi khuay them
                        {

                            item.StandardDuration = standardDuration;
                            item.ActualDuration = temp;
                            item.StartTime = rawData.LastOrDefault().CreatedTime;
                            item.EndTime = rawData.FirstOrDefault().CreatedTime;
                            item.Status = false;
                            _repoStir.Update(item);
                            await _repoStir.SaveAll();
                            var stir = new Stir()
                            {
                                MixingInfoID = model.MixingInfoID,
                                StandardDuration = standardDuration - temp,
                                Status = false,
                                CreatedTime = DateTime.Now,
                                GlueName = model.GlueName,
                                SettingID = model.SettingID
                            };
                            _repoStir.Add(stir);
                            await _repoStir.SaveAll();
                        }
                    }

                    transaction.Complete();
                    return item;
                }
                catch (Exception)
                {
                    transaction.Dispose();
                    return null;
                }
            }
            //using var transaction = new TransactionScopeAsync().Create();
            //{
            //    try
            //    {
            //        var currentTime = DateTime.Now;
            //        var item = await _repoStir.FindAll(x => x.ID == model.ID)
            //            .Include(x => x.Setting)
            //            .ThenInclude(x => x.Building)
            //            .FirstOrDefaultAsync();

            //        var machineID = item.Setting.MachineCode.ToInt();
            //        var end = item.StartScanTime.AddMinutes(model.GlueType.Minutes);
            //        var start = item.StartScanTime;

            //        var rawDataModel = _repoStirStirRawData
            //            .FindAll()
            //            .Where(x => x.Building == item.Setting.Building.Name &&  x.MachineID == machineID && x.CreatedTime >= start && x.CreatedTime <= end)
            //            .Select(x => new { x.RPM, x.CreatedTime, x.Sequence })
            //            .OrderByDescending(x => x.CreatedTime).ToArray();


            //        double temp = 0;
            //        int standardDuration = item.StandardDuration == 0 ? (int)model.GlueType.Minutes * 60 : item.StandardDuration;
            //        // Neu khong co du lieu tu IOT thi gan bang false
            //        if (rawDataModel.Length == 0)
            //        {
            //            temp = (int)Math.Round((model.EndTime - model.StartTime).TotalMinutes, 0);
            //            item.StandardDuration = standardDuration;
            //            item.ActualDuration = temp.ToInt();
            //            item.StartTime = DateTime.MinValue;
            //            item.EndTime = DateTime.MinValue;
            //            item.FinishStiringTime = model.EndTime;
            //            item.Status = false;
            //            _repoStir.Update(item);
            //            await _repoStir.SaveAll();
            //        }
            //        else
            //        {
            //            var stdCon = model.GlueType.RPM;
            //            var sequence = rawDataModel.DistinctBy(x=>x.Sequence).Select(x=>x.Sequence).ToList();
            //            var rawData = rawDataModel
            //               .Where(x => x.RPM >= stdCon && sequence.Contains(x.Sequence))
            //               .Select(x => new { x.RPM, x.CreatedTime, x.Sequence })
            //               .OrderByDescending(x => x.CreatedTime).ToList();

            //            var RPM = rawData.Average(x => x.RPM);
            //            var min = rawData.LastOrDefault().CreatedTime;
            //            var max = rawData.FirstOrDefault().CreatedTime;
            //             temp = (max - min).TotalSeconds;
            //            // Neu khuay du thoi gian va du toc do thi pass
            //            if (standardDuration <= temp)
            //            {
            //                item.StandardDuration = standardDuration;
            //                item.ActualDuration = (int)Math.Round(temp);
            //                item.StartTime = min;
            //                item.EndTime = max;
            //                item.FinishStiringTime = currentTime;
            //                item.Status = true;
            //                item.RPM = (int)Math.Round(RPM);
            //                _repoStir.Update(item);
            //                await _repoStir.SaveAll();

            //                // Lay startTime cua luot dau va end time cua luot cuoi cung co trang thai la pass update vao todolist
            //                var stirModel = await _repoStir.FindAll(a => a.MixingInfoID == item.MixingInfoID).OrderByDescending(x => x.CreatedTime).ToListAsync();
            //                var startStirTime = stirModel.LastOrDefault().StartStiringTime;
            //                var finishStirTime = stirModel.FirstOrDefault().FinishStiringTime;
            //                // update TG pass vao todolist
            //                var todolist = await _repoTodolist.FindAll(x => x.MixingInfoID == model.MixingInfoID).ToListAsync();
            //                todolist.ForEach(todo =>
            //                {
            //                    todo.StartStirTime = startStirTime;
            //                    todo.FinishStirTime = finishStirTime;
            //                });
            //                await _repoTodolist.SaveAll();
            //            }
            //            else // nguoc lai thi khuay them
            //            {
            //                item.StandardDuration = standardDuration;
            //                item.ActualDuration = (int)Math.Round(temp);
            //                item.RPM = (int)Math.Round(RPM);
            //                item.Status = false;
            //                _repoStir.Update(item);
            //                await _repoStir.SaveAll();
            //                var stir = new Stir()
            //                {
            //                    MixingInfoID = model.MixingInfoID,
            //                    StandardDuration = standardDuration - (int)Math.Round(temp),
            //                    Status = false,
            //                    CreatedTime = DateTime.Now,
            //                    GlueName = model.GlueName,
            //                    SettingID = model.SettingID
            //                };
            //                _repoStir.Add(stir);
            //                await _repoStir.SaveAll();
            //            }
            //        }

            //        transaction.Complete();
            //        return item;
            //    }
            //    catch
            //    {
            //        transaction.Dispose();
            //        return null;
            //    }
            //}
        }
    }
}