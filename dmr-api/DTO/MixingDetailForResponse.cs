using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class MixingDetailForResponse
    {
        public MixingDetailForResponse()
        {
            DeliveryConsumption = 0 + "kg";
            MixedConsumption = 0 + "kg";
        }

        public MixingDetailForResponse(double mixedCon, double deliverCon)
        {
            MixedConsumption = Math.Round(mixedCon, 2) + "kg";
            DeliveryConsumption = Math.Round(deliverCon, 2) + "kg";
        }
        public MixingDetailForResponse(double mixedCon, DateTime mixedTime, double deliverCon, DateTime deliveryTime)
        {
            MixedConsumption = Math.Round(mixedCon, 2) + "kg";
            DeliveryConsumption = Math.Round(deliverCon, 2) + "kg";
            MixedTime = mixedTime;
            DeliveryTime = deliveryTime;
        }
        public string MixedConsumption { get; set; }
        public string DeliveryConsumption { get; set; }
        public DateTime MixedTime { get; set; }
        public DateTime DeliveryTime { get; set; }
    }
}
