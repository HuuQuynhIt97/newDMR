using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMR_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;

        public UserRoleController(IUserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }
        [HttpPost]
        public async Task<IActionResult> MappingUserRole([FromBody] UserRoleDto userRoleDto)
        {
            var result = await _userRoleService.MappingUserRole(userRoleDto);
            return Ok(result);
        }
        [HttpPut]
        public async Task<IActionResult> Lock([FromBody] UserRoleDto userRoleDto)
        {
            var result = await _userRoleService.Lock(userRoleDto);
            return Ok(result);
        }
        [HttpPut]
        public async Task<IActionResult> IsLock([FromBody] UserRoleDto userRoleDto)
        {
            var result = await _userRoleService.IsLock(userRoleDto);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> RemoveUserRole([FromBody] UserRoleDto userRoleDto)
        {
            var result = await _userRoleService.RemoveUserRole(userRoleDto);
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUserRoles()
        {
            var result = await _userRoleService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{userID}")]
        public async Task<IActionResult> GetRoleByUserID(int userID)
        {
            var result = await _userRoleService.GetRoleByUserID(userID);
            return Ok(result);
        }
        [HttpPut("{userID}/{roleID}")]
        public async Task<IActionResult> MapUserRole(int userID, int roleID)
        {
            var result = await _userRoleService.MapUserRole(userID, roleID);
            return Ok(result);
        }
    }
}
