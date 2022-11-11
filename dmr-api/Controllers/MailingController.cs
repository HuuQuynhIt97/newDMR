using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DMR_API.Helpers;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DMR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MailingController : ControllerBase
    {
        private readonly IMailingService _mailingService;
        public MailingController(IMailingService mailingService)
        {
            _mailingService = mailingService;
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
            var lines = await _mailingService.GetAllAsync();
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
        public async Task<IActionResult> Create(MailingDto create)
        {

            if (_mailingService.GetById(create.ID) != null)
                return BadRequest("Mailing ID already exists!");
            //create.CreatedDate = DateTime.Now;
            if (await _mailingService.Add(create))
            {
                return NoContent();
            }

            throw new Exception("Creating the mailing failed on save");
        }
        [HttpPost]
        public async Task<IActionResult> CreateRange(List<MailingDto> create)
        {
            var check = await _mailingService.CheckExists(create[0].Frequency, create[0].Report);
            if (check)
            {
                return BadRequest($"The {create[0].Frequency} and {create[0].Report} option already exists!");
            }
            if (await _mailingService.AddRange(create))
            {
                return NoContent();
            }
            throw new Exception("Creating the mailing failed on save");
        }
        [HttpPut]
        public async Task<IActionResult> Update(MailingDto update)
        {
            if (await _mailingService.Update(update))
                return NoContent();
            return BadRequest($"Updating Mailing {update.ID} failed on save");
        }
        [HttpPut]
        public async Task<IActionResult> UpdateRange(List<MailingDto> update)
        {
            if (await _mailingService.UpdateRange(update))
                return NoContent();
            return BadRequest($"Updating Mailing failed on save");
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _mailingService.Delete(id))
                return NoContent();
            throw new Exception("Error deleting the Mailing");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteRange(List<MailingDto> delete)
        {
            if (await _mailingService.DeleteRange(delete))
                return NoContent();
            throw new Exception("Error deleting the Mailing");
        }
    }
}