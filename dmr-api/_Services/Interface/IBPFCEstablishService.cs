using DMR_API.DTO;
using DMR_API.Helpers.Enum;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
    public interface IBPFCEstablishService : IECService<BPFCEstablishDto>
    {
        Task<object> GetDetailBPFC(int bpfcID);
        Task<object> GetDoneBPFC();
        Task<object> GetUndoneBPFC();
        Task ImportExcel(List<BPFCEstablishDtoForImportExcel> bPFCEstablishDtos);
        Task<object> Approval(int bpfcID, int userid);
        Task<object> Done(int bpfcID, int userid);
        Task<object> Release(int bpfcID, int userid);
        Task<object> Reject(int bpfcID, int userid);
        Task SendMailForPIC(string email);
        Task<List<BPFCHistoryDto>> GetAllHistoryAsync();
        Task<bool> Create(BPFCHistoryDto entity);
        Task<bool> UpdateBPFCHistory(BPFCHistory entity);
        Task<object> LoadBPFCHistory(int bpfcID);
        Task<List<BPFCStatusDto>> FilterByApprovedStatus();
        Task<List<BPFCStatusDto>> FilterByFinishedStatus();
        Task<List<BPFCStatusDto>> FilterByNotApprovedStatus();
        Task<List<BPFCStatusDto>> DefaultFilter();
        Task<List<BPFCStatusDto>> RejectedFilter();
        Task<List<BPFCRecordDto>> GetAllBPFCRecord(Status status, string startBuildingDate, string endBuildingDate);
        Task<List<BPFCStatusDto>> GetAllBPFCStatus();
        Task<List<BPFCStatusDto>> GetAllBPFCEstablish();
        Task<object> GetAllBPFCByBuildingID(int buildingID);
        Task<List<string>> GetGlueByBPFCID(int bpfcID);
        Task<BPFCEstablishDto> GetBPFCID(GetBPFCIDDto bpfcInfo);
        Task<bool> UpdateSeason(BPFCEstablishUpdateSeason entity);
        Task<bool> UpdateDueDate(BPFCEstablishUpdateDueDate entity);
    }
}
