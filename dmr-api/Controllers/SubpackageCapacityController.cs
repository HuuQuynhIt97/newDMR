using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DMR_API.Helpers;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DMR_API.Models;

namespace DMR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SubpackageCapacityController : ControllerBase
    {
        private readonly ISubpackageCapacityService _SubpackageCapacityService;
        public SubpackageCapacityController(ISubpackageCapacityService SubpackageCapacityService)
        {
            _SubpackageCapacityService = SubpackageCapacityService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lines = await _SubpackageCapacityService.GetAllAsync();
            return Ok(lines);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SubpackageCapacity create)
        {

            if (_SubpackageCapacityService.GetById(create.ID) != null)
                return BadRequest("SubpackageCapacity ID already exists!");
            //create.CreatedDate = DateTime.Now;
            if (await _SubpackageCapacityService.Add(create))
            {
                return NoContent();
            }

            throw new Exception("Creating the SubpackageCapacity failed on save");
        }

        [HttpPut]
        public async Task<IActionResult> Update(SubpackageCapacity update)
        {
            if (await _SubpackageCapacityService.Update(update))
                return NoContent();
            return BadRequest($"Updating SubpackageCapacity {update.ID} failed on save");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _SubpackageCapacityService.Delete(id))
                return NoContent();
            throw new Exception("Error deleting the SubpackageCapacity");
        }
    }
}