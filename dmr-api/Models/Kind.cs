using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class Kind
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int? KindTypeID { get; set; }
        public KindType KindType { get; set; }
        public ICollection<Glue> Glues { get; set; }
    }
}
