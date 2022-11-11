using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TodolistScheduleService.Constants;
using TodolistScheduleService.Dto;

namespace TodolistScheduleService.JobBase
{
   public class SendMailJobBase: IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var loggerFactory = (ILoggerFactory)new LoggerFactory();
            var path = Directory.GetCurrentDirectory();
            loggerFactory.AddFile($"{path}\\Logs\\LogJobDaily.txt");
            var logger = loggerFactory.CreateLogger(nameof(JobBase));
            var dataMap = context.JobDetail.JobDataMap;
            var json = dataMap.GetString(JobData.Data);
            SendMailParams data = JsonConvert.DeserializeObject<SendMailParams>(json);

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var query = "";
                    foreach (var email in data.Emails)
                    {
                        query += $"emails={email}&";
                    }
                    var currentDate = DateTime.Now.Date.ToString("MM-dd-yyyy");
                    var url = $"{data.URL}{data.PathName}?{query}";
                    try
                    {
                        // Thêm header vào HTTP Request
                        httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml+json");
                        HttpResponseMessage response = await httpClient.GetAsync(url);

                        // Phát sinh Exception nếu mã trạng thái trả về là lỗi
                        response.EnsureSuccessStatusCode();

                        if (response.IsSuccessStatusCode)
                        {
                            logger.LogInformation($"Send mail path: {url}");

                            logger.LogInformation($"{data.GetIdentityParams()} Send mail successfully - statusCode {(int)response.StatusCode} {response.ReasonPhrase}");
                            // Đọc nội dung content trả về
                            string htmltext = await response.Content.ReadAsStringAsync();

                        }
                        else
                        {
                            logger.LogInformation($"Send mail path: {url}");

                            logger.LogError($"Lỗi - statusCode {response.StatusCode} {response.ReasonPhrase}");
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e.Message);
                    }
                }
            }
            catch (Exception)
            {
                logger.LogError($"{data.GetIdentityParams()}The system can not send emails");
            }
        }
    }
}
