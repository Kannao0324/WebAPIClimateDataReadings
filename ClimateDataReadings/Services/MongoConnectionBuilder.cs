using ClimateDataReadings.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ClimateDataReadings.Services
{
    public class MongoConnectionBuilder
    {
        //Variable to hold a reference to our settings class once we request if from our dependency injection
        private readonly IOptions<MongoConnectionSettings> _settings;
        //Request our settings class by putting is as a parameter in the constructor.
        //This will automatically ask for it to be provided by the dependency injection as this class is built.
        public MongoConnectionBuilder(IOptions<MongoConnectionSettings> settings)
        {
            //Store the recieved item into our variable.
            _settings = settings;
        }

        public IMongoDatabase GetDatabase()
        {
            //Create the class for connecting with Mongo db and hand it our connection string.
            var client = new MongoClient(_settings.Value.ConnectionString);
            //Once the class is created, ask it to connect directly to our database.
            return client.GetDatabase(_settings.Value.DatabaseName);
        }
    }
}
