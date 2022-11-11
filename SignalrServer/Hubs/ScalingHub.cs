using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalrServer.Hubs
{
    public class ScalingHub : Hub
    {
        //public async Task Welcom(string scalingMachineID, string message, string unit)
        //{
        //    await Clients.All.SendAsync("Welcom", scalingMachineID, message, unit);
        //}
        public async Task Welcom(string message)
        {
            await Clients.All.SendAsync("Welcom", message);
        }
        public Task JoinHub(string machineID)
        {
           return Task.CompletedTask;
        }
    }
}
