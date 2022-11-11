using DMR_API.Helpers;
using DMR_API.Helpers.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Helpers
{
    public interface IJWTService
    {
        public int GetUserID();
        public string GetUserName();
    }
    public class JWTService : IJWTService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JWTService(
            IHttpContextAccessor httpContextAccessor
            )
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetUserID()
        {
            var token = _httpContextAccessor.HttpContext
                        .Request.Headers["Authorization"].FirstOrDefault();
            if (token is null) return 0;
            var user = token.DecodeToken().First(claim => claim.Type == Claims.nameid + "");
            if (user is null) return 0;
            return user.Value.ToInt();
        }

        public string GetUserName()
        {
            var token = _httpContextAccessor.HttpContext
                         .Request.Headers["Authorization"].FirstOrDefault();
            if (token is null) return null;
            var user = token.DecodeToken().First(claim => claim.Type == Claims.unique_name + "");
            if (user is null) return null;
            return user.Value;
        }
    }
}
