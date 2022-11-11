using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class Version
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string UploadBy { get; set; }
        public string Description { get; set; }
        public DateTime UpatedTime { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
