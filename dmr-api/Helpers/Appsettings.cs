using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Helpers
{
    public class Appsettings
    {
        public string Token { get; set; }
        public string API_AUTH_URL { get; set; }
        public string Signalr_URL { get; set; }
        public int SystemCode { get; set; }
        public string[] CorsPolicy { get; set; }
    }
}
