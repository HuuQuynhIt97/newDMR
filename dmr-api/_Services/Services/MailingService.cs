using AutoMapper;
using AutoMapper.QueryableExtensions;
using DMR_API._Repositories.Interface;
using DMR_API._Services.Interface;
using DMR_API.Data;
using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using DMR_API.SignalrHub;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DMR_API._Services.Services
{
    public class MailingService : IMailingService
    {
        private readonly IMailingRepository _repoMailing;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        private readonly IHubContext<ECHub> _hubContext;
        private readonly HttpClient client;

        public MailingService(IMailingRepository repoMailing,
            IMapper mapper,
            MapperConfiguration configMapper,
            IHubContext<ECHub> hubContext,
            IHttpClientFactory clientFactory)
        {
            _configMapper = configMapper;
            _hubContext = hubContext;
            _mapper = mapper;
            _repoMailing = repoMailing;
            client = clientFactory.CreateClient("default");
        }

        public async Task<bool> Add(MailingDto model)
        {
            if (_repoMailing.FindAll(x => x.UserID == model.UserID).Any())
            {
                return false;
            }
            var item = _mapper.Map<Mailing>(model);
            _repoMailing.Add(item);

            var mailingList = await GetAllAsync();
            await _hubContext.Clients.Group("Mailing").SendAsync("ReceiveMailing", mailingList);
            var res = await _repoMailing.SaveAll();
            return res;
        }

        public async Task<bool> AddRange(List<MailingDto> model)
        {
            var item = _mapper.Map<List<Mailing>>(model);
            _repoMailing.AddRange(item);
          
            var res = await _repoMailing.SaveAll();
            var mailingList = await GetAllByFrequencyAndReport(item.First().Frequency, item.First().Report);
            await _hubContext.Clients.Group("Mailing").SendAsync("ReceiveMailing", mailingList);
            return res;
        }


        public async Task<bool> UpdateRange(List<MailingDto> model)
        {
            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var groupBy = model.GroupBy(x => new { x.Frequency, x.Report }).ToList();
                    foreach (var item in groupBy)
                    {
                        var mailingList = await _repoMailing.FindAll(x => x.Report == item.Key.Report && x.Frequency == item.Key.Frequency).ToListAsync();
                        
                        if (item.Count() > 0 && mailingList.Count > 0 && string.Join("", item.OrderBy(x=>x.UserID).Select(x => x.UserID).ToArray()) != string.Join("", mailingList.OrderBy(x => x.UserID).Select(x=>x.UserID).ToArray()))
                        {
                            // xoa het cai cu add lai
                            var add = _mapper.Map<List<Mailing>>(item);
                            add.ForEach(x => {
                                x.ID = 0;
                                x.TimeSend = item.First().TimeSend;
                            });
                            _repoMailing.RemoveMultiple(mailingList);
                            await _repoMailing.SaveAll();

                            _repoMailing.AddRange(add);
                            await _repoMailing.SaveAll();

                            var mailingList2 = await GetAllByFrequencyAndReport(add.First().Frequency, add.First().Report);
                            await _hubContext.Clients.Group("Mailing").SendAsync("RescheduleJob", mailingList2);

                        } else
                        {
                            var update = _mapper.Map<List<Mailing>>(item);
                            update.ForEach(x => {
                                x.TimeSend = item.First().TimeSend;
                            });
                            _repoMailing.UpdateRange(update);
                            await _repoMailing.SaveAll();

                            var mailingList2 = await GetAllByFrequencyAndReport(update.First().Frequency, update.First().Report);
                            await _hubContext.Clients.Group("Mailing").SendAsync("RescheduleJob", mailingList2);
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
        public async Task<bool> Delete(object id)
        {
            if (!_repoMailing.FindAll(x => x.UserID == id.ToInt()).Any())
            {
                return false;
            }
            var item = _repoMailing.FindById(id);
            _repoMailing.Remove(item);
            return await _repoMailing.SaveAll();

        }

        public async Task<List<MailingDto>> GetAllAsync()
        {
            var response = await client.GetAsync($"Users/GetAll");
            string json = response.Content.ReadAsStringAsync().Result;
            var users = JsonConvert.DeserializeObject<List<UserDto>>(json);
            var model = await _repoMailing.FindAll().ProjectTo<MailingDto>(_configMapper).ToListAsync();
            var result = from a in users
                         join b in model on a.ID equals b.UserID
                         select new MailingDto
                         {
                             ID = b.ID,
                             UserID = b.UserID,
                             UserName = a.Username,
                             Report = b.Report,
                             Email = b.Email,
                             PathName = b.PathName,
                             TimeSend = b.TimeSend,
                             Frequency = b.Frequency
                         };
            var groupBy = result.ToList().GroupBy(x => new { x.Frequency, x.Report });
            var list = new List<MailingDto>();
            foreach (var item in groupBy)
            {
                list.Add(new MailingDto
                {
                    UserNames = item.Select(x => x.UserName).ToList(),
                    UserIDList = item.Select(x => x.UserID).ToList(),
                    UserList = item.Select(x => new UserList { MailingID = x.ID, ID = x.UserID, Email = x.Email}).ToList(),
                    TimeSend = item.First().TimeSend,
                    Frequency = item.Key.Frequency,
                    Report = item.Key.Report,
                    Email = item.First().Email,
                    PathName = item.First().PathName,
                });
            }

            return list;
        }

        public MailingDto GetById(object id)
        {
            return  _repoMailing.FindAll().ProjectTo<MailingDto>(_configMapper).FirstOrDefault();
        }

        public Task<PagedList<MailingDto>> GetWithPaginations(PaginationParams param)
        {
            throw new NotImplementedException();
        }

        public Task<PagedList<MailingDto>> Search(PaginationParams param, object text)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Update(MailingDto model)
        {
            var item = _repoMailing.FindById(model.ID);
            if (item is null) return false;
            item.UserID = model.UserID;
            item.TimeSend = model.TimeSend;
            item.Email = model.Email;
            _repoMailing.Update(item);
            return await _repoMailing.SaveAll();
        }

        public async Task<bool> CheckExists(string frequency, string report)
        {
            return await _repoMailing.FindAll(x => x.Report == report && x.Frequency == frequency).AnyAsync();

        }

        public async Task<List<MailingDto>> GetAllByFrequencyAndReport(string frequency, string report)
        {
            var response = await client.GetAsync($"Users/GetAll");
            string json = response.Content.ReadAsStringAsync().Result;
            var users = JsonConvert.DeserializeObject<List<UserDto>>(json);
            var model = await _repoMailing.FindAll(x=> x.Frequency == frequency && x.Report == report).ProjectTo<MailingDto>(_configMapper).ToListAsync();
            var result = from a in users
                         join b in model on a.ID equals b.UserID
                         select new MailingDto
                         {
                             ID = b.ID,
                             UserID = b.UserID,
                             UserName = a.Username,
                             Report = b.Report,
                             Email = b.Email,
                             PathName = b.PathName,
                             TimeSend = b.TimeSend,
                             Frequency = b.Frequency
                         };
            var groupBy = result.ToList().GroupBy(x => new { x.Frequency, x.Report });
            var list = new List<MailingDto>();
            foreach (var item in groupBy)
            {
                list.Add(new MailingDto
                {
                    UserNames = item.Select(x => x.UserName).ToList(),
                    UserIDList = item.Select(x => x.UserID).ToList(),
                    UserList = item.Select(x => new UserList { MailingID = x.ID, ID = x.UserID, Email = x.Email }).ToList(),
                    TimeSend = item.First().TimeSend,
                    Frequency = item.Key.Frequency,
                    Report = item.Key.Report,
                    PathName = item.First().PathName,
                    Email = item.First().Email,
                });
            }

            return list;
        }

        public async Task<bool> DeleteRange(List<MailingDto> model)
        {
            var groupBy = model.GroupBy(x => new { x.Frequency, x.Report }).ToList();
            var listID = new List<int>();
            foreach (var item in groupBy)
            {
                listID.Add(item.First().ID);
            }
            var removeList = await _repoMailing.FindAll(x => listID.Contains(x.ID)).ToListAsync();
            if (removeList.Count > 0)
            {
                var mailingList = await GetAllByFrequencyAndReport(removeList.First().Frequency, removeList.First().Report);
                _repoMailing.RemoveMultiple(removeList);
                var res = await _repoMailing.SaveAll();
                await _hubContext.Clients.Group("Mailing").SendAsync("KillScheduler", mailingList);
                return res;
            }
         
            return false;
        }
    }
}
