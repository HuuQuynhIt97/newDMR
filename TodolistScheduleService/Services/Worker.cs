using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using QuartzExtention.Constants;
using QuartzExtention.Schedulers;
using SignalRExtension;
using TodolistScheduleService.Dto;
using TodolistScheduleService.JobBase;
using TodolistScheduleService.Schedulers;

namespace TodolistScheduleService.Services
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        QuartzExtention.Schedulers.ISchedulerBase<SendMailJobBase, SendMailParams> _scheduler;
        private readonly Appsettings _appsettings;
        private ISignalRClient _client;
        List<MailingDto> _mailingDtos = null;
        ///
        public Worker(
            ILogger<Worker> logger,
            Appsettings appsettings
            )
        {
            _logger = logger;

            _appsettings = appsettings;

            _client = new SignalRClient(_appsettings.SignalRConnection);

            _scheduler = new SchedulerBase<SendMailJobBase, SendMailParams>();
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.DisposeAsync();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _scheduler.StartAllJob();

            await _client.OnReconnecting(async (connection) =>
           {
               _logger.LogInformation("Reconnecting" + DateTime.Now.ToString());
               _logger.LogInformation($"Reconnecting {connection.State} {DateTime.Now.ToString()}");
               var job2 = await _scheduler.GetAllJobKeyAsync();
               var trigger2 = await _scheduler.GetAllTriggerKey();
               await _scheduler.Clear();
               var job = await _scheduler.GetAllJobKeyAsync();
               var trigger = await _scheduler.GetAllTriggerKey();
           });

            await _client.OnClosed(async (connection) =>
            {
                _logger.LogInformation("Closed" + DateTime.Now.ToString());

                await _scheduler.Clear();
                await _client.StartHub(async (connection) =>
                {
                    await connection.InvokeAsync("Mailing");
                    await connection.InvokeAsync("AskMailing");
                });

            });

            await _client.OnReconnected(async (connection) =>
            {
                _logger.LogInformation("OnReconnected" + DateTime.Now.ToString());
                Console.WriteLine($"OnReconnected {connection.State}");
                await connection.InvokeAsync("Mailing");
                await connection.InvokeAsync("AskMailing");
            });

            await _client.OnEvent(async (connection) =>
            {
                connection.On<List<MailingDto>>("ReceiveMailing", async (data) =>
                {
                    Console.WriteLine($"ReceiveMailing ScheduleJob {DateTime.Now.ToString()}");
                    _mailingDtos = data;
                    await ScheduleJob();
                });
                connection.On<List<MailingDto>>("RescheduleJob", async (data) =>
                {
                    _mailingDtos = data;
                    await RescheduleAllJob();
                });

                // KillScheduler
                connection.On<List<MailingDto>>("KillScheduler", async (data) =>
                {
                    _mailingDtos = data;
                    await UnscheduleAllJob();
                });
                await Task.CompletedTask;
            });

            // Loop is here to wait until the server is running
            await _client.StartHub(async (connection) =>
            {
                await connection.InvokeAsync("Mailing");
                await connection.InvokeAsync("AskMailing");
            });
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

                                maps.Add(JobData.Data, jobMapValue);
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

                                maps.Add(JobData.Data, jobMapValue);
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
                                p.APIInfo(_appsettings.API_URL, jobMap.PathName, mailList);
                                var jobMapValue = JsonConvert.SerializeObject(p);
                                maps.Add(JobData.Data, jobMapValue);
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
                                p.APIInfo(_appsettings.API_URL, jobMap.PathName, mailList);

                                var jobMapValue = JsonConvert.SerializeObject(p);

                                maps.Add(JobData.Data, jobMapValue);
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

                                maps.Add(JobData.Data, jobMapValue);
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

                                maps.Add(JobData.Data, jobMapValue);
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
