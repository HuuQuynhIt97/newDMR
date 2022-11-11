using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Data.MongoModels
{
    [BsonCollection("settings")]
    public class Setting
    {
        [BsonElement("machineID")]
        public int MachineID { get; set; }
        [BsonElement("standardRPM")]
        public int StandardRPM { get; set; }
        [BsonElement("startBuzzerAffter")]
        public int StartBuzzerAffter { get; set; }
        [BsonElement("stopWatch")]
        public int StopWatch { get; set; }
        [BsonElement("timer")]
        public int Timer { get; set; }
        [BsonElement("minRPM")]
        public int MinRPM { get; set; }
    }
}
