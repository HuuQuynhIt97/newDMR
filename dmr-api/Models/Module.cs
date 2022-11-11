using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class Module
    {
        public int ID { get; set; }
        [MaxLength(50)]
        public string Code { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        [MaxLength(100)]
        public string Icon { get; set; }
        public int Sequence { get; set; }
        public DateTime CreatedTime { get; set; }
        public ICollection<FunctionSystem> Functions { get; set; }
    }
}
