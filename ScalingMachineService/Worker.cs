using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ScalingMachineService.Helpers;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using Newtonsoft.Json;

namespace ScalingMachineService
{
    public class Worker : BackgroundService
    {
        SerialPort _serialPort;
        HubConnection _connection;
        bool _continue;
        DateTime lastSend;
        private readonly AppSettings _appSettings;
        private readonly ILogger<Worker> _logger;
        HubConnectionState state = HubConnectionState.Disconnected;

        public Worker(ILogger<Worker> logger, AppSettings appSettings)
        {
            _logger = logger;
            _appSettings = appSettings;
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker Start: {time}", DateTimeOffset.Now);
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker Stop: {time}", DateTimeOffset.Now);
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            await base.StopAsync(cancellationToken);
        }

        public async Task<bool> ConnectWithRetryAsync(CancellationToken token)
        {
            // Keep trying to until we can start or the token is canceled.
            while (true)
            {

                try
                {
                    await _connection.StartAsync(token);
                    // Debug.Assert(_connection.State == HubConnectionState.Connected);
                    _logger.LogInformation($"The signalr client connected at {DateTime.Now.ToString()}");

                    state = HubConnectionState.Connected;
                   // await _connection.InvokeAsync("JoinHub", _appSettings.MachineID);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);

                    // Failed to connect, trying again in 5000 ms.
                    // Debug.Assert(_connection.State == HubConnectionState.Disconnected);
                    await Task.Delay(5000);
                    _logger.LogInformation($"The signalr client is reconnecting at {DateTime.Now.ToString()}");

                }
                if (state == HubConnectionState.Connected)
                {
                    return true;
                }
            }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            _connection = new HubConnectionBuilder()
          .WithUrl(_appSettings.SignalrUrl)
          .WithAutomaticReconnect()
          .Build();
            MyConsole.Info(JsonConvert.SerializeObject(_appSettings));
            _connection.On<string, string, string>("Welcom", (scalingMachineID, message, unit) =>
            {
                if (scalingMachineID == _appSettings.MachineID.ToString())
                {
                    var receiveValue = JsonConvert.SerializeObject(new
                    {
                        scalingMachineID,
                        message,
                        unit
                    });

                    MyConsole.Info($"#### ### Data => {receiveValue}");
                }
            });

            _connection.Closed += async (error) =>
            {
                _logger.LogError(error.Message);
                await Task.Delay(new Random().Next(0, 5) * 1000);
                _logger.LogError($"Envent: Closed - The signalr client is restarting!");
                await _connection.StartAsync();
            };
            _connection.Reconnecting += async error =>
            {
                _logger.LogInformation($"State Hub {_connection.State} - State Global {state}");
                _logger.LogInformation($"Connection started reconnecting due to an error: {error.Message}");

                if (_connection.State == HubConnectionState.Reconnecting)
                    state = HubConnectionState.Disconnected;
                while (state == HubConnectionState.Disconnected)
                {
                    if (await ConnectWithRetryAsync(stoppingToken))
                    {
                        break;
                    }
                }
            };
            _connection.Reconnected += async (connectionId) =>
           {
               _logger.LogInformation($"Connection successfully reconnected. The ConnectionId is now: {connectionId}");
               state = HubConnectionState.Connected;
               while (true)
                   if (await ConnectWithRetryAsync(stoppingToken)) break;
           };

            while (state == HubConnectionState.Disconnected)
                if (await ConnectWithRetryAsync(stoppingToken)) break;

            _logger.LogInformation($"#### ### ClientId: {_connection.ConnectionId}");

            string message;
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(async () =>
                 await Read()
                );

            // Create a new SerialPort object with default settings.
            _logger.LogInformation($"#### ### Serial Port is established");

            _serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            string port = _appSettings.PortName;
            _serialPort.PortName = port;
            string scanPortName = ScanPortName();
            while (!port.Equals(scanPortName))
            {
                if (state == HubConnectionState.Connected)
                {
                    _logger.LogWarning($"#### ### The system is scanning {port} at {DateTime.Now.ToString()}");

                    scanPortName = ScanPortName();
                    await Task.Delay(1000, stoppingToken);
                }
            }
            _logger.LogInformation($"#### ### {port}");

            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.Open();
            _logger.LogWarning($"#### ### #### ### Serial Port is already open at {DateTime.Now.ToString()}");

            _continue = true;
            readThread.Start();
            while (_continue)
            {
                // emit ve client
                message = Console.ReadLine();

                if (stringComparer.Equals("quit", message))
                {
                    _continue = false;
                }
                else
                {

                    //_serialPort.WriteLine(
                    //    String.Format("<{0}>: {1}", name, message));
                }
                Thread.Sleep(10);
            }
            readThread.Join();
            _serialPort.Close();

        }

        public async Task Read()
        {
            while (_continue)
            {
                try
                {
                    string message = _serialPort.ReadLine();
                    if (message.Trim() != string.Empty)
                    {
                        var unit = new Regex(@"[a-zA-Z]").Match(message).Value.ToSafetyString();
                        var messages = new Regex(@"[+-]?([0-9]*[.])?[0-9]+").Match(message).Value.ToFloat();

                        //Quỳnh mới thêm phần này lúc 14:26 PM ngày 18/12/2020 :v
                        //await Task.Delay(1000);
                        // await _connection.InvokeAsync("Welcom", _appSettings.MachineID.ToString(), messages.ToString(), unit.Trim());

                        //Quỳnh mới bỏ phần này lúc 14:20 PM ngày 18/12/2020
                        // mỗi 100ms mới gửi dữ liệu
                        double time = (DateTime.Now - lastSend).TotalMilliseconds;
                        if (time >= _appSettings.CycleTime)
                        {
                            lastSend = DateTime.Now;
                            try
                            {
                                await _connection.InvokeAsync("Welcom", _appSettings.MachineID.ToString(), messages.ToString(), unit.Trim());
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex.Message);
                            }
                        }
                        //if (before != messages && (DateTime.Now - lastSend).TotalMilliseconds >= 100)
                        //{
                        //    before = messages;
                        //    try
                        //    {
                        //        await _connection.InvokeAsync("Welcom", _appSettings.MachineID.ToString(), messages.ToString(), unit.Trim());
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        _logger.LogError(ex.Message);
                        //    }
                        //}
                    }
                }
                catch (System.TimeoutException ex)
                {
                    _logger.LogError(ex.Message);
                }

            }

        }
        private string ScanPortName()
        {
            string portResult = string.Empty;
            // Get a list of serial port names.
            string[] ports = SerialPort.GetPortNames();

            // Display each port name to the console.
            foreach (string port in ports)
            {
                if (_appSettings.PortName.ToLower() == port.ToLower())
                {
                    return port;
                }
            }
            return portResult;
        }

    }
}
