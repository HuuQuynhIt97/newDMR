using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace SetupWeighingScale
{
    class Program
    {
        static bool _continue;
        static void Main(string[] args)
        {
            string message;
            string portName30kg;
            string portName3kg;
            _continue = true;
            string path30kg = @"C:\service\weighingScale30kg\appsettings.json";
            string path3kg = @"C:\service\weighingScale3kg\appsettings.json";

            dynamic weighingScale30kg = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText(path30kg));
            dynamic weighingScale3kg = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText(path3kg));
          

            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;


            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine("Only type the number EX: COM4 -> 4");
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("--------------------weighing scale 30kg-----------------------");

            while (true)
            {
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Enter COM port value of weighing scale 30kg: ");
                portName30kg = Console.ReadLine();

                int w30kg;
                bool isNumeric30kg = int.TryParse(portName30kg, out w30kg);
                if (isNumeric30kg)
                {
                    weighingScale30kg["AppSettings"]["PortName"] = "COM" + portName30kg;
                    break;
                }
                else
                {
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Please only type the number! Press enter to try again!");
                }
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("--------------------weighing scale 3kg-----------------------");

            while (true)
            {
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Enter COM port value of weighing scale 3kg: ");
                portName3kg = Console.ReadLine();

                int w3kg;
                bool isNumeric3kg = int.TryParse(portName30kg, out w3kg);
                if (isNumeric3kg)
                {
                    weighingScale30kg["AppSettings"]["PortName"] = "COM" + portName3kg;
                    break;
                }
                else
                {
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Please only type the number! Press enter to try again!");
                }
            }
           

            string output3kg = Newtonsoft.Json.JsonConvert.SerializeObject(weighingScale3kg, Newtonsoft.Json.Formatting.Indented);
            string output30kg = Newtonsoft.Json.JsonConvert.SerializeObject(weighingScale3kg, Newtonsoft.Json.Formatting.Indented);
            try
            {
                File.WriteAllText(path3kg, output3kg);
                File.WriteAllText(path30kg, output30kg);

                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"The new COM port value of weighing scale 30kg: {"COM" + portName30kg}");
                Console.WriteLine($"The new COM port value of weighing scale 3kg: {"COM" + portName3kg}");
            }
            catch (Exception ex)
            {
                Console.ResetColor();
                Console.WriteLine(ex.Message);

                throw;
            }
          
            Console.WriteLine("Type Q to exit");
            while (_continue)
            {
                message = Console.ReadLine();

                if (stringComparer.Equals("q", message.ToLower()))
                {
                    _continue = false;
                }
                else
                {
                }
            }
        }
    }
}
