using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RoleController(IRoleService RoleService)
        {
            _roleService = RoleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var role = await _roleService.GetAllAsync();
            return Ok(role);
        }
        [HttpPost]
        public async Task<IActionResult> Create(RoleDto create)
        {
            if (await _roleService.Add(create))
            {
                return NoContent();
            }

            throw new Exception("Creating the kind failed on save");
        }

        [HttpPut]
        public async Task<IActionResult> Update(RoleDto update)
        {
            if (await _roleService.Update(update))
                return NoContent();
            return BadRequest($"Updating Role {update.ID} failed on save");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _roleService.Delete(id))
                return NoContent();
            throw new Exception("Error deleting the Role");
        }
    }
}
