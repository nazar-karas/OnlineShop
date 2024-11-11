using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Documents
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string Id { get; set; }
        [BsonElement("firstName")]
        public string FirstName { get; set; }
        [BsonElement("lastName")]
        public string LastName { get; set; }
        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; }
        [BsonElement("isShipped")]
        public bool IsShipped { get; set; }
        [BsonElement("products")]
        public List<Product> Products { get; set; }
        [BsonElement("payment")]
        public Payment Payment { get; set; }

        #region References

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_carrierId")]
        public string CarrierId { get; set; }

        #endregion
    }
}
