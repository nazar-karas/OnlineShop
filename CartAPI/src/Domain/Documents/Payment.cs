using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Domain.Documents
{
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string Id { get; set; }
        [BsonElement("type")]
        public string Type { get; set; }
        [BsonElement("status")]
        public string Status { get; set; }
    }
}
