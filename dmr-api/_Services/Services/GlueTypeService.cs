using dmr_api.Models;
using DMR_API._Repositories.Interface;
using DMR_API._Services.Interface;
using DMR_API.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace DMR_API._Services.Services
{
    public class GlueTypeService : IGlueTypeService
    {
        private readonly IGlueTypeRepository _repoGlueType;

        public GlueTypeService(IGlueTypeRepository repoGlueType)
        {
            _repoGlueType = repoGlueType;
        }

        public async Task<bool> CreateChild(GlueType model)
        {
            try
            {
                _repoGlueType.Add(model);

                return await _repoGlueType.SaveAll();
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> Update(GlueType model)
        {
            try
            {
                _repoGlueType.Update(model);

                return await _repoGlueType.SaveAll();
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> CreateParent(GlueType model)
        {
            try
            {
                _repoGlueType.Add(model);

                return await _repoGlueType.SaveAll();
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<GlueType>> GetAll() => await _repoGlueType.GetAll().ToListAsync();

        public async Task<IEnumerable<HierarchyNode<GlueType>>> GetAllAsTreeView() => (await _repoGlueType.GetAll().ToListAsync()).AsHierarchy(x => x.ID, y => y.ParentID);

        public async Task<List<GlueType>> GetAllByLevel(int parentID) => await _repoGlueType.FindAll(x => x.Level == parentID).ToListAsync();


        public async Task<List<GlueType>> GetAllByParentID(int parentID) => await _repoGlueType.FindAll(x => x.ParentID == parentID).ToListAsync();

        public async Task<bool> Delete(int id)
        {
            try
            {
                var model = _repoGlueType.FindById(id);
                if (model is null) return false;
                _repoGlueType.Remove(model);
                return await _repoGlueType.SaveAll();
            }
            catch
            {
                return false;
            }
        }
    }
}
