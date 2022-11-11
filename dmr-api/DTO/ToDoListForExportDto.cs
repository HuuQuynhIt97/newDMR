using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class ToDoListForExportDto
    {
        public int Sequence { get; set; }
        public string Line { get; set; }
        public int Station { get; set; }
        public string ModelName { get; set; }
        public string ModelNO { get; set; }
        public string ArticleNO { get; set; }
        public string Supplier { get; set; }
        public string GlueName { get; set; }
        public int BuildingID { get; set; }

        public DateTime? StartMixingTime { get; set; }
        public DateTime? FinishMixingTime { get; set; }

        public DateTime? StartStirTime { get; set; }
        public DateTime? FinishStirTime { get; set; }
        public double StirCicleTime { get; set; }
        public double AverageRPM { get; set; }

        public DateTime? StartDispatchingTime { get; set; }
        public DateTime? FinishDispatchingTime { get; set; }

        public DateTime? PrintTime { get; set; }

        public double StandardConsumption { get; set; }
        public double MixedConsumption { get; set; }
        public double DeliveredConsumption { get; set; }
        public double DeliveredConsumptionEachLine { get; set; }

        public string Status { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }
    }
}
