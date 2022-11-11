using DMR_API._Services.Interface;
using DMR_API.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Services
{
    public class Response
    {
        public DateTime End { get; set; }
        public DateTime Start { get; set; }
        public double Consumption { get; set; }
        public string Message { get; set; }
    }
    public class TimeServiceParams
    {
        public DateTime End { get; set; }
        public DateTime Start { get; set; }
        public DateTime EndLunchTime { get; set; }
        public DateTime StartLunchTime { get; set; }

        public double PrepareTime { get; set; }

        public double Consumption { get; set; }
        public double HourlyOutput { get; set; }
        public double ReplacementFrequency { get; set; }
    }
    public class TimeService : ITimeService
    {
        public List<TodolistDto> GenerateTaskByTimeRange(TimeServiceParams model)
        {
            throw new NotImplementedException();
        }

        public List<Response> TimeRange(DateTime startTemp, DateTime endTemp, DateTime dueDate)
        {
            var ct = dueDate;
            var startLunchTime = new DateTime(ct.Year, ct.Month, ct.Day, 12, 30, 0);
            var finishLunchTime = new DateTime(ct.Year, ct.Month, ct.Day, 13, 30, 0);

            var start = new DateTime(ct.Year, ct.Month, ct.Day, startTemp.Hour, startTemp.Minute, 0);
            var end = new DateTime(ct.Year, ct.Month, ct.Day, endTemp.Hour, endTemp.Minute, 0);
            var finishWorkingTime = end;
            var prepareTime = TimeSpan.FromMinutes(30);
            double replacementFrequency = 2;

            var estimatedStartTimeTemp = start;
            var fwt = new DateTime();
            var kgPair = 0.01;
            var hourlyOutput = 120;
            var list = new List<Response>();
            double RF = 2;
            if (dueDate.Date != DateTime.Now.Date)
            {
                var EST = dueDate.Date.Add(new TimeSpan(7, 30, 00)) - prepareTime;
                estimatedStartTimeTemp = EST;
                while (true)
                {
                    fwt = estimatedStartTimeTemp.Add(prepareTime);
                    var todo = new Response();
                    if (estimatedStartTimeTemp >= end) break;
                    // Rot vao khoang TG an trua tinh lai consumption 
                    if (
                        estimatedStartTimeTemp >= startLunchTime // 11:30 >= 12:30
                        && estimatedStartTimeTemp <= finishLunchTime // 13:30 <= 13:30 
                        )
                    {
                        estimatedStartTimeTemp = finishLunchTime;
                        todo.Start = estimatedStartTimeTemp;  // SLT 13:30
                        todo.End = estimatedStartTimeTemp.Add(prepareTime); // 13:30 + preparetime
                        // 13:30 + 2 = 15:30 >= 14:00
                        var estimatedStartNextTimeTemp = estimatedStartTimeTemp.AddHours(replacementFrequency);
                        if (estimatedStartNextTimeTemp > finishWorkingTime)
                        {
                            replacementFrequency = (end - estimatedStartTimeTemp).TotalHours;
                            todo.Message += $"Vượt quá endTime: {replacementFrequency}";
                        }

                        var standardConsumption = kgPair * (double)hourlyOutput * replacementFrequency;
                        todo.Consumption = standardConsumption;
                    }
                    else
                    {
                        var estimatedStartNextTimeTemp = estimatedStartTimeTemp.AddHours(replacementFrequency);

                        if (estimatedStartNextTimeTemp > finishWorkingTime)
                        {
                            replacementFrequency = (finishWorkingTime - estimatedStartTimeTemp).TotalHours;
                            todo.Message += $"TH TGBD tiep theo lon hon TGKT. Tinh lai RF {replacementFrequency} gio";
                        }
                        else if (
                           estimatedStartTimeTemp < startLunchTime
                        && estimatedStartNextTimeTemp >= finishLunchTime
                        || estimatedStartTimeTemp < startLunchTime
                            && estimatedStartNextTimeTemp <= finishLunchTime
                            && estimatedStartNextTimeTemp > startLunchTime
                            )
                        {
                            // neu TGBG tiep theo ma nam trong gio an trua thi gan bang TGKT an trua
                            estimatedStartNextTimeTemp = estimatedStartNextTimeTemp <= finishLunchTime && estimatedStartNextTimeTemp >= startLunchTime ? finishLunchTime : estimatedStartNextTimeTemp;
                            var recalculateReplacementFrequency = (estimatedStartNextTimeTemp - estimatedStartTimeTemp).TotalHours - (finishLunchTime - startLunchTime).TotalHours;
                            replacementFrequency = recalculateReplacementFrequency;
                            todo.Message += $"Giao voi TG an trua .Tinh lai RF = {replacementFrequency} gio.";
                        }

                        todo.Start = estimatedStartTimeTemp;
                        var standardConsumption = kgPair * (double)hourlyOutput * replacementFrequency;
                        todo.Consumption = standardConsumption;
                        todo.End = fwt;
                    }
                    replacementFrequency = RF;
                    estimatedStartTimeTemp = estimatedStartTimeTemp.AddHours(replacementFrequency);
                    list.Add(todo);
                }
            }
            else
            {
                while (true)
                {
                    fwt = estimatedStartTimeTemp.Add(prepareTime);
                    var todo = new Response();
                    if (estimatedStartTimeTemp >= end) break;
                    // chỉ can rot vao khoang TG an trua thi gan TGBD = TGKT an trua
                    // 11:00
                    if (estimatedStartTimeTemp >= startLunchTime && estimatedStartTimeTemp <= finishLunchTime)
                    {
                        estimatedStartTimeTemp = finishLunchTime;
                        todo.Start = estimatedStartTimeTemp;  // SLT 13:30
                        todo.End = estimatedStartTimeTemp.Add(prepareTime); // 13:30 + preparetime
                        // 13:30 + 2 = 15:30 >= 14:00
                        var estimatedStartNextTimeTemp = estimatedStartTimeTemp.AddHours(replacementFrequency);
                        if (estimatedStartNextTimeTemp > finishWorkingTime)
                        {
                            replacementFrequency = (end - estimatedStartTimeTemp).TotalHours;
                            todo.Message += $"Vượt quá endTime: {replacementFrequency}";
                        }

                        var standardConsumption = kgPair * (double)hourlyOutput * replacementFrequency;
                        todo.Consumption = standardConsumption;

                    }
                    else
                    {
                        var estimatedStartNextTimeTemp = estimatedStartTimeTemp.AddHours(replacementFrequency);

                        if (estimatedStartNextTimeTemp > finishWorkingTime)
                        {
                            replacementFrequency = (finishWorkingTime - estimatedStartTimeTemp).TotalHours;
                            todo.Message += $"TH TGBD tiep theo lon hon TGKT. Tinh lai RF {replacementFrequency} gio";
                        } //11:00 < 12:30 && 13:00 < 16:30 && 13:00: 13:30 &&  
                        else if (estimatedStartTimeTemp < startLunchTime
                        && estimatedStartNextTimeTemp >= finishLunchTime
                        || estimatedStartTimeTemp < startLunchTime
                            && estimatedStartNextTimeTemp <= finishLunchTime
                            && estimatedStartNextTimeTemp > startLunchTime
                            ) // 15:00 > 12:30
                        {
                            // 11:00 - 15:00 ... 12:30-13:30
                            // 11:00-13:00 ... 12:30-13:30


                            // neu TGBG tiep theo ma nam trong gio an trua thi gan bang TGKT an trua
                            estimatedStartNextTimeTemp = estimatedStartNextTimeTemp <= finishLunchTime && estimatedStartNextTimeTemp >= startLunchTime ? finishLunchTime : estimatedStartNextTimeTemp;
                            var recalculateReplacementFrequency = (estimatedStartNextTimeTemp - estimatedStartTimeTemp).TotalHours - (finishLunchTime - startLunchTime).TotalHours;
                            replacementFrequency = recalculateReplacementFrequency;
                            todo.Message += $"Giao voi TG an trua .Tinh lai RF = {replacementFrequency} gio.";
                        }

                        todo.Start = estimatedStartTimeTemp;
                        var standardConsumption = kgPair * (double)hourlyOutput * replacementFrequency;
                        todo.Consumption = standardConsumption;
                        todo.End = fwt;
                    }
                    replacementFrequency = RF;
                    estimatedStartTimeTemp = estimatedStartTimeTemp.AddHours(replacementFrequency);
                    list.Add(todo);
                }
            }


            return list;
        }
    }
}
