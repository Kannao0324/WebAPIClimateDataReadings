using MongoDB.Bson.Serialization.Attributes;

namespace ClimateDataReadings.Models.DTOs
{
    // The fileds that users can add or update in the DTO class.
    public class WeatherDataDTO
    {
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Device Name")]
        public string DeviceName { get; set; } = string.Empty;
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Precipitation mm/h")]
        public double Precipitation { get; set; }
        public DateTime Time { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Temperature (°C)")]
        public double Temperature { get; set; }
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Atmospheric Pressure(kPa)")]
        public double AtmosphericPressure { get; set; }
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Max Wind Speed (m/s)")]
        public double MaxWindSpeed { get; set; }
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Solar Radiation (W/m2)")]
        public double SolarRadiation { get; set; }
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Vapor Pressure (kPa)")]
        public double VaporPressure { get; set; }
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Humidity (%)")]
        public double Humidity { get; set; }
        // Maps the property below to the field in the database that matches the name in the BsonElement tag.
        [BsonElement("Wind Direction (°)")]
        public double WindDirection { get; set; }

    }
}
