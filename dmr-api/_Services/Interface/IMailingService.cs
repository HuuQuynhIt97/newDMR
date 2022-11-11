using DMR_API.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
    public interface IMailingService : IECService<MailingDto>
    {
        Task<bool> AddRange(List<MailingDto> model);
        Task<bool> UpdateRange(List<MailingDto> model);
        Task<bool> DeleteRange(List<MailingDto> model);
        Task<List<MailingDto>> GetAllByFrequencyAndReport(string frequency, string report);
        Task<bool> CheckExists(string frequency, string report);
    }
}
