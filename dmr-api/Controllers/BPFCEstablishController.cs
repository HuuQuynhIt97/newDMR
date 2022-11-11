using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Helpers.Enum;
using DMR_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DMR_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BPFCEstablishController : ControllerBase
    {
        private readonly IBPFCEstablishService _bPFCEstablishService;

        public BPFCEstablishController(IBPFCEstablishService bPFCEstablishService)
        {
            _bPFCEstablishService = bPFCEstablishService;
        }
        // done
        [HttpGet]
        public async Task<IActionResult> GetDoneBPFC()
        {
            return Ok(await _bPFCEstablishService.GetDoneBPFC());
        }
        // undone
        [HttpGet]
        public async Task<IActionResult> GetUndoneBPFC()
        {
            return Ok(await _bPFCEstablishService.GetUndoneBPFC());
        }

        // GET: api/<BPFCEstablishController>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _bPFCEstablishService.GetAllAsync());
        }

        [HttpGet("{bpfcID}")]
        public async Task<IActionResult> GetDetailBPFC(int bpfcID)
        {
            return Ok(await _bPFCEstablishService.GetDetailBPFC(bpfcID));
        }


        [HttpGet("{bpfcID}")]
        public async Task<IActionResult> GetGlueByBPFCID(int bpfcID)
        {
            return Ok(await _bPFCEstablishService.GetGlueByBPFCID(bpfcID));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllHistory()
        {
            return Ok(await _bPFCEstablishService.GetAllHistoryAsync());
        }

        [HttpPost]
        public async Task<ActionResult<BPFCHistoryDto>> AddBPFCHistory(BPFCHistoryDto entity)
        {
            return Ok(await _bPFCEstablishService.Create(entity));
        }
        [HttpPut]
        public async Task<ActionResult<BPFCHistory>> UpdateBPFCHistory(BPFCHistory entity)
        {
            return Ok(await _bPFCEstablishService.UpdateBPFCHistory(entity));
        }
        [HttpGet("{bpfcID}")]
        public async Task<ActionResult<BPFCHistoryDto>> LoadBPFCHistory(int bpfcID)
        {
            return Ok(await _bPFCEstablishService.LoadBPFCHistory(bpfcID));
        }

        [HttpPost]
        public async Task<IActionResult> GetBPFCID(GetBPFCIDDto bpfcInfo)
        {
            return Ok(await _bPFCEstablishService.GetBPFCID(bpfcInfo));
        }
        [HttpGet]
        public async Task<IActionResult> GetAllBPFCStatus()
        {
            return Ok(await _bPFCEstablishService.GetAllBPFCStatus());
        }
        [HttpGet("{status}/{start}/{end}")]
        [HttpGet("{status}")]
        [HttpGet("{start}/{end}")]
        public async Task<IActionResult> GetAllBPFCRecord(Status status = Status.Unknown, string start = "", string end = "")
        {
            return Ok(await _bPFCEstablishService.GetAllBPFCRecord(status, start, end));
        }
        [HttpPost]
        public async Task<ActionResult> Import([FromForm] IFormFile file2)
        {
            IFormFile file = Request.Form.Files["UploadedFile"];
            object createdBy = Request.Form["CreatedBy"];
            var datasList = new List<BPFCEstablishDtoForImportExcel>();
            //var datasList2 = new List<UploadDataVM2>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            if ((file != null) && (file.Length > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                string fileName = file.FileName;
                int userid = createdBy.ToInt();
                using (var package = new ExcelPackage(file.OpenReadStream()))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;

                    for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                    {
                        var modelName = workSheet.Cells[rowIterator, 1].Value.ToSafetyString();
                        var modelNo = workSheet.Cells[rowIterator, 2].Value.ToSafetyString();
                        var articleNo = workSheet.Cells[rowIterator, 3].Value.ToSafetyString();
                        var process = workSheet.Cells[rowIterator, 4].Value.ToSafetyString();
                        DateTime? dueDate = workSheet.Cells[rowIterator, 5].Value as DateTime?;
                        if (!modelName.IsNullOrEmpty() && !modelNo.IsNullOrEmpty() && !articleNo.IsNullOrEmpty() && !process.IsNullOrEmpty())
                        {
                            datasList.Add(new BPFCEstablishDtoForImportExcel()
                            {
                                ModelName = modelName,
                                ModelNo = modelNo,
                                ArticleNo = articleNo,
                                Process = process,
                                CreatedDate = DateTime.Now,
                                CreatedBy = userid,
                                DueDate = dueDate
                            });
                        }
                    }
                }
                await _bPFCEstablishService.ImportExcel(datasList);
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }

        }
        [HttpGet("{bpfcID}/{userid}")]
        public async Task<IActionResult> Done(int bpfcID, int userid)
        {
            return Ok(await _bPFCEstablishService.Done(bpfcID, userid));
        }
        [HttpGet("{bpfcID}/{userid}")]
        public async Task<IActionResult> Approval(int bpfcID, int userid)
        {
            return Ok(await _bPFCEstablishService.Approval(bpfcID, userid));

        }
        [HttpGet("{bpfcID}/{userid}")]
        public async Task<IActionResult> Release(int bpfcID, int userid)
        {
            return Ok(await _bPFCEstablishService.Release(bpfcID, userid));
        }
        [HttpGet("{bpfcID}/{userid}")]
        public async Task<IActionResult> Reject(int bpfcID, int userid)
        {
            return Ok(await _bPFCEstablishService.Reject(bpfcID, userid));

        }
        [HttpGet]
        public async Task<IActionResult> FilterByApprovedStatus()
        {
            var bpfc = await _bPFCEstablishService.FilterByApprovedStatus();
            return Ok(bpfc);
        }
        [HttpGet]
        public async Task<IActionResult> FilterByFinishedStatus()
        {
            var bpfc = await _bPFCEstablishService.FilterByFinishedStatus();
            return Ok(bpfc);
        }
        [HttpGet]
        public async Task<IActionResult> DefaultFilter()
        {
            var bpfc = await _bPFCEstablishService.DefaultFilter();
            return Ok(bpfc);
        }
        [HttpGet]
        public async Task<IActionResult> RejectedFilter()
        {
            var bpfc = await _bPFCEstablishService.RejectedFilter();
            return Ok(bpfc);
        }
        [HttpGet]
        public async Task<IActionResult> FilterByNotApprovedStatus()
        {
            var bpfc = await _bPFCEstablishService.FilterByNotApprovedStatus();
            return Ok(bpfc);
        }
        [HttpGet("{email}")]
        public async Task<IActionResult> SendMailForPIC(string email)
        {
            await _bPFCEstablishService.SendMailForPIC(email);
            return NoContent();
        }
        [HttpGet("{buildingID}")]
        public async Task<IActionResult> GetAllBPFCByBuildingID(int buildingID)
        {
            return Ok(await _bPFCEstablishService.GetAllBPFCByBuildingID(buildingID));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSeason(BPFCEstablishUpdateSeason entity)
        {

            if (await _bPFCEstablishService.UpdateSeason(entity))
                return NoContent();

            return BadRequest($"Updating BPFC {entity.ID} failed on save");
        }
        [HttpPut]
        public async Task<IActionResult> UpdateDueDate(BPFCEstablishUpdateDueDate entity)
        {

            if (await _bPFCEstablishService.UpdateDueDate(entity))
                return NoContent();

            return BadRequest($"Updating BPFC {entity.ID} failed on save");
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {

            if (await _bPFCEstablishService.Delete(id))
                return NoContent();

            return BadRequest($"Delete BPFC {id} failed on save");
        }
    }
}
