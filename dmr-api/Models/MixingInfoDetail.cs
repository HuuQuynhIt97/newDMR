using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class MixingInfoDetail
    {
        [Key]
        public int ID { get; set; }
        [StringLength(50)]
        public string Batch { get; set; }
        [StringLength(2)]
        public string Position { get; set; }
        public double Amount { get; set; }
        public int IngredientID { get; set; }
        public DateTime Time_Start { get; set; }
        public int MixingInfoID { get; set; }
        public Ingredient Ingredient { get; set; }
        public MixingInfo MixingInfo { get; set; }
    }
}
