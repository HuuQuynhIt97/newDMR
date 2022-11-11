using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class StirController : ControllerBase
    {
        private readonly IMixingInfoService _mixingInfoService;
        private readonly IStirService _stirService;

        public StirController(IMixingInfoService mixingInfoService,
            IStirService stirService)
        {
            _mixingInfoService = mixingInfoService;
            _stirService = stirService;
        }
        [HttpGet("{mixingInfoID}")]
        public async Task<IActionResult> GetStirByMixingInfoID(int mixingInfoID)
        {
            return Ok(await _stirService.GetStirByMixingInfoID(mixingInfoID));
        }
        [HttpGet("{glueName}")]
        public async Task<IActionResult> GetStirInfo(string glueName)
        {
            return Ok(await _mixingInfoService.Stir(glueName));
        }
        [HttpGet("{stirID}")]
        public async Task<IActionResult> GetRPM(int stirID)
        {
            return Ok(await _mixingInfoService.GetRPM(stirID));
        }
        [HttpGet("{mixingInfoID}/{building}/{start}/{end}")]
        public async Task<IActionResult> GetRPM(int mixingInfoID, string building, string start, string end)
        {
            return Ok(await _mixingInfoService.GetRPM(mixingInfoID, building, start, end));
        }
        [HttpGet("{mixingInfoID}/{start}/{end}")]
        public IActionResult GetRawData(int mixingInfoID, string start, string end)
        {
            return Ok( _mixingInfoService.GetRawData(mixingInfoID, start, end));
        }
        [HttpGet("{machineCode}/{start}/{end}")]
        public async Task<IActionResult> GetRPMByMachineCode(string machineCode, string start, string end)
        {
            return Ok(await _mixingInfoService.GetRPMByMachineCode(machineCode, start, end));
        }


        [HttpPost]
        public async Task<IActionResult> Create(StirDTO create)
        {

            if (_stirService.GetById(create.ID) != null)
                return BadRequest("Line ID already exists!");
            //create.CreatedDate = DateTime.Now;
            if (await _stirService.Add(create))
            {
                return NoContent();
            }

            throw new Exception("Creating the model name failed on save");
        }

        [HttpPut]
        public async Task<IActionResult> Update(StirDTO update)
        {
            var res = await _stirService.UpdateStir(update);
            if (res != null)
                return Ok(res);
            return BadRequest($"Updating stir {update.ID} failed on save");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _stirService.Delete(id))
                return NoContent();
            throw new Exception("Error deleting the model name");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lines = await _stirService.GetAllAsync();
            return Ok(lines);
        }
        [HttpGet("{buildingID}/{qrCode}")]
        public async Task<IActionResult> ScanMachine(int buildingID, string qrCode)
        {
            return Ok(await _stirService.ScanMachine(buildingID, qrCode));
        }
        [HttpPut("{mixingInfoID}")]
        public async Task<IActionResult> UpdateStartScanTime(int mixingInfoID)
        {
            if (await _stirService.UpdateStartScanTime(mixingInfoID))
            {
                return NoContent();
            }

            return BadRequest("Updating the stiring failed on save");
        }

    }
}
