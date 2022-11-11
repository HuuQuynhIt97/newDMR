using System.Threading.Tasks;
using DMR_API._Repositories.Interface;
using DMR_API.Data;
using DMR_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using DMR_API.DTO;
using System.Collections.Generic;
using AutoMapper;

namespace DMR_API._Repositories.Repositories
{
    public class ArticleNoRepository : ECRepository<ArticleNo>, IArticleNoRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public ArticleNoRepository(DataContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public Task<object> GetArticleNoByModelNameID(int modelNameID)
        {
            throw new System.NotImplementedException();
        }
    }
}