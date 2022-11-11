using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
    public interface IToDoListService
    {
        Task<ToDoListForReturnDto> Done(int buildingID);
        /// <summary>
        /// Kiểm tra đang thêm kế hoạch làm việc cho xưởng đế (STF) hay là thành hình (ASY)
        /// </summary>
        /// <param name="plans">Dach dánh ID kế hoạch làm việc.</param>
        /// <returns>True là xưởng đế (STF), ngược lại thành hình (ASY)</returns>
        Task<bool> CheckBuildingType(List<int> plans);

        Task<ToDoListForReturnDto> DispatchDone(int buildingID);

        /// <summary>
        /// Tạo danh sách việc làm cho thành hình (ASY)
        /// </summary>
        /// <param name="plans">Danh sách ID kế hoạch làm việc.</param>
        /// <returns>Trả về 1 object chứa status và message</returns>
        Task<object> GenerateToDoList(List<int> plans);

        /// <summary>
        ///  Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
        /// </summary>
        /// <param name="glueNameID"></param>
        /// <param name="glueID"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        Task<ToDoListForReturnDto> ToDo(int buildingID);

        Task<ToDoListForReturnDto> ToDoAddition(int buildingID);

        Task<DispatchListForReturnDto> DispatchAddition(int buildingID);
        
        Task<DispatchListForReturnDto> DispatchList(int buildingID);

        Task<DispatchListForReturnDto> DispatchListDelay(int buildingID);
        /// <summary>
        ///  Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
        /// </summary>
        /// <param name="glueNameID"></param>
        /// <param name="glueID"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        Task<ToDoListForReturnDto> Delay(int buildingID);
        Task<bool> AddRange(List<ToDoList> toDoList);
        Task<MixingInfo> Mix(MixingInfoForCreateDto mixForToDoListDto);
        void UpdateDispatchTimeRange(ToDoListForUpdateDto model);
        void UpdateMixingTimeRange(ToDoListForUpdateDto model);
        void UpdateStiringTimeRange(ToDoListForUpdateDto model);
        Task<Byte[]> ExportExcelToDoListByBuilding(int buildingID);
        Task<Byte[]> ExportExcelNewReportOfDonelistByBuilding(int buildingID);
        Task<Byte[]> ExportExcelToDoListWholeBuilding();
        Task<Byte[]> ExportExcelToDoListWholeBuilding(DateTime startTime, DateTime endTime);

        MixingInfo PrintGlue(int mixingInfoID);
        MixingInfo FindPrintGlue(int mixingInfoID);
        MixingInfo PrintGlueDispatchList(int mixingInfoID, int dispatchListID);
        Task<MixingInfo> PrintGlueDispatchListAsync(int mixingInfoID, int glueNameID, DateTime estimatedStartTime, DateTime estimatedFinsihTime);
        Task<ResponseDetail<string>> UpdateMixingInfoDispatchList(int mixingInfoID, int glueNameID, DateTime estimatedStartTime, DateTime estimatedFinsihTime);
        Task<ResponseDetail<string>> FinishDispatch(FinishDispatchParams obj);
        Task<ResponseDetail<string>> UpdateDispatchDetail(DispatchDetailForUpdateDto dispatchDto);

        Task<List<DispatchDetailDto>> GetDispatchDetail(int buildingID, int glueNameID, DateTime estimatedStartTime, DateTime estimatedFinsihTime);
        Task<object> Dispatch(DispatchParams todolistDto);
        Task<bool> Cancel(ToDoListForCancelDto todolistID);
        Task<bool> CancelRange(List<ToDoListForCancelDto> todolistIDList);
        Task<bool> SendMail(Byte[] data, DateTime time);
        bool UpdateStartStirTimeByMixingInfoID(int mixingInfoID);
        bool UpdateFinishStirTimeByMixingInfoID(int mixingInfoID);
        MixingDetailForResponse GetMixingDetail(MixingDetailParams obj);

        /// <summary>
        /// Tạo ra danh sách dispatch theo plan
        /// </summary>
        /// <param name="plans"></param>
        /// <returns></returns>
        Task<object> GenerateDispatchList(List<int> plans);

        /// <summary>
        ///  Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
        /// </summary>
        /// <param name="glueNameID"></param>
        /// <param name="glueID"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        /// 

        Task<object> Addition(int glueNameID, int glueID, DateTime start, DateTime end);
        Task<ResponseDetail<object>> AdditionDispatch(int glueNameID);
        //Thêm bởi Quỳnh (Leo 2/2/2021 11:46)
        Task<object> MixedHistory(int MixingInfoID);
        //Thêm bởi Quỳnh (Leo 2/2/2021 11:46)


        Task<List<DispatchListDto>> GetDispatchListDetail(int glueNameID, string estimatedStartTime, string estimatedFinishTime);
        Task<ResponseDetail<object>> UpdateDispatchDetail(DispatchListForUpdateDto update);
        Task<List<MixingInfo>> GetMixingInfoHistory(int buildingID, int glueNameID, string estimatedStartTime, string estimatedFinishTime);
        Task<object> AddOvertime(List<int> plans);
        Task<object> RemoveOvertime(List<int> plans);

    }
}
