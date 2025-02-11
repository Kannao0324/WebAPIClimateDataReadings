using ClimateDataReadings.Services;
using MongoDB.Driver;
using MongoDB.Bson;
using ClimateDataReadings.Models;
using ClimateDataReadings.Models.DTOs;
using System.Linq;
using System.Collections.Immutable;

namespace ClimateDataReadings.Repositories
{
    public class WeatherDataRepository: IWeatherDataRepository
    {
        //Variable to store a reference to our MongoDB collection.
        private readonly IMongoCollection<WeatherData> _weather;
        //Request the connection builder
        //from the dependency injection by specifying it in the constructor.
        public WeatherDataRepository(MongoConnectionBuilder connection)
        {
            //Use the connection builder object to connect to the database and to the notes collection.
            _weather = connection.GetDatabase().GetCollection<WeatherData>("WeatherData");
        }

        public MaxPCPNDataDTO GetMaxRainByDateUsingLINQAggregation(string sensor)
        {
            // Calculate the date in the last 5 months from the current date
            DateTime end = DateTime.UtcNow;
            DateTime start = end.AddMonths(-5);
            
            //Access the collection as a queryable object that can have LINQ commands applied to it.
            var collection = _weather.AsQueryable();
            
            //Use a select statemnet to map the objects to DTOs and then pass them all into a result.
            var result = collection.Where(w => w.DeviceName == sensor &&
                                               w.Time >= start && 
                                               w.Time <= end )
                                   .OrderByDescending(w => w.Precipitation)
                                   .GroupBy(w => w.DeviceName)
                                   .Select(w => new MaxPCPNDataDTO
                                   {
                                       DeviceName = w.First().DeviceName,
                                       Precipitation = w.First().Precipitation,
                                       Time = w.First().Time
                                   }).FirstOrDefault();
            return result;
        }

        public List<MaxTempDTO> GetMaxTempByDateUsingLINQAggregation(DateTime? start, DateTime? end)
        {
            //Access the collection as a queryable object that can have LINQ commands applied to it.
            var collection = _weather.AsQueryable();
            //Use a select statemnet to map the objects to DTOs and then pass them all into a list.
            var result = collection.Where(w => w.Time >= start && w.Time <= end)
                                   .OrderByDescending(w => w.Temperature)
                                   .GroupBy(w => w.DeviceName)
                                   .Select(g => new MaxTempDTO
                                   {
                                       DeviceName = g.First().DeviceName,
                                       Time = g.First().Time,
                                       Temperature = g.First().Temperature
                                   })
                                   .ToList();
            return result;
        }

        public WeatherPresentationDTO GetReadingsByDate(DateTime time, string sensor)
        {
            
            // Add 5min buffer for start and end date
            DateTime start = time.AddMinutes(-5);
            DateTime end = time.AddMinutes(5);

            
            //Access the collection as a queryable object that can have LINQ commands applied to it.
            //The commands will not execute until the ToList is called.
            var collection = _weather.AsQueryable();
            //Use a select statemnet to map the objects to DTOs and then pass them all into a list.
            var result = collection.Where(w => w.DeviceName == sensor &&
                                               w.Time >= start &&
                                               w.Time <= end)
                                   .Select(w => new WeatherPresentationDTO
                                   {
                                       DeviceName = w.DeviceName,
                                       Precipitation = w.Precipitation,
                                       Time = w.Time,
                                       Temperature = w.Temperature,
                                       AtmosphericPressure = w.AtmosphericPressure,
                                       SolarRadiation = w.SolarRadiation
                                   }).FirstOrDefault();
            return result;
        }

        public void PostManyReadings(List<WeatherData> dataList)
        {
            //Pass the data list to the database to be saved.
            _weather.InsertManyAsync(dataList);
        }

        public void PostReading(WeatherData data)
        {
            //Pass the single data to the database to be saved.
            _weather.InsertOneAsync(data);
        }

        public void UpdatePrecipitation(string id, WeatherData data)
        {
            // Convert the string version of our Id back on its original objectId format.
            ObjectId objectId = ObjectId.Parse(id);
            // Create a filter to match the object Id against the _id in the collection
            var filter = Builders<WeatherData>.Filter.Eq(n => n._id, objectId);
            // Create a builder to generate our update rules.
            var builder = Builders<WeatherData>.Update;
            // Create a set of rules for what fields need to be updated
            // and what they need to be set to.
            var update = builder.Set(w => w.Precipitation, data.Precipitation);

            // Pass the filter and update rules to the database to be processed.
            var result = _weather.UpdateOne(filter, update);
            // If the data doesn't exist, show an error
            if (result.ModifiedCount == 0)
            {
                throw new InvalidOperationException("The data is not existed.");
;           }
        }
    }
}

