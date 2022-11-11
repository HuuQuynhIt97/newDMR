using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class Plan
    {
        public Plan()
        {
            var currentTime = DateTime.Now;
            CreatedDate = currentTime;
            StartWorkingTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 7, 30, 00);
            FinishWorkingTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 16, 30, 00);
        }

        public int ID { get; set; }
        public int BuildingID { get; set; }
        public int BPFCEstablishID { get; set; }
        public int Quantity { get; set; }
        public string BPFCName { get; set; }
        public int HourlyOutput { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime StartWorkingTime { get; set; }
        public DateTime FinishWorkingTime { get; set; }
        public int WorkingHour { get; set; }
        public bool IsGenarateTodo { get; set; }
        public bool IsRefreshTodo { get; set; }
        public bool IsChangeBPFC { get; set; }
        public bool IsDelete { get; set; }
        public bool IsOvertime { get; set; }
        public bool IsOffline { get; set; } // v102

        public DateTime? UpdatedOffline { get; set; }
        public DateTime? UpdatedOnline { get; set; }

        public DateTime? UpdatedOvertime { get; set; }
        public DateTime? UpdatedNoOvertime { get; set; }

        public DateTime DeleteTime { get; set; }
        public DateTime ModifyTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public int DeleteBy { get; set; }
        public int CreateBy { get; set; }// v102
        public int UpdatedBy { get; set; }// v102

        public int UpdatedOfflineBy { get; set; }// v102
        public int UpdatedOnlineBy { get; set; }// v102

        public int UpdatedOvertimeBy { get; set; }// v102
        public int UpdatedNoOvertimeBy { get; set; }// v102

        public Building Building { get; set; }
        public BPFCEstablish BPFCEstablish { get; set; }
        public ICollection<PlanDetail> PlanDetails { get; set; }
        public ICollection<ToDoList> ToDoList { get; set; }
        public ICollection<DispatchList> DispatchList { get; set; }
        public ICollection<Station> Stations { get; set; }

    }
}
