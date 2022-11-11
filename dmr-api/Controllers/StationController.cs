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
    public class StationController : ControllerBase
    {
        private readonly IStationService _stationService;
        public StationController(IStationService StationService)
        {
            _stationService = StationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var station = await _stationService.GetAllAsync();
            return Ok(station);
        }
        [HttpGet("{planID}")]
        public async Task<IActionResult> GetAllByPlanID(int planID)
        {
            var station = await _stationService.GetAllByPlanID(planID);
            return Ok(station);
        }

        [HttpPost]
        public async Task<IActionResult> Create(StationDto create)
        {

            if (_stationService.GetById(create.ID) != null)
                return BadRequest(" StationID already exists!");
            create.CreateTime = DateTime.Now;
            if (await _stationService.Add(create))
            {
                return NoContent();
            }

            throw new Exception("Creating the station failed on save");
        }

        [HttpPut]
        public async Task<IActionResult> Update(StationDto update)
        {
            if (await _stationService.Update(update))
                return NoContent();
            return BadRequest($"Updating the station {update.ID} failed on save");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateRange([FromBody]List<StationDto> update)
        {
            if (await _stationService.UpdateRange(update))
                return NoContent();
            return BadRequest($"Updating the station failed on save");
        }
        [HttpPost]
        public async Task<IActionResult> AddRange([FromBody] List<StationDto> update)
        {
            if (await _stationService.AddRange(update))
                return NoContent();
            return BadRequest($"Updating the station failed on save");
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _stationService.DeleteStation(id))
                return NoContent();
            throw new Exception("Error deleting the station");
        }
    }
}
