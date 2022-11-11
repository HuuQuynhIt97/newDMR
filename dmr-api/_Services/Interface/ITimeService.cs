using DMR_API._Services.Services;
using DMR_API.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{

    public interface ITimeService
    {
        List<Response> TimeRange(DateTime start, DateTime end, DateTime dueDate);
        List<TodolistDto> GenerateTaskByTimeRange(TimeServiceParams model);

    }

}
