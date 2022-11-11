using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Version = DMR_API.Models.Version;

namespace DMR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class VersionController : ControllerBase
    {
        private readonly IVersionService _versionService;
        public VersionController(IVersionService versionService)
        {
            _versionService = versionService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _versionService.GetAllAsync();
            return Ok(data.FirstOrDefault(x => x.ID == id));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var plans = await _versionService.GetAllAsync();
            return Ok(plans);
        }
        [HttpGet("{text}")]
        public async Task<IActionResult> Search([FromQuery] PaginationParams param, string text)
        {
            var lists = await _versionService.Search(param, text);
            Response.AddPagination(lists.CurrentPage, lists.PageSize, lists.TotalCount, lists.TotalPages);
            return Ok(lists);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Version create)
        {

            if (_versionService.GetById(create.ID) != null)
                return BadRequest("Version already exists!");
            create.CreatedTime = DateTime.Now;
            if (await _versionService.Add(create))
            {
                return NoContent();
            }

            throw new Exception("Creating the Version failed on save");
        }
        [HttpPut]
        public async Task<IActionResult> Update(Version update)
        {
            update.UpatedTime = DateTime.Now;
            if (await _versionService.Update(update))
                return NoContent();
            return BadRequest($"Updating the Version {update.ID} failed on save");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _versionService.Delete(id))
                return NoContent();
            throw new Exception("Error deleting the Version");
        }
    }
}
