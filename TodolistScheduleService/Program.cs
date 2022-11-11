using System.IO;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TodolistScheduleService.Services;

namespace TodolistScheduleService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(loggerFactory =>
                {
                    var path = Directory.GetCurrentDirectory();
                    loggerFactory.AddFile($"{path}\\Logs\\Log.txt");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;
                    Appsettings appsettings = configuration.GetSection("AppSettings").Get<Appsettings>();

                   // HubConnection _connection = new HubConnectionBuilder()
                   //.WithUrl(appsettings.SignalRConnection)
                   //.WithAutomaticReconnect(new RandomRetryPolicy())
                   //.Build();
                    //services.AddSingleton<HubConnection>(_connection);
                    services.AddSingleton<Appsettings>(appsettings);
                    services.AddHostedService<Worker>();
                }).UseWindowsService();

    }
   
}
