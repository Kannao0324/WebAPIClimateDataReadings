using ClimateDataReadings.Models;
using ClimateDataReadings.Models.DTOs;

namespace ClimateDataReadings.Repositories
{
    public interface IWeatherDataRepository
    {
        // Basic CRUD Operations
        void PostReading(WeatherData data);
        void UpdatePrecipitation(string id, WeatherData data);

        // Bulk CRUD Operations
        void PostManyReadings(List<WeatherData> dataList);
        // Aggregation Operations
        WeatherPresentationDTO GetReadingsByDate(DateTime time, string sensor);
        List<MaxTempDTO> GetMaxTempByDateUsingLINQAggregation(DateTime? start, DateTime? end);
        MaxPCPNDataDTO GetMaxRainByDateUsingLINQAggregation(string sensor);

    }
}
