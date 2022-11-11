using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
    public interface IBottomFactoryService
    {
        Task<ToDoListForReturnDto> ToDoList(int buildingID);
        Task<ToDoListForReturnDto> DoneList(int buildingID);
        Task<ToDoListForReturnDto> UndoneList(int buildingID);

        Task<DispatchListForReturnDto> DispatchList(int buildingID);
        Task<ToDoListForReturnDto> DelayList(int buildingID);
        Task<ToDoListForReturnDto> EVA_UVList(int buildingID);

        Task<ResponseDetail<BottomFactoryForReturnDto>> ScanQRCode(ScanQRCodeParams printParams);
        Task<ResponseDetail<BottomFactoryForReturnDto>> ScanQRCodeV110(ScanQRCodeParams printParams);
        Task<ResponseDetail<List<SubpackageDto>>> GenerateScanByNumber(GenerateSubpackageParams obj);
        Task<ResponseDetail<object>> AddDispatch(AddDispatchParams bfparams);
        Task<ResponseDetail<object>> UpdateDispatch(UpdateDispatchParams update);
        Task<BottomFactoryForDispatchDto> GetAllDispatch(DispatchParamsDto obj);

        Task<MixingInfo> GetMixingInfo(int mixingInfoID);
        Task<SubpackageCapacity> GetSubpackageCapacity();
        Task<Subpackage> GetSubpackageLatestSequence(string batch, int glueNameID, int buildingID);

        Task<bool> SaveSubpackage(SubpackageParam obj );


        /// <summary>
        /// Tạo danh sách việc làm cho xưởng đế (STF)
        /// </summary>
        /// <param name="plans">Danh sách ID kế hoạch làm việc.</param>
        /// <returns>Trả về 1 object chứa status và message</returns>
        Task<object> GenerateToDoList(List<int> plans);
        Task<bool> Print(List<int> subpackages);

    }
}
