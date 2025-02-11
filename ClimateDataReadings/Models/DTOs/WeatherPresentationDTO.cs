using MongoDB.Bson.Serialization.Attributes;

namespace ClimateDataReadings.Models.DTOs
{
    public class WeatherPresentationDTO
    {
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Device Name")]
        public string DeviceName { get; set; } = string.Empty;
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Precipitation mm/h")]
        public double Precipitation { get; set; }
        public DateTime Time { get; set; }
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Temperature (°C)")]
        public double Temperature { get; set; }
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Atmospheric Pressure(kPa)")]
        public double AtmosphericPressure { get; set; }
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Solar Radiation (W/m2)")]
        public double SolarRadiation { get; set; }

    }
}
