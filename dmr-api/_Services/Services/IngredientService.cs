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
using Microsoft.AspNetCore.Http;
using dmr_api.Models;
using System.Text.RegularExpressions;

namespace DMR_API._Services.Services
{
    public class IngredientService : IIngredientService
    {
        private readonly IGlueIngredientRepository _repoGlueIngredient;
        private readonly IIngredientRepository _repoIngredient;
        private readonly IIngredientInfoRepository _repoIngredientInfo;
        private readonly IIngredientInfoReportRepository _repoIngredientInfoReport;
        private readonly ISupplierRepository _repoSupplier;
        private readonly IPlanRepository _repoPlan;
        private readonly IMixingInfoRepository _repoMixingInfo;
        private readonly IGlueRepository _repoGlue;
        private readonly IGlueTypeRepository _repoGlueType;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        private readonly IHttpContextAccessor _accessor;

        public IngredientService(
            IMixingInfoRepository repoMixingInfo,
            IGlueRepository repoGlue,
            IGlueIngredientRepository repoGlueIngredient,
            IIngredientInfoReportRepository repoIngredientInfoReport,
            IPlanRepository repoPlan,
            IIngredientRepository repoIngredient,
            IHttpContextAccessor accessor,
            IGlueTypeRepository repoGlueType,
            IIngredientInfoRepository repoIngredientInfo,
            ISupplierRepository repoSupplier,
            IMapper mapper,
            MapperConfiguration configMapper
            )
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoIngredient = repoIngredient;
            _repoIngredientInfo = repoIngredientInfo;
            _repoSupplier = repoSupplier;
            _accessor = accessor;
            _repoPlan = repoPlan;
            _repoIngredientInfoReport = repoIngredientInfoReport;
            _repoGlueIngredient = repoGlueIngredient;
            _repoGlueType = repoGlueType;
            _repoGlue = repoGlue;
            _repoMixingInfo = repoMixingInfo;
        }
        public async Task<bool> CheckExists(int id)
        {
            return await _repoIngredient.CheckExists(id);
        }
        public async Task<bool> CheckExistsName(string name)
        {
            return await _repoIngredient.FindAll().AnyAsync(x => x.isShow && x.Name.Trim().ToLower().Equals(name.Trim().ToLower()));
        }

        //Thêm Ingredient mới vào bảng Ingredient
        public async Task<bool> Add(IngredientDto model)
        {
            string token = _accessor.HttpContext.Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            if (userID == 0) return false;
            var ingredient = _mapper.Map<Ingredient>(model);
            ingredient.isShow = true;
            ingredient.CreatedBy = userID;
            _repoIngredient.Add(ingredient);
            return await _repoIngredient.SaveAll();
        }

        public async Task<bool> Add1(IngredientDto1 model)
        {
            string token = _accessor.HttpContext.Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            if (userID == 0) return false;
            var ingredient = _mapper.Map<Ingredient>(model);
            ingredient.isShow = true;
            var supplier_name =  _repoSupplier.FindAll().FirstOrDefault(x => x.ID == model.SupplierID).Name;
            ingredient.PartNO = model.PartNO + ':' + supplier_name;
            ingredient.CreatedBy = userID;
            _repoIngredient.Add(ingredient);
            return await _repoIngredient.SaveAll();
        }

        public async Task<string> AddRangeAsync(List<IngredientForImportExcelDto> model)
        {
            var suppliers = await _repoSupplier.FindAll().ToListAsync();
            var ingredientList = _repoIngredient.FindAll();
            string error = string.Empty;
            foreach (var item in model)
            {
                var checkMaterial = ingredientList.FirstOrDefault(x => x.MaterialNO.Equals(item.MaterialNO));
                var checkName = ingredientList.FirstOrDefault(x => x.Name.Equals(item.Name));
                if (checkMaterial != null)
                {
                    error = $"Material# {item.MaterialNO} already exists in the database";
                    break;
                }
                if (checkName != null)
                {
                    error = $"Ingredient Name {item.Name} already exists in the database";
                    break;
                }
            }
            if (error != string.Empty) return error;
            model.ForEach(item =>
            {

                item.isShow = true;
                var supply = suppliers.FirstOrDefault(x => x.Name.ToLower().Equals(item.SupplierName.ToSafetyString().ToLower()));
                item.SupplierID = supply != null ? supply.ID : 0;
            });
            var ingredients = _mapper.Map<List<Ingredient>>(model);
            _repoIngredient.AddRange(ingredients);
            try
            {
                await _repoIngredient.SaveAll();
                return "ok";
            }
            catch
            {
                return "Error";
            }
        }


        //Lấy danh sách Ingredient và phân trang
        public async Task<PagedList<IngredientDto>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoIngredient.FindAll().Where(x => x.isShow == true).Include(x => x.Supplier).ProjectTo<IngredientDto>(_configMapper).OrderByDescending(x => x.ID);
            return await PagedList<IngredientDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }

        //Tìm kiếm Ingredient
        public async Task<PagedList<IngredientDto>> Search(PaginationParams param, object text)
        {
            var lists = _repoIngredient.FindAll().Where(x => x.isShow == true).Include(x => x.Supplier).ProjectTo<IngredientDto>(_configMapper)
            .Where(x => x.Code.Contains(text.ToSafetyString()) || x.Name.Contains(text.ToSafetyString()) || x.Supplier.Contains(text.ToSafetyString()))
            .OrderByDescending(x => x.ID);
            return await PagedList<IngredientDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }

        // Xóa Ingredient
        public async Task<bool> Delete(object id)
        {
            string token = _accessor.HttpContext.Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            if (userID == 0) return false;
            var ingredient = _repoIngredient.FindById(id.ToInt());
            ingredient.isShow = false;
            ingredient.ModifiedBy = userID;
            ingredient.ModifiedDate = DateTime.Now;
            return await _repoIngredient.SaveAll();
        }

        // Cập nhật Ingredient
        public async Task<bool> Update(IngredientDto model)
        {
            string token = _accessor.HttpContext.Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            if (userID == 0) return false;
            var ingredient = _mapper.Map<Ingredient>(model);
            ingredient.isShow = true;
            ingredient.ModifiedBy = userID;
            var supplier_name = _repoSupplier.FindAll().FirstOrDefault(x => x.ID == model.SupplierID).Name;
            ingredient.PartNO = model.PartNO + ':' + supplier_name;
            ingredient.ModifiedDate = DateTime.Now;
            _repoIngredient.Update(ingredient);
            return await _repoIngredient.SaveAll();
        }

        // Lấy toàn bộ danh sách Ingredient 
        public async Task<List<IngredientDto>> GetAllAsync()
        => await _repoIngredient.FindAll()
                .Where(x => x.isShow == true)
                .Include(x => x.Supplier)
                .Include(x => x.GlueType)
                .ProjectTo<IngredientDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        public async Task<List<GlueType>> GetAllGlueTypeAsync()
        {
            return await _repoGlueType.FindAll().OrderBy(x => x.ID).ToListAsync();
        }
        // Lấy toàn bộ danh sách IngredientInfo
        public async Task<List<IngredientInfoDto>> GetAllIngredientInfoAsync()
        {
            var resultStart = DateTime.Now;
            var resultEnd = DateTime.Now;
            return await _repoIngredientInfo.FindAll().Where(x => x.CreatedDate.Date >= resultStart.Date && x.CreatedDate.Date <= resultEnd.Date).ProjectTo<IngredientInfoDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        // Lấy toàn bộ danh sách IngredientInfo
        public async Task<List<IngredientInfoDto>> GetAllIngredientInfoOutputAsync()
        {
            var resultStart = DateTime.Now;
            var resultEnd = DateTime.Now;
            return await _repoIngredientInfo.FindAll().Where(x => x.CreatedDate.Date >= resultStart.Date && x.CreatedDate.Date <= resultEnd.Date).ProjectTo<IngredientInfoDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        // Lấy toàn bộ danh sách IngredientInfo theo building
        public async Task<List<IngredientInfoDto>> GetAllIngredientInfoByBuildingNameAsync(string name)
        {
            var resultStart = DateTime.Now;
            var resultEnd = DateTime.Now;
            return await _repoIngredientInfo.FindAll().Where(x => x.CreatedDate.Date >= resultStart.Date && x.CreatedDate.Date <= resultEnd.Date && x.BuildingName == name).ProjectTo<IngredientInfoDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        // Lấy toàn bộ danh sách IngredientReport
        public async Task<List<IngredientInfoReportDto>> GetAllIngredientInfoReportAsync()
        {
            var resultStart = DateTime.Now;
            var resultEnd = DateTime.Now;
            return await _repoIngredientInfoReport.FindAll().Where(x => x.CreatedDate.Date >= resultStart.Date && x.CreatedDate.Date <= resultEnd.Date && x.Qty > 0).ProjectTo<IngredientInfoReportDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        //Hàm Lấy toàn bộ danh sách IngredientReport theo building
        public async Task<List<IngredientInfoReportDto>> GetAllIngredientInfoReportByBuildingNameAsync(string name)
        {
            var resultStart = DateTime.Now;
            var resultEnd = DateTime.Now;
            return await _repoIngredientInfoReport.FindAll().Where(x => x.CreatedDate.Date >= resultStart.Date && x.CreatedDate.Date <= resultEnd.Date && x.BuildingName == name).ProjectTo<IngredientInfoReportDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        // Filter Ingredient Report theo ngay
        public async Task<object> GetAllIngredientReportByRange(DateTime min, DateTime max)
        {
            return await _repoIngredientInfo.FindAll()
                .Where(x => x.CreatedTime.Date >= min.Date && x.CreatedTime.Date <= max.Date)
                .ToListAsync();
        }

        // Filter Ingredient Report theo ngay + building
        public async Task<object> GetAllIngredientReportByRangeWithBuilding(DateTime min, DateTime max, string name)
        {
            return await _repoIngredientInfoReport.FindAll()
                .Where(x => x.CreatedTime.Date >= min.Date && x.CreatedTime.Date <= max.Date && x.BuildingName == name)
                .ToListAsync();
        }

        // lấy Ingredient theo Ingredient_Id
        public IngredientDto GetById(object id)
        {
            return _mapper.Map<Ingredient, IngredientDto>(_repoIngredient.FindById(id));
        }

        // Check qrcode  IngredientReport co ton tai hay khong
        public async Task<bool> CheckBarCodeExists(string code)
        {
            return await _repoIngredient.CheckBarCodeExists(code);
        }

        public async Task<bool> UpdatePrint(QrPrintDto entity)
        {
            var model = await _repoIngredient.FindAll().FirstOrDefaultAsync(x => x.ID == entity.ID);
            if (model != null)
            {
                model.ManufacturingDate = entity.ManufacturingDate;
                return await _repoIngredient.SaveAll();
            }
            else
            {
                return false;

            }
        }

        public async Task<IngredientDto> ScanQRCode(string qrCode)
        {
            var ingredient = await _repoIngredient.FindAll().Where(x => x.isShow == true).FirstOrDefaultAsync(x => x.PartNO.Equals(qrCode));
            var result = _mapper.Map<IngredientDto>(ingredient);
            return result;
        }
        //Update 08/04/2021 - Leo
        public async Task<object> ScanQRCodeFromChemialWareHouseV1(ScanQrCodeDto entity)
        {
            var results = entity.qrCode.Split("    ");
            var partNo = results[2].Split(":")[1].Trim() + ':' + results[0].Split(":")[1].Trim();
            var Batch = results[4].Split(":")[1].Trim() + ':' + results[0].Split(":")[1].Trim();
            var model = _repoIngredient.FindAll().FirstOrDefault(x => x.PartNO.Equals(partNo));
            var supModel = _repoSupplier.GetAll();
            var ProductionDates = results[5].Split(":")[1].Trim().ToDateTime();
            var exp = ProductionDates.AddMonths(3);
            var currentDate = DateTime.Now;
            var data = await CreateIngredientInfo(new IngredientInfo
            {
                Name = model.Name,
                ExpiredTime = exp.Date,
                ManufacturingDate = ProductionDates.Date,
                SupplierName = supModel.FirstOrDefault(s => s.ID == model.SupplierID).Name,
                Qty = model.Unit.ToInt(),
                Batch = Batch,
                Consumption = "0",
                Code = model.PartNO,
                IngredientID = model.ID,
                UserID = entity.userid,
                BuildingName = entity.building

            });

            // check trong bang ingredientReport xem đã tồn tại code hay chưa , nếu có tồn tại 
            if (await _repoIngredientInfoReport.CheckBarCodeExists(partNo))
            {
                // check tiep trong bang ingredientReport xem co du lieu chua 
                var result = _repoIngredientInfoReport.FindAll().FirstOrDefault(x => x.Code == partNo && x.Batch == Batch && x.CreatedDate.Date == currentDate.Date);

                // nếu khác Null thi update lai
                if (result != null)
                {
                    result.Qty = model.Unit.ToInt() + result.Qty;
                    await UpdateIngredientInfoReport(result);
                }

                // nếu bằng null thì tạo mới IngredientReport
                else
                {
                    await CreateIngredientInfoReport(new IngredientInfoReport
                    {
                        Name = model.Name,
                        ExpiredTime = ProductionDates.Date.AddMonths(3),
                        ManufacturingDate = ProductionDates.Date,
                        SupplierName = supModel.FirstOrDefault(s => s.ID == model.SupplierID).Name,
                        Qty = model.Unit.ToInt(),
                        Consumption = "0",
                        Code = model.PartNO,
                        Batch = Batch,
                        IngredientInfoID = data.ID,
                        UserID = entity.userid,
                        BuildingName = entity.building
                    });
                }
            }

            // nếu chưa tồn tại thì thêm mới
            else
                await CreateIngredientInfoReport(new IngredientInfoReport
                {
                    Name = model.Name,
                    ExpiredTime = ProductionDates.Date.AddMonths(3),
                    ManufacturingDate = ProductionDates.Date,
                    SupplierName = supModel.FirstOrDefault(s => s.ID == model.SupplierID).Name,
                    Qty = model.Unit.ToInt(),
                    Batch = Batch,
                    Consumption = "0",
                    Code = model.PartNO,
                    IngredientInfoID = data.ID,
                    UserID = entity.userid,
                    BuildingName = entity.building
                });
            return true;
        }
        public async Task<object> ScanQRCodeOutputV1(ScanQrCodeDto enity)
        {

            //var dayAndBatch = string.Empty;
            //var pattern = @"((\d*)-(\w*)-)*";
            //var obj = new string[] { };
            //Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

            //Match m = r.Match(qrCode);
            //if (m.Success)
            //{
            //    dayAndBatch = m.Groups[1].ToSafetyString();
            //    obj = dayAndBatch.Split('-');
            //}

            var results = enity.qrCode.Split("    ");
            //var partNo = results[2].Split(":")[1].Trim();
            //var Batch = results[4].Split(":")[1].Trim();
            //var model = _repoIngredient.FindAll().FirstOrDefault(x => x.PartNO.Equals(partNo));
            //var supModel = _repoSupplier.GetAll();
            //var ProductionDates = results[5].Split(":")[1].Trim().ToDateTime();
            //var exp = ProductionDates.AddMonths(3);
            //var currentDate = DateTime.Now;

            // load tat ca supplier
            var supModel = _repoSupplier.GetAll();
            // lay gia tri "barcode" trong chuỗi qrcode được chuyền lên
            var partNo = results[2].Split(":")[1].Trim() + ':' + results[0].Split(":")[1].Trim();
            // tim ID của ingredient
            var ingredientID = _repoIngredient.FindAll().FirstOrDefault(x => x.PartNO.Equals(partNo)).ID;
            // Find ingredient theo ingredientID vừa tìm được ở trên
            var model = _repoIngredient.FindById(ingredientID);
            // lấy giá trị "Batch" trong chuỗi qrcode được chuyền lên
            var Batch = results[4].Split(":")[1].Trim() + ':' + results[0].Split(":")[1].Trim();

            var currentDay = DateTime.Now;


            // check trong bang ingredientReport xem đã tồn tại code hay chưa , nếu có tồn tại 
            if (await _repoIngredientInfo.CheckBarCodeExists(partNo))
            {
                // check tiep trong bang ingredientReport xem co du lieu chua 
                var checkStatus = _repoIngredientInfo.FindAll().Where(x => x.Code == partNo && x.BuildingName == enity.building && x.Batch == Batch && x.CreatedDate.Date == currentDay.Date && x.Status == false).OrderBy(y => y.CreatedTime).FirstOrDefault();
                // nếu khác Null thi update lai
                if (checkStatus != null)
                {
                    checkStatus.Status = true;
                    await UpdateIngredientInfo(checkStatus);
                }
                else
                {
                    return new
                    {
                        status = false,
                        message = "Đã dùng hết !"
                    };
                }
            }

            // nếu chưa tồn tại thì thêm mới
            else
            {
                return new
                {
                    status = false,
                    message = "Hãy scan QR Code hàng nhập trước :) !"
                };
            }

            return true;
        }
        //End update
        public async Task<object> ScanQRCodeFromChemialWareHouse(string qrCode, string building, int userid)
        {

            //var dayAndBatch = string.Empty;
            //var pattern = @"((\d*)-(\w*)-)*";
            //var obj = new string[] { };
            //Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

            //Match m = r.Match(qrCode);
            //if (m.Success)
            //{
            //    dayAndBatch = m.Groups[1].ToSafetyString();
            //    obj = dayAndBatch.Split('-');
            //}

            var results = qrCode.Split("    ");
            var partNo = results[2].Split(":")[1].Trim() + ':' + results[0].Split(":")[1].Trim();
            var Batch = results[4].Split(":")[1].Trim() + ':' + results[0].Split(":")[1].Trim();
            var model = _repoIngredient.FindAll().FirstOrDefault(x => x.PartNO.Equals(partNo));
            var supModel = _repoSupplier.GetAll();
            var ProductionDates = results[5].Split(":")[1].Trim().ToDateTime();
            var exp = ProductionDates.AddMonths(3);
            var currentDate = DateTime.Now;
            var data = await CreateIngredientInfo(new IngredientInfo
            {
                Name = model.Name,
                ExpiredTime = exp.Date,
                ManufacturingDate = ProductionDates.Date,
                SupplierName = supModel.FirstOrDefault(s => s.ID == model.SupplierID).Name,
                Qty = model.Unit.ToInt(),
                Batch = Batch,
                Consumption = "0",
                Code = model.MaterialNO,
                IngredientID = model.ID,
                UserID = userid,
                BuildingName = building

            });

            // check trong bang ingredientReport xem đã tồn tại code hay chưa , nếu có tồn tại 
            if (await _repoIngredientInfoReport.CheckBarCodeExists(partNo))
            {
                // check tiep trong bang ingredientReport xem co du lieu chua 
                var result = _repoIngredientInfoReport.FindAll().FirstOrDefault(x => x.Code == partNo && x.Batch == Batch && x.CreatedDate.Date == currentDate.Date);

                // nếu khác Null thi update lai
                if (result != null)
                {
                    result.Qty = model.Unit.ToInt() + result.Qty;
                    await UpdateIngredientInfoReport(result);
                }

                // nếu bằng null thì tạo mới IngredientReport
                else
                {
                    await CreateIngredientInfoReport(new IngredientInfoReport
                    {
                        Name = model.Name,
                        ExpiredTime = ProductionDates.Date.AddMonths(3),
                        ManufacturingDate = ProductionDates.Date,
                        SupplierName = supModel.FirstOrDefault(s => s.ID == model.SupplierID).Name,
                        Qty = model.Unit.ToInt(),
                        Consumption = "0",
                        Code = model.MaterialNO,
                        Batch = Batch,
                        IngredientInfoID = data.ID,
                        UserID = userid,
                        BuildingName = building
                    });
                }
            }

            // nếu chưa tồn tại thì thêm mới
            else
                await CreateIngredientInfoReport(new IngredientInfoReport
                {
                    Name = model.Name,
                    ExpiredTime = ProductionDates.Date.AddMonths(3),
                    ManufacturingDate = ProductionDates.Date,
                    SupplierName = supModel.FirstOrDefault(s => s.ID == model.SupplierID).Name,
                    Qty = model.Unit.ToInt(),
                    Batch = Batch,
                    Consumption = "0",
                    Code = model.MaterialNO,
                    IngredientInfoID = data.ID,
                    UserID = userid,
                    BuildingName = building
                });
            return true;
        }
        public async Task<object> ScanQRCodeOutput(string qrCode, string building, int userid)
        {

            var dayAndBatch = string.Empty;
            var pattern = @"((\d*)-(\w*)-)*";
            var obj = new string[] { };
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

            Match m = r.Match(qrCode);
            if (m.Success)
            {
                dayAndBatch = m.Groups[1].ToSafetyString();
                obj = dayAndBatch.Split('-');
            }

            var results = qrCode.Split("    ");
            //var partNo = results[2].Split(":")[1].Trim();
            //var Batch = results[4].Split(":")[1].Trim();
            //var model = _repoIngredient.FindAll().FirstOrDefault(x => x.PartNO.Equals(partNo));
            //var supModel = _repoSupplier.GetAll();
            //var ProductionDates = results[5].Split(":")[1].Trim().ToDateTime();
            //var exp = ProductionDates.AddMonths(3);
            //var currentDate = DateTime.Now;

            // load tat ca supplier
            var supModel = _repoSupplier.GetAll();
            // lay gia tri "barcode" trong chuỗi qrcode được chuyền lên
            var partNo = results[2].Split(":")[1].Trim() + ':' + results[0].Split(":")[1].Trim();
            // tim ID của ingredient
            var ingredientID = _repoIngredient.FindAll().FirstOrDefault(x => x.PartNO.Equals(partNo)).ID;
            // Find ingredient theo ingredientID vừa tìm được ở trên
            var model = _repoIngredient.FindById(ingredientID);
            // lấy giá trị "Batch" trong chuỗi qrcode được chuyền lên
            var Batch = results[4].Split(":")[1].Trim() + ':' + results[0].Split(":")[1].Trim();

            var currentDay = DateTime.Now;


            // check trong bang ingredientReport xem đã tồn tại code hay chưa , nếu có tồn tại 
            if (await _repoIngredientInfo.CheckBarCodeExists(partNo))
            {
                // check tiep trong bang ingredientReport xem co du lieu chua 
                var checkStatus = _repoIngredientInfo.FindAll().Where(x => x.Code == partNo && x.BuildingName == building && x.Batch == Batch && x.CreatedDate.Date == currentDay.Date && x.Status == false).OrderBy(y => y.CreatedTime).FirstOrDefault();
                // nếu khác Null thi update lai
                if (checkStatus != null)
                {
                    checkStatus.Status = true;
                    await UpdateIngredientInfo(checkStatus);
                }
                else
                {
                    return new
                    {
                        status = false,
                        message = "Đã dùng hết !"
                    };
                }
            }

            // nếu chưa tồn tại thì thêm mới
            else
            {
                return new
                {
                    status = false,
                    message = "Hãy scan QR Code hàng nhập trước :) !"
                };
            }

            return true;
        }

        public Task<object> ScanQRCodeFromChemialWareHouseDate(string qrCode, string start, string end)
        {
            // var supModel = _repoSupplier.GetAll();
            // var modelID = _repoIngredient.FindAll().FirstOrDefault(x => x.Code.Equals(qrCode) && x.isShow == true).ID;
            // var model = _repoIngredient.FindById(modelID);
            // var resultStart = DateTime.Now;
            // var resultEnd = DateTime.Now;
            // if (await _repoIngredientInfo.CheckBarCodeExists(qrCode))
            // {
            //     var result = _repoIngredientInfo.FindAll().FirstOrDefault(x => x.Code == qrCode && x.CreatedDate <= resultEnd.Date && x.CreatedDate >= resultStart.Date);

            //     if (result != null)
            //     {
            //         result.Qty = model.Unit.ToInt() + result.Qty;
            //         await UpdateIngredientInfo(result);
            //     }
            //     else
            //     {
            //         var data = await CreateIngredientInfo(new IngredientInfo
            //         {

            //             Name = model.Name,
            //             ExpiredTime = model.ExpiredTime,
            //             ManufacturingDate = model.ManufacturingDate,
            //             SupplierName = supModel.FirstOrDefault(s => s.ID == model.SupplierID).Name,
            //             Qty = model.Unit.ToInt(),
            //             Consumption = "0",
            //             Code = model.Code
            //         });
            //     }
            // }
            // else
            // {
            //     var data = await CreateIngredientInfo(new IngredientInfo
            //     {
            //         Name = model.Name,
            //         ExpiredTime = model.ExpiredTime,
            //         ManufacturingDate = model.ManufacturingDate,
            //         SupplierName = supModel.FirstOrDefault(s => s.ID == model.SupplierID).Name,
            //         Qty = model.Unit.ToInt(),
            //         Consumption = "0",
            //         Code = model.Code
            //     });
            // }
            throw new NotImplementedException();
        }

        public async Task<IngredientInfo> CreateIngredientInfo(IngredientInfo data)
        {
            try
            {
                _repoIngredientInfo.Add(data);
                await _repoIngredientInfo.SaveAll();
                return data;
            }
            catch (Exception)
            {

                return data;
            }
        }

        public async Task<IngredientInfoReport> CreateIngredientInfoReport(IngredientInfoReport data)
        {
            try
            {
                _repoIngredientInfoReport.Add(data);
                await _repoIngredientInfoReport.SaveAll();
                return data;
            }
            catch (Exception)
            {

                return data;
            }
        }

        public async Task<IngredientInfo> UpdateIngredientInfo(IngredientInfo data)
        {
            try
            {
                _repoIngredientInfo.Update(data);
                await _repoIngredientInfo.SaveAll();
                return data;
            }
            catch (Exception)
            {

                return data;
            }
        }

        public async Task<IngredientInfoReport> UpdateIngredientInfoReport(IngredientInfoReport data)
        {
            try
            {
                _repoIngredientInfoReport.Update(data);
                await _repoIngredientInfoReport.SaveAll();
                return data;
            }
            catch (Exception)
            {

                return data;
            }
        }

        public async Task<bool> UpdateConsumptionChemialWareHouse(string qrCode, int consump)
        {
            try
            {
                if (await _repoIngredientInfo.CheckBarCodeExists(qrCode))
                {
                    var result = _repoIngredientInfo.FindAll().FirstOrDefault(x => x.Code == qrCode);
                    result.Consumption = consump.ToString();
                    if (result.Qty != 0)
                    {
                        result.Qty = result.Qty - consump;
                    }
                    var data = await UpdateIngredientInfo(result);
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<bool> UpdateConsumptionIngredientReport(string qrCode, string batch, int consump)
        {
            try
            {
                if (await _repoIngredientInfoReport.CheckBarCodeExists(qrCode))
                {
                    // var resultEnd = DateTime.Now;
                    var result = _repoIngredientInfoReport.FindAll().FirstOrDefault(x => x.Code == qrCode && x.Batch == batch);
                    result.Consumption = consump.ToSafetyString();
                    // if (result.Qty != 0)
                    // {
                    //     result.Qty = result.Qty - consump;
                    // }
                    var data = await UpdateIngredientInfoReport(result);
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<bool> UpdateConsumptionOfBuildingIngredientReport(UpdateConsumpDto entity)
        {
            try
            {
                if (await _repoIngredientInfoReport.CheckBarCodeExists(entity.qrCode))
                {
                    var currentDay = DateTime.Now;
                    var result = _repoIngredientInfoReport.FindAll().FirstOrDefault(x => x.Code == entity.qrCode && x.Batch == entity.batch && x.BuildingName == entity.buildingName && x.CreatedDate.Date == currentDay.Date);
                    result.Consumption = entity.consump;
                    // if (result.Qty != 0)
                    // {
                    //     result.Qty = result.Qty - consump;
                    // }

                    var data = await UpdateIngredientInfoReport(result);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> DeleteIngredientInfo(int id, string code, int qty, string batch)
        {
            var item = _repoIngredientInfo.FindById(id);
            _repoIngredientInfo.Remove(item);
            await Update(code, qty, batch);
            return await _repoIngredientInfo.SaveAll();
        }

        public async Task<bool> DeleteIngredientInfoReport(int id)
        {
            var item = _repoIngredientInfoReport.FindAll().FirstOrDefault(x => x.IngredientInfoID == id).ID;
            var item2 = _repoIngredientInfoReport.FindById(item);
            _repoIngredientInfoReport.Remove(item2);

            return await _repoIngredientInfoReport.SaveAll();
        }

        public async Task<bool> Update(string code, int qty, string batch)
        {
            try
            {
                if (await _repoIngredientInfoReport.CheckBarCodeExists(code))
                {
                    var result = _repoIngredientInfoReport.FindAll().FirstOrDefault(x => x.Code == code && x.Batch == batch);
                    result.Qty = result.Qty - qty;
                    if (result.Qty == 0)
                    {
                        _repoIngredientInfoReport.Remove(result);
                        await _repoIngredientInfoReport.SaveAll();
                    }
                    else
                    {
                        await UpdateIngredientInfoReport(result);
                    }
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> CheckIncoming(string ingredientName, string batch, string building)
        {
            var from = DateTime.Now.AddDays(-5).Date;
            var to = DateTime.Now.Date;
            return await _repoIngredientInfo.FindAll()
            .Where(x => x.CreatedDate.Date >= from && x.CreatedDate.Date >= to).AnyAsync(x => x.BuildingName.Equals(building) && x.Name.Trim().Equals(ingredientName.Trim()) && x.Batch.Equals(batch));
        }

        // them boi henry

        public async Task<List<IngredientInfoDto>> GetAllIngredientInfoByBuildingAsync(string building)
        {
            var resultStart = DateTime.Now;
            var resultEnd = DateTime.Now;
            return await _repoIngredientInfo.FindAll()
                                            .Where(x => x.CreatedDate.Date >= resultStart.Date 
                                                        && x.CreatedDate.Date <= resultEnd.Date
                                                        && x.BuildingName == building
                                                        )
                                            .ProjectTo<IngredientInfoDto>(_configMapper)
                                            .OrderByDescending(x => x.ID)
                                            .ToListAsync();
        }

        public async Task<List<IngredientInfoDto>> GetAllIngredientInfoOutputByBuildingAsync(string building)
        {
            var resultStart = DateTime.Now;
            var resultEnd = DateTime.Now;
            return await _repoIngredientInfo.FindAll()
                                            .Where(x => x.CreatedDate.Date >= resultStart.Date 
                                                     && x.CreatedDate.Date <= resultEnd.Date
                                                     && x.BuildingName == building
                                                     )
                                            .ProjectTo<IngredientInfoDto>(_configMapper)
                                            .OrderByDescending(x => x.ID)
                                            .ToListAsync();
        }

        public async Task<ResponseDetail<object>> Rate()
        {
            var model = await _repoIngredient.FindAll(x => x.isShow).Select(x=> new {
                x.MaterialNO, x.VOC, x.DaysToExpiration, x.Unit, x.ExpiredTime
            }).ToListAsync();
            var total = model.Count;
            var complete = model.Where(x => x.MaterialNO != "" && x.VOC > 0 && x.ExpiredTime > 0 && x.DaysToExpiration > 0 && x.Unit > 0
            ).Count();
            var rate = Math.Round((complete / (double)total) * 100);
           return new ResponseDetail<object>{
               Data = new {
                   Total = total,
                   Complete = complete,
                   CompleteRate =rate
               },
               Status = true
           };
        }

        
    }
}