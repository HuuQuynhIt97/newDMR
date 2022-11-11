using DMR_API.Data;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Repositories.Interface
{
   public interface IArticleNoRepository: IECRepository<ArticleNo>
    {
        Task<object> GetArticleNoByModelNameID(int modelNameID);
    }
}
