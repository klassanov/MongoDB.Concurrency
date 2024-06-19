using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OrderStatusApp.DbModels
{
    public class Order
    {
        public ObjectId Id { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }

        [BsonElement("isProcessing")]
        public bool IsProcessing { get; set; }

        [BsonElement("orderId")]
        public string OrderId { get; set; }

        [ConcurrencyCheck]
        [BsonElement("version")]
        public int Version { get; set; }
    }
}
