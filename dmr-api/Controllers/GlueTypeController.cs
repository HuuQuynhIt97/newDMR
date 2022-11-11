using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DMR_API.Helpers;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dmr_api.Models;

namespace DMR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class GlueTypeController : ControllerBase
    {
        private readonly IGlueTypeService _glueTypeService;

        public GlueTypeController(IGlueTypeService glueTypeService)
        {
            _glueTypeService = glueTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsTreeView()
        {
            var model = await _glueTypeService.GetAllAsTreeView();
            return Ok(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var model = await _glueTypeService.GetAll();
            return Ok(model);
        }

        [HttpGet("{parentID}")]
        public async Task<IActionResult> GetAllByParentID(int parentID)
        {
            var model = await _glueTypeService.GetAllByParentID(parentID);
            return Ok(model);
        }
        [HttpGet("{level}")]
        public async Task<IActionResult> GetAllByLevel(int level)
        {
            var model = await _glueTypeService.GetAllByLevel(level);
            return Ok(model);
        }
       

        [HttpPost]
        public async Task<IActionResult> CreateParent(GlueType model)
        {
            if (await _glueTypeService.CreateParent(model))
            {
                return NoContent();
            }

            throw new Exception("Creating the glue type failed on save");
        }

        [HttpPost]
        public async Task<IActionResult> CreateChild(GlueType model)
        {
            if (await _glueTypeService.CreateChild(model))
            {
                return NoContent();
            }

            throw new Exception("Creating the glue type failed on save");
        }

        [HttpPut]
        public async Task<IActionResult> Update(GlueType model)
        {

            if (await _glueTypeService.Update(model))
            {
                return NoContent();
            }

            throw new Exception("Updating the glue type failed on save");
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _glueTypeService.Delete(id))
                return NoContent();
            throw new Exception("Error deleting the glue type");
        }
    }
}