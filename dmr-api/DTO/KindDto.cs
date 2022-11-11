using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class KindDto
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string KindTypeName { get; set; }
        public int? KindTypeID { get; set; }
        public KindType KindType { get; set; }
    }
}
