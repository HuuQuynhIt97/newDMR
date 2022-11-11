using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using TodolistScheduleService.Schedulers;

namespace TodolistScheduleService.Services
{
    public class Todolist : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private HubConnection _connection;
        private bool _flag = true;
        private List<string> emails = new List<string>();
        DateTime lastSend;
        Scheduler _scheduler;
        public Todolist(ILogger<Worker> logger)
        {
            _connection = new HubConnectionBuilder()
              .WithUrl("http://10.4.0.76:1004/ec-hub")
              .Build();
            Console.WriteLine($"Hub State: {_connection.State}");
            _logger = logger;
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogError($"Todolist Service StopAsync at: { DateTimeOffset.Now}");
            await _connection.DisposeAsync();
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Loop is here to wait until the server is running
            while (true)
            {
                _logger.LogInformation($"Hub: {_connection.State}");

                try
                {
                    await _connection.StartAsync(stoppingToken);
                    break;
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }
         
            _logger.LogInformation($"Hub State: {_connection.State}");
            _connection.On<int>("ReceiveTodolist", (building) =>
           {
               _logger.LogInformation($"ReceiveTodolist building: {building}");
           });
            _connection.On("ReceiveCreatePlan", () =>
            {
                _logger.LogInformation($"ReceiveCreatePlan");
                _flag = true;
            });
            _connection.Closed += async (error) =>
            {
                _flag = false;
                _logger.LogError(error.Message);
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await _connection.StartAsync();
            };
            _connection.Reconnecting += (error) =>
           {
               _logger.LogError(error.Message);
               _logger.LogInformation($"Singnalr Reconnecting: { DateTimeOffset.Now} ---- Flag: {_flag}");

               return Task.CompletedTask;
           };
            _connection.Reconnected += async (connectionId) =>
           {
               _flag = true;
               _logger.LogInformation($"{connectionId} reconnected at: { DateTimeOffset.Now} ---- Flag: {_flag}");
               await Task.CompletedTask;
           };
          
            while (true)
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    try
                    {
                        await _connection.StartAsync(stoppingToken);
                        if (_connection.State == HubConnectionState.Connected)
                        {
                            _logger.LogInformation($"Hub: {_connection.State}");
                            _flag = true;
                        }
                    }
                    catch
                    {
                        await Task.Delay(3000);
                    }
                }

                // dowork hear
                var ct = DateTime.Now;
                var dt = new DateTime(ct.Year, ct.Month, ct.Day, 17, 30, 0);

                if (ct.TimeOfDay == dt.TimeOfDay)
                {
                    await _connection.InvokeAsync("SendMail", "2");
                    _logger.LogInformation($"###### Da gui mail {DateTime.Now.ToString("MMM dd, yyyy HH:mm:ss")}");
                }
                await Task.Delay(1000);

            }


        }

    }
}
