using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using TodolistScheduleService.Dto;
using TodolistScheduleService.Extension;
using TodolistScheduleService.Schedulers;
namespace TodolistScheduleService.Services
{
    public class Todo : BackgroundService
    {
        private readonly ILogger<Todo> _logger;
        SchedulerMultiple _scheduler;
        private readonly HubConnection _connection;
        private readonly Appsettings _appsettings;
        List<MailingDto> _mailingDtos = null;
        public Todo(
            ILogger<Todo> logger,
            HubConnection connection,
            Appsettings appsettings
            )
        {
            _logger = logger;
            _connection = connection;
            _appsettings = appsettings;
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _connection.DisposeAsync();
        }
      
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _scheduler = new SchedulerMultiple(_logger);
            await _scheduler.StartAllJob();
            Console.WriteLine("Start scheduler");
            _connection.Reconnecting += async (error) =>
            {
                _logger.LogInformation("Reconnecting" + DateTime.Now.ToString());

                await _scheduler.Clear();

                var job = await _scheduler.GetAllJobKeyAsync();
                var trigger = await _scheduler.GetAllTriggerKey();
            };
            _connection.Closed += async (error) =>
            {
                _logger.LogInformation("Closed" + DateTime.Now.ToString());

                await _connection.ConnectWithRetryAsync(stoppingToken);
            };
            _connection.Reconnected += async (error) =>
            {
                await _connection.InvokeAsync("Mailing");
                await _connection.InvokeAsync("AskMailing");
            };
         
            _connection.On<List<MailingDto>>("ReceiveMailing", async (data) =>
           {
               _mailingDtos = data;
               await ScheduleJob();
           });
            _connection.On<List<MailingDto>>("RescheduleJob", async (data) =>
            {
                _mailingDtos = data;
                await RescheduleAllJob();
            });

            // KillScheduler
            _connection.On<List<MailingDto>>("KillScheduler", async (data) =>
            {
                _mailingDtos = data;
                await UnscheduleAllJob();
            });
            // Loop is here to wait until the server is running
            await _connection.ConnectWithRetryAsync(stoppingToken);
        }

        /// <summary>
        /// Lập lịch
        /// </summary>
        /// <returns></returns>
        async Task ScheduleJob()
        {
            if (_mailingDtos != null)
            {
                var group = _mailingDtos.GroupBy(x => new { x.Frequency }).ToList();

                foreach (var item in group)
                {
                    var groupyByFrequency = item.GroupBy(x => new { x.Report, x.Frequency }).ToList();
                    foreach (var frequencycItem in groupyByFrequency)
                    {
                        var mailList = frequencycItem.SelectMany(x => x.UserList.Select(x => x.Email)).ToList();
                        var time = frequencycItem.First().TimeSend;

                        if (frequencycItem.Key.Frequency == Constants.FrequencyOption.Daily)
                        {
                            IDictionary<string, object> maps = new Dictionary<string, object>();
                            foreach (var jobMap in frequencycItem.ToList())
                            {
                                var p = new SendMailParams();
                                p.IdentityParams(jobMap.Report, jobMap.Frequency);
                                p.APIInfo(_appsettings.API_URL, jobMap.PathName, mailList);
                                var jobMapValue = JsonConvert.SerializeObject(p);

                                maps.Add("Data", jobMapValue);
                            }
                            await _scheduler.StartDaily(time.Hour, time.Minute, maps);
                        }
                        else if (frequencycItem.Key.Frequency == Constants.FrequencyOption.Weekly)
                        {
                            IDictionary<string, object> maps = new Dictionary<string, object>();
                            foreach (var jobMap in frequencycItem.ToList())
                            {
                                var p = new SendMailParams();
                                p.IdentityParams(jobMap.Report, jobMap.Frequency);
                                p.APIInfo(_appsettings.API_URL, jobMap.PathName, mailList);
                                var jobMapValue = JsonConvert.SerializeObject(p);

                                maps.Add("Data", jobMapValue);
                            }

                            await _scheduler.StartWeekly(time.DayOfWeek, time.Hour, time.Minute, maps);
                        }
                        else if (frequencycItem.Key.Frequency == Constants.FrequencyOption.Monthly)
                        {
                            IDictionary<string, object> maps = new Dictionary<string, object>();
                            foreach (var jobMap in frequencycItem.ToList())
                            {
                                var p = new SendMailParams();
                                p.IdentityParams(jobMap.Report, jobMap.Frequency);
                                p.APIInfo(_appsettings.API_URL,jobMap.PathName, mailList);
                                var jobMapValue = JsonConvert.SerializeObject(p);
                                maps.Add("Data", jobMapValue);
                            }
                            await _scheduler.StartMonthly(time.Hour, time.Minute, maps);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cập nhật lịch
        /// </summary>
        /// <returns></returns>
        async Task RescheduleAllJob()
        {
            if (_mailingDtos != null)
            {
                var group = _mailingDtos.GroupBy(x => new { x.Frequency }).ToList();
                foreach (var item in group)
                {

                    var groupyByFrequency = item.GroupBy(x => new { x.Report, x.Frequency }).ToList();
                    foreach (var frequencycItem in groupyByFrequency)
                    {
                        var mailList = frequencycItem.SelectMany(x => x.UserList.Select(x => x.Email)).ToList();
                        var time = frequencycItem.First().TimeSend;
                        if (frequencycItem.Key.Frequency == Constants.FrequencyOption.Daily)
                        {
                            IDictionary<string, object> maps = new Dictionary<string, object>();
                            foreach (var jobMap in frequencycItem.ToList())
                            {
                                var p = new SendMailParams();
                                p.IdentityParams(jobMap.Report, jobMap.Frequency);
                                p.APIInfo(_appsettings.API_URL,jobMap.PathName, mailList);
                            
                                var jobMapValue = JsonConvert.SerializeObject(p);

                                maps.Add("Data", jobMapValue);
                            }
                            await _scheduler.UpdateDailyTrigger(time.Hour, time.Minute, maps);
                        }
                        else if (frequencycItem.Key.Frequency == Constants.FrequencyOption.Weekly)
                        {
                            IDictionary<string, object> maps = new Dictionary<string, object>();
                            foreach (var jobMap in frequencycItem.ToList())
                            {
                                var p = new SendMailParams();
                                p.IdentityParams(jobMap.Report, jobMap.Frequency);
                                p.APIInfo(_appsettings.API_URL, jobMap.PathName, mailList);
                                var jobMapValue = JsonConvert.SerializeObject(p);

                                maps.Add("Data", jobMapValue);
                            }
                            await _scheduler.UpdateWeeklyTrigger(time.DayOfWeek, time.Hour, time.Minute, maps);
                        }
                        else if (frequencycItem.Key.Frequency == Constants.FrequencyOption.Monthly)
                        {

                            IDictionary<string, object> maps = new Dictionary<string, object>();
                            foreach (var jobMap in frequencycItem.ToList())
                            {
                                var p = new SendMailParams();
                                p.IdentityParams(jobMap.Report, jobMap.Frequency);
                                p.APIInfo(_appsettings.API_URL, jobMap.PathName, mailList);
                                var jobMapValue = JsonConvert.SerializeObject(p);

                                maps.Add("Data", jobMapValue);
                            }

                            await _scheduler.UpdateMonthlyTrigger(time.Hour, time.Minute, maps);

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Hủy lịch
        /// </summary>
        /// <returns></returns>
        async Task UnscheduleAllJob()
        {
            if (_mailingDtos != null)
            {
                var group = _mailingDtos.GroupBy(x => new { x.Frequency }).ToList();
                foreach (var item in group)
                {

                    var groupyByFrequency = item.GroupBy(x => new { x.Report, x.Frequency }).ToList();
                    foreach (var frequencycItem in groupyByFrequency)
                    {
                        var mailList = frequencycItem.SelectMany(x => x.UserList.Select(x => x.Email)).ToList();
                        var time = frequencycItem.First().TimeSend;
                        if (frequencycItem.Key.Frequency == Constants.FrequencyOption.Daily)
                        {
                            foreach (var jobMap in frequencycItem.ToList())
                            {
                                var triggerKey = new TriggerKey(jobMap.Report, jobMap.Frequency);
                                await _scheduler.UnscheduleJob(triggerKey);
                            }
                        }
                        else if (frequencycItem.Key.Frequency == Constants.FrequencyOption.Weekly)
                        {
                            foreach (var jobMap in frequencycItem.ToList())
                            {
                                var triggerKey = new TriggerKey(jobMap.Report, jobMap.Frequency);
                                await _scheduler.UnscheduleJob(triggerKey);
                            }
                        }
                        else if (frequencycItem.Key.Frequency == Constants.FrequencyOption.Monthly)
                        {
                            foreach (var jobMap in frequencycItem.ToList())
                            {

                                var triggerKey = new TriggerKey(jobMap.Report, jobMap.Frequency);
                                await _scheduler.UnscheduleJob(triggerKey);
                            }
                        }
                    }
                }
            }
        }
    }
}
