using DMR_API.Helpers;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DMR_API.SchedulerHelper.Jobs
{
    public class ReloadDispatchJob :   IJob
    {
        HubConnection _connection;

        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string Signalr_URL = dataMap.GetString("Signalr_URL");

            _connection = new HubConnectionBuilder()
            .WithUrl(Signalr_URL)
            .Build();
            // Loop is here to wait until the server is running
            while (true)
            {

                try
                {
                    await _connection.StartAsync();
                    await _connection.InvokeAsync("JoinReloadDispatch");
                    break;
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }
            await Console.Out.WriteLineAsync($"Hub: {_connection.State}");


            try
            {
                //using (var httpClient = new HttpClient())
                //{
                //    var currentDate = DateTime.Now.Date.ToString("MM/dd/yyyy");
                //    var url = $"http://10.4.0.76:1044/api/ToDoList/SendMail/{currentDate}/{currentDate}";
                //    Console.WriteLine($"Starting connect {url}");
                //    try
                //    {
                //        // Thêm header vào HTTP Request
                //        httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml+json");
                //        HttpResponseMessage response = await httpClient.GetAsync(url);

                //        // Phát sinh Exception nếu mã trạng thái trả về là lỗi
                //        response.EnsureSuccessStatusCode();

                //        if (response.IsSuccessStatusCode)
                //        {
                //            Console.WriteLine($"Tải thành công - statusCode {(int)response.StatusCode} {response.ReasonPhrase}");
                //            // Đọc thông tin header trả về


                //            Console.WriteLine("Starting read data");

                //            // Đọc nội dung content trả về
                //            string htmltext = await response.Content.ReadAsStringAsync();
                //            Console.WriteLine($"Nhận được {htmltext.Length} ký tự");
                //            Console.WriteLine();
                //        }
                //        else
                //        {
                //            Console.WriteLine($"Lỗi - statusCode {response.StatusCode} {response.ReasonPhrase}");
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine(e.Message);
                //    }
                //}
                await _connection.InvokeAsync("ReloadDispatch");
                await _connection.DisposeAsync();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Reload Dispatch");
                Console.WriteLine($"ReloadDispatch ClientHub: {_connection.State}");
                Console.ResetColor();
            }
            catch (Exception)
            {
                await Console.Out.WriteLineAsync("The system can not reload dispatch");

            }
        }

       
    }
}
