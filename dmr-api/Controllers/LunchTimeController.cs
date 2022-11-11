using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DMR_API.Helpers;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LunchTimeController : ControllerBase
    {
        private readonly ILunchTimeService _lunchTimeService;
        public LunchTimeController(ILunchTimeService lunchTimeService)
        {
            _lunchTimeService = lunchTimeService;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetLines([FromQuery]PaginationParams param)
        //{
        //    var lines = await _lineService.GetWithPaginations(param);
        //    Response.AddPagination(lines.CurrentPage,lines.PageSize,lines.TotalCount,lines.TotalPages);
        //    return Ok(lines);
        //}

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lines = await _lunchTimeService.GetAllAsync();
            return Ok(lines);
        }

        //[HttpGet("{text}")]
        //public async Task<IActionResult> Search([FromQuery]PaginationParams param, string text)
        //{
        //    var lists = await _lineService.Search(param, text);
        //    Response.AddPagination(lists.CurrentPage, lists.PageSize, lists.TotalCount, lists.TotalPages);
        //    return Ok(lists);
        //}
       
        [HttpPost]
        public async Task<IActionResult> Create(LunchTimeDto create)
        {

            if (_lunchTimeService.GetById(create.ID) != null)
                return BadRequest("LunchTime ID already exists!");
            //create.CreatedDate = DateTime.Now;
            if (await _lunchTimeService.Add(create))
            {
                return NoContent();
            }

            throw new Exception("Creating the lunchTime failed on save");
        }

        [HttpPut]
        public async Task<IActionResult> Update(LunchTimeDto update)
        {
            if (await _lunchTimeService.Update(update))
                return NoContent();
            return BadRequest($"Updating LunchTime {update.ID} failed on save");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _lunchTimeService.Delete(id))
                return NoContent();
            throw new Exception("Error deleting the LunchTime");
        }
    }
}