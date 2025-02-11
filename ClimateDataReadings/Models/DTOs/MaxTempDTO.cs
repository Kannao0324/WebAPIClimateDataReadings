using MongoDB.Bson.Serialization.Attributes;

namespace ClimateDataReadings.Models.DTOs
{
    public class MaxTempDTO
    {
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Device Name")]
        public string DeviceName { get; set; } = string.Empty;
       
        public DateTime Time { get; set; }

        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Temperature (°C)")]
        public double Temperature { get; set; }

    }
}
