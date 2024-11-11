using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Domain.Documents
{
    public class Carrier
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string Id { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("department")]
        public int Department { get; set; }
        [BsonElement("city")]
        public string City { get; set; }
        [BsonElement("street")]
        public string Street { get; set; }
    }
}
