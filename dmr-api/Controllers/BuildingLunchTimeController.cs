using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMR_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BuildingLunchTimeController : ControllerBase
    {
        private readonly IBuildingLunchTimeService _buildingLunchTimeService;
        private readonly IBuildingService _buildingService;

        public BuildingLunchTimeController(IBuildingLunchTimeService buildingLunchTimeService, IBuildingService buildingService)
        {
            _buildingLunchTimeService = buildingLunchTimeService;
            _buildingService = buildingService;
        }
        // getall building
        [HttpGet]
        public async Task<IActionResult> GetAllBuildings()
        {
            var result = await _buildingService.GetBuildings();
            return Ok(result);
        }
        [HttpGet("{buildingID}")]
        public async Task<IActionResult> GetPeriodMixingByBuildingID(int buildingID)
        {
            var result = await _buildingLunchTimeService.GetPeriodMixingByBuildingID(buildingID);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateLunchTime(LunchTimeDto create)
        {
            var status = await _buildingService.AddOrUpdateLunchTime(create);
            if (status) return NoContent();
            else
                throw new Exception("Creating or updating the lunchTime failed on save");
        }
        [HttpPut]
        public async Task<IActionResult> UpdatePeriodMixing(PeriodMixing update)
        {
            var res = await _buildingLunchTimeService.UpdatePeriodMixing(update);
            if (res.Status) return NoContent();
            return BadRequest(res.Message);
        }
        [HttpPost]
        public async Task<IActionResult> AddPeriodMixing(PeriodMixing update)
        {
            var res = await _buildingLunchTimeService.AddPeriodMixing(update);
            if (res.Status) return NoContent();
            return BadRequest(res.Message);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePeriodMixing(int id)
        {
         
            var res = await _buildingLunchTimeService.DeletePeriodMixing(id);
            
            if (res.Status) return NoContent();
            return BadRequest(res.Message);
        }


        [HttpGet("{periodMixingID}")]
        public async Task<IActionResult> GetPeriodDispatchByPeriodMixingID(int periodMixingID)
        {
            var result = await _buildingLunchTimeService.GetPeriodDispatchByPeriodMixingID(periodMixingID);
            return Ok(result);
        }
        [HttpPut]
        public async Task<IActionResult> UpdatePeriodDispatch(PeriodDispatch update)
        {
            var res = await _buildingLunchTimeService.UpdatePeriodDispatch(update);
            if (res.Status) return NoContent();
            return BadRequest(res.Message);
        }
        [HttpPost]
        public async Task<IActionResult> AddPeriodDispatch(PeriodDispatch update)
        {
            var res = await _buildingLunchTimeService.AddPeriodDispatch(update);
            if (res.Status) return NoContent();
            return BadRequest(res.Message);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePeriodDispatch(int id)
        {

            var res = await _buildingLunchTimeService.DeletePeriodDispatch(id);

            if (res.Status) return NoContent();
            return BadRequest(res.Message);
        }

        [HttpPut]
        public async Task<IActionResult> AddLunchTimeBuilding(Building building)
        {
            var res = await _buildingLunchTimeService.AddLunchTimeBuilding(building);
            if (res.Status) return NoContent();
            return BadRequest(res.Message);
        }
    }
}
