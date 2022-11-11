using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class MixingInfoDetailForAddDto
    {
        public int ID { get; set; }
        public double Amount { get; set; }
        public string Position { get; set; }
        public string Batch { get; set; }
        public DateTime Time_Start { get; set; } //Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
        public int IngredientID { get; set; }
        public int MixingInfoID { get; set; }
    }

}
