using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DMR_API.Helpers;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto.Tls;
using DMR_API.SignalrHub;
using Microsoft.AspNetCore.SignalR;

namespace DMR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BottomFactoryController : ControllerBase
    {
        private readonly IBottomFactoryService _factoryService;
        private readonly IHubContext<ECHub> _hubContext;
        public BottomFactoryController(IBottomFactoryService factoryService, IHubContext<ECHub> hubContext)
        {
            _factoryService = factoryService;
            _hubContext = hubContext;
        }


        [HttpGet("{building}")]
        public async Task<IActionResult> ToDo(int building)
        {
            var batchs = await _factoryService.ToDoList(building);
            return Ok(batchs);
        }


        [HttpGet("{building}")]
        public async Task<IActionResult> DispatchList(int building)
        {
            var batchs = await _factoryService.DispatchList(building);
            return Ok(batchs);
        }
        [HttpGet("{building}")]
        public async Task<IActionResult> DispatchListDelay(int building)
        {
            var batchs = await _factoryService.DelayList(building);
            return Ok(batchs);
        }
        [HttpGet("{building}")]
        public async Task<IActionResult> Delay(int building)
        {
            var batchs = await _factoryService.UndoneList(building);
            return Ok(batchs);
        }

        [HttpGet("{building}")]
        public async Task<IActionResult> Done(int building)
        {
            var batchs = await _factoryService.DoneList(building);
            return Ok(batchs);
        }
        [HttpGet("{building}")]
        public async Task<IActionResult> EVAUVList(int building)
        {
            var batchs = await _factoryService.EVA_UVList(building);
            return Ok(batchs);
        }

        [HttpPost]
        public async Task<IActionResult> ScanQRCode(ScanQRCodeParams scanQRCodeParams)
        {
            var batchs = await _factoryService.ScanQRCodeV110(scanQRCodeParams);
            return Ok(batchs);
        }
        [HttpPost]
        public async Task<IActionResult> Print(BottomFactoryForPrintParams obj)
        {
            var batchs = await _factoryService.Print(obj.Subpackages);
            return Ok(batchs);
        }
        [HttpPost]
        public async Task<IActionResult> SaveSubpackage(SubpackageParam obj)
        {
            var batchs = await _factoryService.SaveSubpackage(obj);
            return Ok(batchs);
        }
        [HttpPost]
        public async Task<IActionResult> AddDispatch(AddDispatchParams obj)
        {
            var batchs = await _factoryService.AddDispatch(obj);
            if (batchs.Status)
                return NoContent();
            return BadRequest(batchs.Message);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDispatch(UpdateDispatchParams obj)
        {
            var batchs = await _factoryService.UpdateDispatch(obj);
            if(batchs.Status)
                return NoContent();
            return BadRequest(batchs.Message);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateScanByNumber(GenerateSubpackageParams obj)
        {
            var batchs = await _factoryService.GenerateScanByNumber(obj);
            return Ok(batchs);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllDispatch([FromQuery] DispatchParamsDto obj)
        {
            var batchs = await _factoryService.GetAllDispatch(obj);
            return Ok(batchs);
        }

        [HttpGet("{mixingInfoID}")]
        public async Task<IActionResult> GetMixingInfo(int mixingInfoID)
        {
            var batchs = await _factoryService.GetMixingInfo(mixingInfoID);
            return Ok(batchs);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubpackageCapacity()
        {
            var batchs = await _factoryService.GetSubpackageCapacity();
            return Ok(batchs);
        }

        [HttpGet("{batch}/{glueNameID}/{buildingID}")]
        public async Task<IActionResult> GetSubpackageLatestSequence(string batch, int glueNameID, int buildingID)
        {
            var batchs = await _factoryService.GetSubpackageLatestSequence(batch,glueNameID,buildingID);
            return Ok(batchs);
        }
    }
}