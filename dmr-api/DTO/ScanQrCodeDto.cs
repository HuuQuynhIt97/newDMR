using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class ScanQrCodeDto
    {
        public string qrCode { get; set; }
        public string building { get; set; }
        public int userid { get; set; }
    }
}
