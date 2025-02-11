using MongoDB.Bson.Serialization.Attributes;

namespace ClimateDataReadings.Models.DTOs
{
    public class MaxPCPNDataDTO
    {
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Device Name")]
        public string DeviceName { get; set; } = string.Empty;
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Precipitation mm/h")]
        public double Precipitation { get; set; }

        public DateTime Time { get; set; }

       
        
    }
}
