using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class BPFCEstablish
    {
        public BPFCEstablish()
        {
            if (CreatedDate == DateTime.MinValue)
                CreatedDate = DateTime.Now;
            UpdateTime = DateTime.Now;
        }
        public int ID { get; set; }
        public int ModelNameID { get; set; }
        public int ModelNoID { get; set; }
        public int ArticleNoID { get; set; }
        public int ArtProcessID { get; set; }
        public bool ApprovalStatus { get; set; }
        public bool FinishedStatus { get; set; }
        public int ApprovalBy { get; set; }
        public int CreatedBy { get; set; }
        public string Season { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? UpdateTime { get; set; }
        public DateTime? BuildingDate { get; set; }
        public DateTime? DueDate { get; set; }

        public bool IsDelete { get; set; }
        public DateTime? DeleteTime { get; set; }
        public int DeleteBy { get; set; }

        public ArtProcess ArtProcess { get; set; }
        public ModelName ModelName { get; set; }
        public ModelNo ModelNo { get; set; }
        public ArticleNo ArticleNo { get; set; }
        public ICollection<Plan> Plans { get; set; }
        public ICollection<Glue> Glues { get; set; }
    }
}
