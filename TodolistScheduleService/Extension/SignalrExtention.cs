using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TodolistScheduleService.Extension
{
    public static class SignalrExtention
    {
        public static async Task<bool> ConnectWithRetryAsync(this HubConnection connection, CancellationToken token)
        {
            // Keep trying to until we can start or the token is canceled.
            while (true)
            {
                try
                {
                    await connection.StartAsync(token);
                    await connection.InvokeAsync("Mailing");
                    await connection.InvokeAsync("AskMailing");
                    return true;
                }
                catch when (token.IsCancellationRequested)
                {
                    return false;
                }
                catch
                {
                    // Failed to connect, trying again in 5000 ms.
                    await Task.Delay(2000);
                }
            }
        }
    }
}
