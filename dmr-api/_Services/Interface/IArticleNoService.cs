using DMR_API.DTO;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
   public interface IArticleNoService : IECService<ArticleNoDto>
    {
        Task<List<ArticleNoDto>> GetArticleNoByModelNameID(int modelNameID);
        Task<List<ArticleNoDto>> GetArticleNoByModelNoID(int modelNoID);
    }
}
