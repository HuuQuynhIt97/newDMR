using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class ActionInFunctionSystem
    {
        public int FunctionSystemID { get; set; }
        public int ActionID { get; set; }
        public Action Action { get; set; }
        public FunctionSystem FunctionSystem { get; set; }
    }
}
