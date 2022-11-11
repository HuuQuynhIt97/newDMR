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
    public class ArtProcessService : IArtProcessService
    {

        private readonly IArtProcessRepository _repoArticalNo;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public ArtProcessService(IArtProcessRepository repoArticalNo, IMapper mapper, MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoArticalNo = repoArticalNo;

        }

        public async Task<bool> Add(ArtProcessDto model)
        {
            var artProcess = _mapper.Map<ArtProcess>(model);
            _repoArticalNo.Add(artProcess);
            return await _repoArticalNo.SaveAll();
        }

        public async Task<PagedList<ArtProcessDto>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoArticalNo.FindAll().ProjectTo<ArtProcessDto>(_configMapper).OrderByDescending(x => x.ID);
            return await PagedList<ArtProcessDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        public async Task<PagedList<ArtProcessDto>> Search(PaginationParams param, object text)
        {
            var lists = _repoArticalNo.FindAll().ProjectTo<ArtProcessDto>(_configMapper)
            .Where(x => x.ID.Equals(text.ToInt()))
            .OrderByDescending(x => x.ID);
            return await PagedList<ArtProcessDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        public async Task<bool> Delete(object id)
        {
            var ArtProcess = _repoArticalNo.FindById(id);
            _repoArticalNo.Remove(ArtProcess);
            return await _repoArticalNo.SaveAll();
        }

        public async Task<bool> Update(ArtProcessDto model)
        {
            var artProcess = _mapper.Map<ArtProcess>(model);
            _repoArticalNo.Update(artProcess);
            return await _repoArticalNo.SaveAll();
        }

        public async Task<List<ArtProcessDto>> GetAllAsync()
        {
            return await _repoArticalNo.FindAll().ProjectTo<ArtProcessDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        public ArtProcessDto GetById(object id)
        {
            return _mapper.Map<ArtProcess, ArtProcessDto>(_repoArticalNo.FindById(id));
        }

        public async Task<List<ArtProcessDto>> GetArtProcessByArticleNoID(int articleNoID)
        {
            var model = await _repoArticalNo.FindAll().Where(x => x.ArticleNoID == articleNoID).ProjectTo<ArtProcessDto>(_configMapper).ToListAsync();
            return model;
        }
    }
}