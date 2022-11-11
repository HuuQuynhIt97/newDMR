
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WeighingScaleEmulator
{
    class Program
    {
        static DateTime lastSend;
        static async Task Main(string[] args)
        {


            await Client("http://10.4.4.224:1009/ec-hub", "Client 1", "E");

        }


        static async Task Client(string path, string name, string building)
        {
            var _connection = new HubConnectionBuilder()
              .WithUrl("http://localhost:5001/scalingHub")
             .Build();

            _connection.On<string, string, string>("Welcom", (scalingMachineID, amount, unit) =>
            {
                string text = unit != "g" ? $"{name} The big one: " : $"{name} The small one: ";
                string newMessage = $"#### ### {text} {scalingMachineID}: {amount}{unit} {building}";
                Console.ForegroundColor = unit != "g" ? ConsoleColor.Green : ConsoleColor.White;
                Console.WriteLine(newMessage);
            });
            await _connection.StartAsync();
            Console.WriteLine(_connection.State);


            while (true)
            {
                Parallel.Invoke(
                async () =>
                {
                    //double kg = Math.Round(RandomNumber(100, 134), 2);
                    Thread.Sleep(1000);

                    await _connection.InvokeAsync("Welcom", "3", 74.6 + "", "g");
                },
                 async () =>
                 {
                     Thread.Sleep(1000);

                     //double kg = Math.Round(RandomNumber(100, 134), 2);
                     await _connection.InvokeAsync("Welcom", "3", 495 + "", "g");
                 },

                   async () =>
                   {
                       Thread.Sleep(1000);

                       //double kg = Math.Round(RandomNumber(100, 134), 2);
                       await _connection.InvokeAsync("Welcom", "3", 245 + "", "g");
                   },
                async () =>
                {
                    Thread.Sleep(1000);

                    double kg = Math.Round(RandomNumber(3.5, 4.2), 2);
                    await _connection.InvokeAsync("Welcom", "4", 5 + "", "k");
                }



                // async () =>
                // {
                //     double kg = Math.Round(RandomNumber(100, 124), 2);
                //     await _connection.InvokeAsync("Welcom", "2", kg + "", "k");
                // },
                // async () =>
                // {
                //     double kg = Math.Round(RandomNumber(3, 3.2), 2);
                //     await _connection.InvokeAsync("Welcom", "2", kg + "", "k");
                // },

                // async () =>
                // {
                //     double g = Math.Round(RandomNumber(940, 950), 2);
                //     await _connection.InvokeAsync("Welcom", "1", g.ToString(), "g");
                // }
                );

                Thread.Sleep(1000);

            }
        }

        public static double RandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}
