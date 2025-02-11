using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace ClimateDataReadings.Models
{
    public class ApiUser
    {
        [BsonId]
        [JsonIgnore]
        public ObjectId _id { get; set; }
        public string objId => _id.ToString();
        [BsonElement("Username")]
        public string UserName { get; set; } = string.Empty;
        [BsonElement("Contact Name")]
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = Roles.STUDENT.ToString();
        public string ApiKey { get; set; } = string.Empty;
        [BsonElement("Created Date")]
        public DateTime Created { get; set; }
        [BsonElement("Last Access")]
        public DateTime LastAccess { get; set; }

       
    }
}
