using dmr_api.Models;
using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
   public interface IGlueTypeService 
    {
        Task<IEnumerable<HierarchyNode<GlueType>>> GetAllAsTreeView();
        Task<List<GlueType>> GetAll();
        Task<List<GlueType>> GetAllByParentID(int parentID);
        Task<List<GlueType>> GetAllByLevel(int parentID);
        Task<bool> CreateParent(GlueType model);
        Task<bool> Update(GlueType model);
        Task<bool> CreateChild(GlueType model);
        Task<bool> Delete(int id);
    }
}
