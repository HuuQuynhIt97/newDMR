using DMR_API.DTO;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
    public interface IMixingInfoService
    {
        MixingInfo Mixing(MixingInfoForCreateDto mixing);
        MixingInfo GetByID(int ID);
        MixingInfo AddMixingInfo(MixingInfoForAddDto mixing);
        Task<List<MixingInfoDto>> GetMixingInfoByGlueName(string glueName, int buildingID);
        Task<object> Stir(string glueName);
        Task<object> GetRPM(int mixingInfoID, string building, string startTime, string endTime);
        Task<object> GetRPM(int stirID);
        object GetRawData(int machineID, string startTime, string endTime);
        Task<object> GetRPMByMachineID(int machineID, string startTime, string endTime);
        Task<object> GetRPMByMachineCode(string machineCode, string startTime, string endTime);

    }
}
