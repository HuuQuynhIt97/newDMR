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
    public class ToDoListController : ControllerBase
    {
        private readonly IToDoListService _toDoList;
        private readonly IBottomFactoryService _bottomFactoryService;
        private readonly IMailExtension _mailingService;
        private readonly IHubContext<ECHub> _hubContext;
        public ToDoListController(IToDoListService toDoList,
            IBottomFactoryService bottomFactoryService,
            IMailExtension mailingService,
            IHubContext<ECHub> hubContext)
        {
            _toDoList = toDoList;
            _bottomFactoryService = bottomFactoryService;
            _mailingService = mailingService;
            _hubContext = hubContext;
        }

        // Thêm bởi Quỳnh (Leo 2/2/2021 11:46)
        [HttpGet("{MixingInfoID}")]
        public async Task<IActionResult> MixedHistory(int MixingInfoID)
        {
            var status = await _toDoList.MixedHistory(MixingInfoID);
            return Ok(status);

        }
        // Thêm bởi Quỳnh (Leo 2/2/2021 11:46)
        [HttpGet("{glueNameID}/{glueID}/{start}/{end}")]
        public async Task<IActionResult> Addition(int glueNameID, int glueID, DateTime start, DateTime end)
        {
            var status = await _toDoList.Addition(glueNameID, glueID, start, end);
            return Ok(status);

        }

        [HttpGet("{glueNameID}")]
        public async Task<IActionResult> AdditionDispatch(int glueNameID)
        {
            var res = await _toDoList.AdditionDispatch(glueNameID);
            if (res.Status) return NoContent();
            return BadRequest(res.Message);
        }

        [HttpPut("{building}")]
        public IActionResult UpdateFinishStirTimeByMixingInfoID(int building)
        {
            if (_toDoList.UpdateFinishStirTimeByMixingInfoID(building))
            {
                return NoContent();
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpPut("{building}")]
        public IActionResult UpdateStartStirTimeByMixingInfoID(int building)
        {
            if (_toDoList.UpdateStartStirTimeByMixingInfoID(building))
            {
                return NoContent();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("{building}")]
        public async Task<IActionResult> ToDo(int building)
        {
            var batchs = await _toDoList.ToDo(building);
            return Ok(batchs);
        }

        [HttpGet("{building}")]
        public async Task<IActionResult> ToDoAddition(int building)
        {
            var batchs = await _toDoList.ToDoAddition(building);
            return Ok(batchs);
        }
        [HttpGet("{building}")]
        public async Task<IActionResult> DispatchAddition(int building)
        {
            var batchs = await _toDoList.DispatchAddition(building);
            return Ok(batchs);
        }
        [HttpGet("{building}")]
        public async Task<IActionResult> DispatchList(int building)
        {
            var batchs = await _toDoList.DispatchList(building);
            return Ok(batchs);
        }
        [HttpGet("{building}")]
        public async Task<IActionResult> DispatchListDelay(int building)
        {
            var batchs = await _toDoList.DispatchListDelay(building);
            return Ok(batchs);
        }
        [HttpGet("{building}")]
        public async Task<IActionResult> Delay(int building)
        {
            var batchs = await _toDoList.Delay(building);
            return Ok(batchs);
        }
        [HttpGet("{building}")]
        public async Task<IActionResult> Done(int building)
        {
            var batchs = await _toDoList.Done(building);
            return Ok(batchs);
        }
        [HttpPost]
        public async Task<IActionResult> Dispatch(DispatchParams todolistDto)
        {
            var batchs = await _toDoList.Dispatch(todolistDto);
            return Ok(batchs);
        }
        [HttpGet("{mixingInfoID}")]
        public IActionResult PrintGlue(int mixingInfoID)
        {
            var batchs = _toDoList.PrintGlue(mixingInfoID);
            return Ok(batchs);
        }

        [HttpGet("{mixingInfoID}")]
        public IActionResult FindPrintGlue(int mixingInfoID)
        {
            var batchs = _toDoList.FindPrintGlue(mixingInfoID);
            return Ok(batchs);
        }
        [HttpPost]
        public IActionResult Cancel([FromBody] ToDoListForCancelDto todolistID)
        {
            var batchs = _toDoList.Cancel(todolistID);
            return Ok(batchs);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateToDoList(List<int> plans)
        {
            if (await _toDoList.CheckBuildingType(plans))
            {
                var status = await _bottomFactoryService.GenerateToDoList(plans);
                return Ok(status);
            }
            else
            {
                var status = await _toDoList.GenerateToDoList(plans);
                return Ok(status);
            }
        }
        [HttpPost]
        public async Task<IActionResult> GenerateDispatchList(List<int> plans)
        {
            var status = await _toDoList.GenerateDispatchList(plans);
            return Ok(status);

        }

        [HttpGet("{buildingID}")]
        public async Task<IActionResult> ExportExcel(int buildingID)
        {
            var bin = await _toDoList.ExportExcelToDoListByBuilding(buildingID);
            return File(bin, "application/octet-stream", "doneListReport.xlsx");
        }

        [HttpGet("{buildingID}")]
        public async Task<IActionResult> GetNewReport(int buildingID)
        {
            var bin = await _toDoList.ExportExcelNewReportOfDonelistByBuilding(buildingID);
            return File(bin, "application/octet-stream", "doneListReport.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBuildingReport([FromQuery]List<string> emails)
        {
            var bin = await _toDoList.ExportExcelToDoListWholeBuilding();
            var subject = "Mixing Room Report";
            var fileName = $"{DateTime.Now.ToString("MMddyyyy")}_AchievementRateReport.xlsx";
            var message = "Please refer to the Mixing Room Report";
            if (bin.Length > 0)
            {
                await _mailingService.SendEmailWithAttactExcelFileAsync(emails, subject, message, fileName, bin);
            }

            return File(bin, "application/octet-stream", "doneListReport.xlsx");
        }
        //[HttpGet]
        //public async Task<IActionResult> GetAllBuildingReport()
        //{
        //    var bin = await _toDoList.ExportExcelToDoListWholeBuilding();
        //    return File(bin, "application/octet-stream", "doneListReport.xlsx");
        //}
        [HttpGet("{start}/{end}")]
        public async Task<IActionResult> GetAllBuildingReportByRange(DateTime start, DateTime end)
        {

            var bin = await _toDoList.ExportExcelToDoListWholeBuilding(start, end);
            var sendMail = await _toDoList.SendMail(bin, start);
            return Ok(sendMail);
        }
        [HttpPost]
        public IActionResult GetMixingDetail(MixingDetailParams obj)
        {
            var bin = _toDoList.GetMixingDetail(obj);
            return Ok(bin);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateDispatchDetail(DispatchDetailForUpdateDto dispatchDto)
        {
            var res = await _toDoList.UpdateDispatchDetail(dispatchDto);
            if (res.Status)
                return Ok(res.Message);
            return BadRequest(res.Message);
        }

        [HttpPost]
        public IActionResult CancelRange(List<ToDoListForCancelDto> todolistIDList)
        {
            var batchs = _toDoList.CancelRange(todolistIDList);
            return Ok(batchs);
        }
        [HttpGet("{mixingInfoID}/{glueNameID}/{estimatedStartTime}/{estimatedFinishTime}")]
        public async Task<IActionResult> PrintGlueDispatchList(int mixingInfoID, int glueNameID, DateTime estimatedStartTime, DateTime estimatedFinishTime)
        {
            return Ok(await _toDoList.PrintGlueDispatchListAsync(mixingInfoID, glueNameID, estimatedStartTime, estimatedFinishTime));
        }

        [HttpGet("{buildingID}/{glueNameID}/{estimatedStartTime}/{estimatedFinishTime}")]
        public async Task<IActionResult> GetDispatchDetail(int buildingID, int glueNameID, DateTime estimatedStartTime, DateTime estimatedFinishTime)
        {
            var data = await _toDoList.GetDispatchDetail(buildingID, glueNameID, estimatedStartTime, estimatedFinishTime);
            return Ok(data);
        }
        [HttpPut]
        public async Task<IActionResult> FinishDispatch(FinishDispatchParams obj)
        {
            var data = await _toDoList.FinishDispatch(obj);
            if (data.Status)
                return Ok(data.Message);
            return BadRequest(data.Message);
        }
        [HttpGet("{glueNameID}/{estimatedStartTime}/{estimatedFinishTime}")]
        public async Task<IActionResult> GetDispatchListDetail(int glueNameID, string estimatedStartTime, string estimatedFinishTime)
        {
            var data = await _toDoList.GetDispatchListDetail(glueNameID, estimatedStartTime, estimatedFinishTime);
            return Ok(data);
        }

        [HttpGet("{mixingInfoID}/{glueNameID}/{estimatedStartTime}/{estimatedFinishTime}")]
        public async Task<IActionResult> UpdateMixingInfoDispatchList(int mixingInfoID, int glueNameID, DateTime estimatedStartTime, DateTime estimatedFinishTime)
        {
            var data = await _toDoList.UpdateMixingInfoDispatchList(mixingInfoID, glueNameID, estimatedStartTime, estimatedFinishTime);

            if (data.Status) return NoContent();
            return BadRequest(data.Message);
        }

        //[HttpPut]
        //public async Task<IActionResult> UpdateDispatchDetail(DispatchListForUpdateDto obj)
        //{
        //    var data = await _toDoList.UpdateDispatchDetail(obj);
        //    if (data.Status)
        //        return NoContent();
        //    return BadRequest(data.Message);
        //}
        [HttpGet("{buildingID}/{glueNameID}/{estimatedStartTime}/{estimatedFinishTime}")]
        public async Task<IActionResult> GetMixingInfoHistory(int buildingID, int glueNameID, string estimatedStartTime, string estimatedFinishTime)
        {
            var data = await _toDoList.GetMixingInfoHistory(buildingID, glueNameID, estimatedStartTime, estimatedFinishTime);
            return Ok(data);
        }
        // them code moi 1/30/2021
        [HttpPost]
        public async Task<IActionResult> AddOvertime(List<int> plans)
        {
            var status = await _toDoList.AddOvertime(plans);
            return Ok(status);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveOvertime(List<int> plans)
        {
            var status = await _toDoList.RemoveOvertime(plans);
            return Ok(status);

        }
    }
}