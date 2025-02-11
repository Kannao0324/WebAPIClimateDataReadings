using MongoDB.Bson.Serialization.Attributes;

namespace ClimateDataReadings.Models.DTOs
{
    public class UserDTO
    {
        [BsonElement("Username")]
        public string UserName { get; set; } = string.Empty;
        [BsonElement("Contact Name")]
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = Roles.STUDENT.ToString();
       

    }
}
