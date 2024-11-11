using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Domain.Documents
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string Id { get; set; }
        [BsonElement("barcode")]
        public string? Barcode { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("price")]
        public double Price { get; set; }
        [BsonElement("quantity")]
        public int Quantity { get; set; }
    }
}
