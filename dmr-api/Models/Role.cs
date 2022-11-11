using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class Role
    {
        public int ID { get; set; }
        [MaxLength(50)]
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
