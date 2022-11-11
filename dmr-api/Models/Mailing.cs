using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class Mailing
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public int UserID { get; set; }
        public string Frequency { get; set; }
        public string Report { get; set; }
        public string PathName { get; set; }
        public DateTime TimeSend { get; set; }
    }
}
