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
    public class ShakeController : ControllerBase
    {
        private readonly IShakeService _shakeService;
        public ShakeController(IShakeService shakeService)
        {
            _shakeService = shakeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lines = await _shakeService.GetAllAsync();
            return Ok(lines);
        }
        [HttpGet("{mixingInfoID}")]
        public async Task<IActionResult> GetShakesByMixingInfoID(int mixingInfoID)
        {
            var lines = await _shakeService.GetShakesByMixingInfoID(mixingInfoID);
            return Ok(lines);
        }
        [HttpGet("{mixingInfoID}")]
        public async Task<IActionResult> GenerateShakes(int mixingInfoID)
        {
            var lines = await _shakeService.GenerateShakes(mixingInfoID);
            return Ok(lines);
        }
        [HttpPost]
        public async Task<IActionResult> Create(Shake create)
        {

            if (_shakeService.GetById(create.ID) != null)
                return BadRequest("Shake ID already exists!");
            //create.CreatedDate = DateTime.Now;
            if (await _shakeService.Add(create))
            {
                return NoContent();
            }

            throw new Exception("Creating the Shake failed on save");
        }

        [HttpPut]
        public async Task<IActionResult> Update(Shake update)
        {
            if (await _shakeService.Update(update))
                return NoContent();
            return BadRequest($"Updating Shake {update.ID} failed on save");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _shakeService.Delete(id))
                return NoContent();
            throw new Exception("Error deleting the Shake");
        }
    }
}