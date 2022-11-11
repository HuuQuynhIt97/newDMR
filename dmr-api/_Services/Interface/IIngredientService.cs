using dmr_api.Models;
using DMR_API.DTO;
using DMR_API.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
   public interface IIngredientService: IECService<IngredientDto>
    {
        Task<bool> CheckExists(int id); 
        Task<bool> CheckBarCodeExists(string code);
        Task<bool> Add1(IngredientDto1 ingredientIngredientDto);
        Task<string> AddRangeAsync(List<IngredientForImportExcelDto> model);
        Task<bool> DeleteIngredientInfo(int id, string code , int qty ,string batch);
        Task<bool> UpdatePrint(QrPrintDto entity);
        Task<IngredientDto> ScanQRCode(string qrCode);
        Task<List<IngredientInfoDto>> GetAllIngredientInfoAsync();
        Task<List<IngredientInfoDto>> GetAllIngredientInfoByBuildingAsync(string building);
        Task<List<IngredientInfoDto>> GetAllIngredientInfoOutputByBuildingAsync(string building);

        Task<List<IngredientInfoDto>> GetAllIngredientInfoOutputAsync();
        Task<List<IngredientInfoDto>> GetAllIngredientInfoByBuildingNameAsync(string name);
        Task<List<IngredientInfoReportDto>> GetAllIngredientInfoReportAsync();
        Task<List<IngredientInfoReportDto>> GetAllIngredientInfoReportByBuildingNameAsync(string name);
        Task<object> GetAllIngredientReportByRange(DateTime min, DateTime max);
        Task<object> GetAllIngredientReportByRangeWithBuilding(DateTime min, DateTime max, string name);
        Task<List<GlueType>> GetAllGlueTypeAsync();
        Task<object> ScanQRCodeFromChemialWareHouse(string qrCode, string building, int userid);
        //Update 08/04/2021 - Leo
        Task<object> ScanQRCodeFromChemialWareHouseV1(ScanQrCodeDto entity);
        Task<object> ScanQRCodeOutputV1(ScanQrCodeDto entity);
        //End update
        Task<object> ScanQRCodeOutput(string qrCode, string building, int userid);
        Task<object> ScanQRCodeFromChemialWareHouseDate(string qrCode, string start , string end);
        Task<bool> UpdateConsumptionChemialWareHouse(string qrCode , int consump);

        Task<bool> UpdateConsumptionIngredientReport(string qrCode, string batch, int consump );

        Task<bool> UpdateConsumptionOfBuildingIngredientReport(UpdateConsumpDto entity );

        Task<bool> CheckExistsName(string name);
        Task<ResponseDetail<object>> Rate();
        Task<bool> CheckIncoming(string ingredientName, string batch, string building);

    }
}
