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

namespace DMR_API._Services.Services
{
    public class MixingService : IMixingService
    {
        private readonly IMixingRepository _repoMixing;

        public MixingService(IMixingRepository repoMixing)
        {
            _repoMixing = repoMixing;
        }

        public async Task<bool> AddOrUpdate(int mixingInfoID)
        {
            var isValid = await _repoMixing.FindAll().CountAsync(x => x.MachineID == "EM001");
            if (isValid > 0)
            {
                // update
                var item = await _repoMixing.FindAll().FirstOrDefaultAsync();
                item.MixingInfoID = mixingInfoID;
                return await _repoMixing.SaveAll();

            }
            else
            {
                // add new
                _repoMixing.Add(new Mixing { MixingInfoID = mixingInfoID, MachineID = "EM001" });
                return await _repoMixing.SaveAll();
            }
        }
    }
}