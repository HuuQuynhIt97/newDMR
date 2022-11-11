using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TodolistScheduleService.Dto;
using TodolistScheduleService.Jobs;
using TodolistScheduleService.Services;

namespace TodolistScheduleService.Schedulers
{

    public class SchedulerMultiple
    {
        IScheduler _scheduler;
        private readonly ILogger<Todo> _logger;
        private readonly List<TriggerKey> _triggerKeys = new List<TriggerKey>();
        private readonly List<JobKey> _jobKeys = new List<JobKey>();
        public SchedulerMultiple(ILogger<Todo> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Bắt đầu lập lịch
        /// </summary>
        /// <returns></returns>
        public async Task StartAllJob()
        {
            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await _scheduler.Start();
        }

        /// <summary>
        /// Lập lịch hàng ngày
        /// </summary>
        /// <param name="hour">Giờ</param>
        /// <param name="minute">Phút</param>
        /// <param name="map">Các tham số muốn tiêm vào JobDetail</param>
        /// <returns></returns>
        public async Task StartDaily(int hour, int minute, IDictionary<string, object> map)
        {
            var json = map["Data"].ToString();
            SendMailParams data = JsonConvert.DeserializeObject<SendMailParams>(json);
            var triggerKey = new TriggerKey(data.Name, data.Group);
            var jobKey = new JobKey(data.Name, data.Group);

            var jobDaily = JobBuilder.Create<SendMailJob>()
                                .WithIdentity(jobKey)
                                .SetJobData(new JobDataMap(map))
                                .Build();

            var triggerDaily = TriggerBuilder.Create()
                 .WithIdentity(triggerKey)
                 .WithDailyTimeIntervalSchedule
                   (s =>
                      s.WithIntervalInHours(24)
                     .OnEveryDay()
                     .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(hour, minute))
                   )

                 .Build();
            await _scheduler.ScheduleJob(jobDaily, triggerDaily);
            _triggerKeys.Add(triggerKey);
            _jobKeys.Add(jobKey);
            _logger.LogInformation($"Add {triggerKey.Name}-{triggerKey.Group} at {DateTime.Now.ToString("dd-MM-yyyy HH:mm")}");
            _logger.LogInformation($"Add {triggerKey.Name}-{triggerKey.Group} fire at {hour.ToString("D2") }:{minute.ToString("D2")} everyday.");


        }

        /// <summary>
        /// Lập lịch hàng tuần
        /// </summary>
        /// <param name="dayOfWeek">Ngày trong tuần</param>
        /// <param name="hour">Giờ</param>
        /// <param name="minute">Phút</param>
        /// <param name="map">Các tham số muốn tiêm vào JobDetail</param>
        /// <returns></returns>
        public async Task StartWeekly(DayOfWeek dayOfWeek, int hour, int minute, IDictionary<string, object> map)
        {
            var json = map["Data"].ToString();
            SendMailParams data = JsonConvert.DeserializeObject<SendMailParams>(json);
            var triggerKey = new TriggerKey(data.Name, data.Group);
            var jobKey = new JobKey(data.Name, data.Group);
            var jobWeekly = JobBuilder.Create<SendMailWeeklyJob>()
                .WithIdentity(jobKey)
                            .SetJobData(new JobDataMap(map))
                            .Build();

            var triggerWeekly = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(dayOfWeek, hour, minute))
                .Build();

            await _scheduler.ScheduleJob(jobWeekly, triggerWeekly);
            _triggerKeys.Add(triggerKey);
            _jobKeys.Add(jobKey);
            _logger.LogInformation($"Add {triggerKey.Name}-{triggerKey.Group} at {DateTime.Now.ToString("dd-MM-yyyy HH:mm")}");
            _logger.LogInformation($"Add {triggerKey.Name}-{triggerKey.Group} fire at {hour.ToString("D2") }:{minute.ToString("D2")} every week.");


        }
        /// <summary>
        /// Lập lịch hàng tháng
        /// </summary>
        /// <param name="hour">Giờ</param>
        /// <param name="minute">Phút</param>
        /// <param name="map">Các tham số muốn tiêm vào JobDetail</param>
        /// <returns></returns>
        public async Task StartMonthly(int hour, int minute, IDictionary<string, object> map)
        {
            var json = map["Data"].ToString();
            SendMailParams data = JsonConvert.DeserializeObject<SendMailParams>(json);
            var triggerKey = new TriggerKey(data.Name, data.Group);
            var jobKey = new JobKey(data.Name, data.Group);
            var jobMonthly = JobBuilder.Create<SendMailMonthlyJob>()
                .WithIdentity(jobKey)
                .SetJobData(new JobDataMap(map))
                .Build();

            var triggerMonthly = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .WithCronSchedule
                  ($"0 {minute} {hour} L * ?") // Meaning: Fire at 10:15am on the last day of every month
                .ForJob(jobMonthly.Key)
                .Build();
            await _scheduler.ScheduleJob(jobMonthly, triggerMonthly);
            _triggerKeys.Add(triggerKey);
            _jobKeys.Add(jobKey);
            _logger.LogInformation($"Add {triggerKey.Name}-{triggerKey.Group} at {DateTime.Now.ToString("dd-MM-yyyy HH:mm")}");
            _logger.LogInformation($"Add {triggerKey.Name}-{triggerKey.Group} fire at {hour.ToString("D2") }:{minute.ToString("D2")} on the last day of every month.");


        }

        public async Task StartDisaptch(int repeatMinute, TimeSpan startHourAt, TimeSpan endHourAt)
        {

            var jobDispatch = JobBuilder.Create<ReloadDispatchJob>().Build();
            var st = DateTime.Now.Date.Add(startHourAt);
            var end = DateTimeOffset.Now.Date.Add(endHourAt);
            var triggerDisaptch = TriggerBuilder.Create()
                         .StartAt(st)
                         .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(repeatMinute))
                         .EndAt(end)
                         .Build();

            await _scheduler.ScheduleJob(jobDispatch, triggerDisaptch);
        }
        public async Task StartTodo(int repeatMinute, TimeSpan startHourAt, TimeSpan endHourAt)
        {
            var jobTodo = JobBuilder.Create<ReloadTodoJob>().Build();
            var st = DateTime.Now.Date.Add(startHourAt);
            var end = DateTimeOffset.Now.Date.Add(endHourAt);
            var triggerTodo = TriggerBuilder.Create()
                         .StartAt(st)
                         .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(repeatMinute))
                         .EndAt(end)
                         .Build();
            await _scheduler.ScheduleJob(jobTodo, triggerTodo);
        }


        /// <summary>
        /// Kiểm tra đã bắt đầu lập lịch
        /// </summary>
        /// <returns></returns>
        public async Task<bool> checkScheduleStart()
        {
            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            return _scheduler.IsStarted;
        }
        // You can't change (update) a job once it has been scheduled. 
        //You can only re-schedule it (with any changes you might want to make) or delete it and create a new one.
        public async Task UpdateDailyTrigger(int hour, int minute, IDictionary<string, object> map)
        {
            var json = map["Data"].ToString();
            SendMailParams data = JsonConvert.DeserializeObject<SendMailParams>(json);
            var triggerKey = new TriggerKey(data.Name, data.Group);

            var jobKey = new JobKey(data.Name, data.Group);
            var trigger = await _scheduler.GetTrigger(triggerKey);


            var checkExists = await _scheduler.CheckExists(jobKey);
            if (checkExists)
            {

                var builder = trigger.GetTriggerBuilder();
                builder.StartNow();

                var newtriggerDaily = builder
                   .WithDailyTimeIntervalSchedule
                     (s =>
                        s.WithIntervalInHours(24)
                       .OnEveryDay()
                       .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(hour, minute))
                     )
                   .Build();

                await _scheduler.RescheduleJob(trigger.Key, newtriggerDaily);
                _logger.LogInformation($"Update {triggerKey.Name}-{triggerKey.Group} at {DateTime.Now.ToString("dd-MM-yyyy HH:mm")}");
                _logger.LogInformation($"Update {triggerKey.Name}-{triggerKey.Group} fire at {hour.ToString("D2") }:{minute.ToString("D2")} everyday.");

            }
            else
            {
                await StartDaily(hour, minute, map);
            }
        }

        /// <summary>
        /// Cập nhật lịch hàng tuần
        /// </summary>
        /// <param name="dayOfWeek">Ngày trong tuần</param>
        /// <param name="hour">Giờ</param>
        /// <param name="minute">Phút</param>
        /// <param name="map">Các tham số muốn tiêm vào JobDetail</param>
        /// <returns></returns>
        public async Task UpdateWeeklyTrigger(DayOfWeek dayOfWeek, int hour, int minute, IDictionary<string, object> map)
        {
            var json = map["Data"].ToString();
            SendMailParams data = JsonConvert.DeserializeObject<SendMailParams>(json);
            var triggerKey = new TriggerKey(data.Name, data.Group);

            var jobKey = new JobKey(data.Name, data.Group);
            var trigger = await _scheduler.GetTrigger(triggerKey);
            var job = await _scheduler.GetJobDetail(jobKey);
            var checkExists = await _scheduler.CheckExists(jobKey);
            if (checkExists)
            {
                var builderJob = job.GetJobBuilder();
                builderJob.UsingJobData(new JobDataMap(map));
                builderJob.Build();

                var builder = trigger.GetTriggerBuilder();
                builder.StartNow();
                var newtriggerWeekly = builder
                    .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(dayOfWeek, hour, minute))
                   .Build();

                await _scheduler.RescheduleJob(trigger.Key, newtriggerWeekly);
                _logger.LogInformation($"Update {triggerKey.Name}-{triggerKey.Group} at {DateTime.Now.ToString("dd-MM-yyyy HH:mm")}");
                _logger.LogInformation($"Update {triggerKey.Name}-{triggerKey.Group} fire at {hour.ToString("D2") }:{minute.ToString("D2")} every week.");

            }
            else
            {
                await StartWeekly(dayOfWeek, hour, minute, map);
            }
        }
        /// <summary>
        /// Cập nhật lịch hàng tháng
        /// </summary>
        /// <param name="hour">Giờ</param>
        /// <param name="minute">Phút</param>
        /// <param name="map">Các tham số muốn tiêm vào JobDetail</param>
        /// <returns></returns>
        public async Task UpdateMonthlyTrigger(int hour, int minute, IDictionary<string, object> map)
        {
            var json = map["Data"].ToString();
            SendMailParams data = JsonConvert.DeserializeObject<SendMailParams>(json);
            var triggerKey = new TriggerKey(data.Name, data.Group);

            var jobKey = new JobKey(data.Name, data.Group);
            var trigger = await _scheduler.GetTrigger(triggerKey);
            var job = await _scheduler.GetJobDetail(jobKey);
            var checkExists = await _scheduler.CheckExists(jobKey);
            if (checkExists)
            {
                var builderJob = job.GetJobBuilder();
                builderJob.UsingJobData(new JobDataMap(map));
                builderJob.Build();

                var builder = trigger.GetTriggerBuilder();
                builder.StartNow();
                var newtriggerMonthly = builder
                    .WithCronSchedule
                      ($"0 {minute} {hour} L * ?") // Meaning: Fire at 10:15am on the last day of every month
                      .ForJob(jobKey)
                    .Build();

                // if you don't want that it starts now, pass 'false' for the 'startNow' parameter
                await _scheduler.RescheduleJob(trigger.Key, newtriggerMonthly);
                _logger.LogInformation($"Update {triggerKey.Name}-{triggerKey.Group} at {DateTime.Now.ToString("dd-MM-yyyy HH:mm")}");
                _logger.LogInformation($"Update {triggerKey.Name}-{triggerKey.Group} fire at {hour.ToString("D2") }:{minute.ToString("D2")} on the last day of every month.");


            }
            else
            {
                await StartMonthly(hour, minute, map);
            }

        }
        /// <summary>
        /// Hủy lịch
        /// </summary>
        /// <param name="triggerKey"></param>
        /// <returns></returns>
        public async Task UnscheduleJob(TriggerKey triggerKey)
        {
            var trigger = await _scheduler.GetTrigger(triggerKey);
            var jobKey = new JobKey(triggerKey.Name, triggerKey.Group);
            if (trigger != null)
            {
                await _scheduler.UnscheduleJob(trigger.Key);
                _triggerKeys.Remove(triggerKey);
                _jobKeys.Remove(jobKey);
                _logger.LogInformation($"UnscheduleJob {triggerKey.Name}-{triggerKey.Group} at {DateTime.Now.ToString("dd-MM-yyyy HH:mm")}");
            }
            await Task.CompletedTask;
        }
        public async Task Stop()
        {
            if (_scheduler.IsStarted)
            {
                await _scheduler.Shutdown();
            }
        }
        public async Task Clear()
        {
            await _scheduler.Clear();
        }
        public async Task<List<TriggerKey>> GetAllTriggerKey()
        {
            var result = new List<TriggerKey>();
            foreach (var item in _triggerKeys)
            {
                var trigger = await _scheduler.GetTrigger(item);
                if (trigger != null)
                    result.Add(trigger.Key);
            }
            return result;
        }
        public async Task<List<JobKey>> GetAllJobKeyAsync()
        {
            var result = new List<JobKey>();
            foreach (var item in _jobKeys)
            {
                var job = await _scheduler.GetJobDetail(item);
                if (job != null)
                    result.Add(job.Key);
            }
            return result;
        }

    }
}
