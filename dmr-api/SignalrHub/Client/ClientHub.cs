using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.SignalrHub.Client
{
    public class ClientHub
    {
        HubConnection _connection;
        public ClientHub()
        {
            _connection = new HubConnectionBuilder()
             .WithUrl("http://10.4.4.224:1009/ec-hub")
             .Build();
           
        }
        public async Task Start()
        {
            while (true)
            {

                try
                {
                    await _connection.StartAsync();
                    await Console.Out.WriteLineAsync($"ClientHub: {_connection.State}");

                    break;
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}
