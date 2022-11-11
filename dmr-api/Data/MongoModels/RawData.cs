using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Data.MongoModels
{
    [BsonCollection("rawdatas")]
    public class RawData: Document
    {
        //public RawData()
        //{
        //    this.CreatedDateTime = CreatedDateTime.ToUniversalTime();
        //}

        [BsonElement("machineID")]
        public int MachineID { get; set; }
        [BsonElement("RPM")]
        public int RPM { get; set; }
        [BsonElement("duration")]
        public int Duration { get; set; }
        [BsonElement("sequence")]
        public int Sequence { get; set; }
        [BsonElement("createddatetime")]
        [BsonRepresentation(BsonType.DateTime)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDateTime { get; set; }
        public int __v { get; set; }
    }
}
