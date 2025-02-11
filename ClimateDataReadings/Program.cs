
using ClimateDataReadings.Services;
using ClimateDataReadings.Settings;
using MongoDB.Driver;
using ClimateDataReadings.Repositories;
using Microsoft.OpenApi.Models;
using SharpCompress.Common;

namespace ClimateDataReadings
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            //Finds the filepath of the directory the program is stored in and combine it
            //with the file name of the XML file.
            var filepath = Path.Combine(AppContext.BaseDirectory, "ClimateDataReadings.xml");

            builder.Services.AddSwaggerGen(options =>
            {
                //Adds some customised heading details to your swagger UI.
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "My WeatherData API", Version = "v1" });
                //Tells the Swagger UI to read import the XML comments from the XML file we specified earlier.
                options.IncludeXmlComments(filepath);

                //Create UI for authentication in a swagger to input API key 
                options.AddSecurityDefinition("apiKey", new OpenApiSecurityScheme
                {
                    Description = "Enter your API key here to manage user access.",
                    Name = "apiKey",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "apiKey"
                            },
                            Name = "apiKey",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });

            //Add CORS to the API to manage allowed interactions from domains outside the API
            builder.Services.AddCors(options =>
            {
                //Create a policy that defines the rules for any CORS interactions
                options.AddPolicy("Google", p =>
                {
                    p.WithOrigins("https://www.google.com", "https://www.google.com.au");
                    p.AllowAnyHeader();
                    p.WithMethods("GET", "POST", "PUT", "DELETE", "PATCH");
                });

            });
            //Adds response caching to our system
            builder.Services.AddResponseCaching();

            //Map our connection string settings into the settings class and
            //add it to the dependency injection for fast retrieval when it is needed.
            builder.Services.Configure<MongoConnectionSettings>(builder.Configuration.GetSection("ConnString"));
            //Add any required classes to the Dependency Injection system that need to be shared by the system
            builder.Services.AddScoped<MongoConnectionBuilder>();
            builder.Services.AddScoped<IWeatherDataRepository, WeatherDataRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();



            // builder.Services.AddScoped<INoteRepository, MongoNoteRepository>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
            //Tell our system to use the response caching service we added above.
            app.UseResponseCaching();
            //Tell our system to use CORS on Google website
            app.UseCors("Google");

            app.MapControllers();

            app.Run();
        }
    }
}
