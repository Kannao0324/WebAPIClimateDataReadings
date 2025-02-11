using ClimateDataReadings.AttributeTags;
using ClimateDataReadings.Models;
using ClimateDataReadings.Models.DTOs;
using ClimateDataReadings.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ClimateDataReadings.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherDataController : ControllerBase
    {
        // Create a variable to hold a reference to an IWeatherDataRepository object.
        private readonly IWeatherDataRepository _repository;
        // Create a constractor to request the class that implements the IWeatherRepository Interface
        // from the dependency injection. 
        public WeatherDataController(IWeatherDataRepository repository)
        {
            _repository = repository;
        }

        //GET: api/WeatherData/GetReadingByDate
        /// <summary>
        /// Get a weather reading record by a specific station at a given date and time stored in the Mongo DB and returns it 
        /// </summary>
        /// 
        /// <remarks>
        /// To use this endpoint the user must have an API key which needs to be provided in the request headers.
        /// The date must not be the future date. 
        /// </remarks>
        /// 
        /// <param name="time">A Mongo DB time range narrowing down the result representing the desired document in the database</param>
        /// <param name="sensor">A Mongo DB device name narrowing down the result representing the desired document in the database</param>
        /// <returns>Returns the temperature, atmospheric pressure, radiation, and precipitation value representing database records.</returns>
        /// <response code= "200">OK</response>
        /// <response code= "400">Bad Request</response>
        /// <response code= "401">Unauthorised</response>
        /// <response code= "403">Access Denied</response>
        /// <response code= "404">Not Found</response>
        /// <response code= "500">Internal Server Error</response>
        [HttpGet("GetReadingByDate")]
        [ApiKey(Roles.TEACHER, Roles.STUDENT)]
        public ActionResult<WeatherPresentationDTO> GetReadingByDate(DateTime time, string sensor)
        {
            // Check if the time date is not null or blank 
            if (time == DateTime.MinValue)
            {
                return BadRequest("The start date and the end date cannot be blank or null.");
            }
            DateTime current = DateTime.UtcNow;
            // Check if time is not in the future date 
            if (time > current)
            {
                return BadRequest("The date cannot be the future!");
            }

            var reading = _repository.GetReadingsByDate(time,sensor);

            //Check if the query resulted in any mathces.               
            if (reading == null )
            {
                return NotFound("No record found");
            }
            return Ok(reading);
        }

        //GET: api/WeatherData/GetMaxTempByDate
        /// <summary>
        /// Get the data that shows max temperatures for all stations for a given Date / Time range stored in the Mongo DB and returns it 
        /// </summary>
        /// 
        /// <remarks>
        /// To use this endpoint the user must have an API key which needs to be provided in the request headers.
        /// The start date cannot be after the end date and the start and end date must not the future date.
        /// </remarks>
        /// 
        /// <param name="start">A Mongo DB time range narrowing down the result representing the desired document in the database</param>
        /// <param name="end">A Mongo DB time range narrowing down the result representing the desired document in the database</param>
        /// <returns>Returns the sensor name, reading date / time and the precipitation value representing database records.</returns>
        ///  <response code= "200">OK</response>
        /// <response code= "400">Bad Request</response>
        /// <response code= "401">Unauthorised</response>
        /// <response code= "403">Access Denied</response>
        /// <response code= "404">Not Found</response>
        /// <response code= "500">Internal Server Error</response>
        
        [HttpGet("GetMaxTempByDate")]
        [ApiKey(Roles.TEACHER, Roles.STUDENT)]
        public ActionResult<List<MaxTempDTO>> GetMaxTempByDateUsingLINQAggregation(DateTime? start, DateTime? end)
        {
            // Check the user entry for the start date and the end date are null
            if (start == null || end == null)
            {
                return BadRequest("The start date and the end date cannot be blank or null.");
            }
            DateTime current = DateTime.UtcNow;
            // Check if the start date is after the end date
            if (start > end)
            {
                return BadRequest("The start date cannot be after the end date!");
            }
            // Check if the start date and the end date is in the future date 
            if (start > current || end > current)
            {
                return BadRequest("The date cannot be the future!");
            }

            var readings = _repository.GetMaxTempByDateUsingLINQAggregation(start,end);

            //Check if the query resulted in any mathces.               
            if (readings == null || !readings.Any())
            {
                return NotFound("No record found");
            }

            return Ok(readings);
        }

        //GET: api/WeatherData/GetMaxRainByDate
        /// <summary>
        /// Get the data that shows max precipitation in the last 5 Months for a specific sensor stored in the Mongo DB and returns it 
        /// </summary>
        /// 
        /// <remarks>
        /// To use this endpoint the user must have an API key which needs to be provided in the request headers
        /// </remarks>
        /// 
        /// <param name="sensor">A mongo DB devivce name representing the desired document in the database </param>
        /// <returns>Returns the sensor name, reading date / time and the maxiumum precipitation value representing database records.</returns>
        ///  <response code= "200">OK</response>
        /// <response code= "401">Unauthorised</response>
        /// <response code= "403">Access Denied</response>
        /// <response code= "404">Not Found</response>
        /// <response code= "500">Internal Server Error</response>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client,
            VaryByQueryKeys = new string[] {"sensor"})]
        [HttpGet("GetMaxRainByDate")]
        [ApiKey(Roles.TEACHER, Roles.STUDENT)]
        public ActionResult<MaxPCPNDataDTO> GetMaxRainByDateUsingLINQAggregation(string sensor)
        {
            var reading = _repository.GetMaxRainByDateUsingLINQAggregation(sensor);

            //Check if the query resulted in any mathces.               
            if (reading == null)
            {
                return NotFound("No record found");
            }

            return Ok(reading);
        }



        //POST: api/WeatherData/PostWeatherData
        /// <summary>
        ///  The sensor adds a single weather reading into the Weather data collection. 
        /// </summary>
        /// 
        /// <remarks>
        /// To use this endpoint the user must have an API key which needs to be provided in the request headers
        /// </remarks>
        /// 
        /// <param name="dataDto">dataDto stores the weather date except of _id and objectId that users enter</param>
        /// <returns>Insert the new weather reading in the WeatherData collection in the database.</returns>
        /// <response code= "201">Created</response>
        /// <response code= "400">Bad Request</response>
        /// <response code= "401">Unauthorised</response>
        /// <response code= "403">Access Denied</response>
        /// <response code= "500">Internal Server Error</response>
        [HttpPost("PostWeatherData")]
        [ApiKey(Roles.SENSOR)]
        public ActionResult PostWeatherData([FromBody] WeatherDataDTO dataDto)
        {
            
            var data = new WeatherData
            {
                DeviceName = dataDto.DeviceName,
                Precipitation = dataDto.Precipitation,
                Time = DateTime.UtcNow,
                Latitude = dataDto.Latitude,
                Longitude = dataDto.Longitude,
                Temperature = dataDto.Temperature,
                AtmosphericPressure = dataDto.AtmosphericPressure,
                MaxWindSpeed = dataDto.MaxWindSpeed,
                SolarRadiation = dataDto.SolarRadiation,
                VaporPressure = dataDto.VaporPressure,
                Humidity = dataDto.Humidity,
                WindDirection = dataDto.WindDirection
            };
            //Pass the request on to the repository for processing.
            _repository.PostReading(data);
            // User input validation
            if (dataDto == null)
            {
                return BadRequest("Provide all the values required.");
            }
            //Return the 201 response to the caller. The 1st parameter here
            //Should be your method name. The 2nd should be your created object.
            return CreatedAtAction("PostWeatherData", data);
        }

        //POST: api/WeatherData/PostMany
        /// <summary>
        /// To allow the sensor to record multiple readings through this endpoint before storing in the database
        /// </summary>
        /// 
        /// <remarks>
        /// To use this endpoint the user must have an API key which needs to be provided in the request headers
        /// </remarks>
        /// 
        /// <param name="dataDTOList">DTO List prevents the sensor from recording obejtct id</param>
        /// <returns>Insert multiple records in the database</returns>
        /// <response code= "201">Created</response>
        /// <response code= "400">Bad Request</response>
        /// <response code= "401">Unauthorised</response>
        /// <response code= "403">Access Denied</response>
        /// <response code= "500">Internal Server Error</response>
        [HttpPost("PostMany")]
        [ApiKey(Roles.SENSOR)]
        public ActionResult PostMany([FromBody] List<WeatherDataDTO> dataDTOList)
        {
            if (dataDTOList.Count < 1)
            {
                return BadRequest("No items in provided list");
            }

            //Pass the DTO list through a LINQ select query which will create a new note
            //for each item and put them into a new list
            var dataList = dataDTOList.Select(w => new WeatherData
            {
                DeviceName = w.DeviceName,
                Precipitation = w.Precipitation,
                Time = DateTime.UtcNow,
                Latitude = w.Latitude,
                Longitude = w.Longitude,
                Temperature = w.Temperature,
                AtmosphericPressure =  w.AtmosphericPressure,
                MaxWindSpeed = w.MaxWindSpeed,
                SolarRadiation = w.SolarRadiation,
                VaporPressure = w.VaporPressure,
                Humidity = w.Humidity,
                WindDirection = w.WindDirection
            }).ToList();
            //Pass the list to the repository to be saved
            _repository.PostManyReadings(dataList);
            return CreatedAtAction("PostMany", dataList);
        }

        //Patch: api/WeatherData/UpdatePrecipitation
        /// <summary>
        /// To allow TEACHER users to correct errors of precipitation in the dataset
        /// </summary>
        /// 
        /// <remarks>
        /// To use this endpoint the user must have an API key which needs to be provided in the request headers.
        /// Object ID must be 24 characters long.
        /// </remarks>
        /// 
        /// <param name="id">A Mongo DB obeject _id representing the desired document in the database</param>
        /// <param name="dataDto">DTO allows users to only update precipitation</param>
        /// <returns>Update a specified existing precipitation value to a specific value</returns>
        /// <response code= "200">OK</response>
        /// <response code= "400">Bad Request</response>
        /// <response code= "401">Unauthorised</response>
        /// <response code= "403">Access Denied</response>
        /// <response code= "404">Not Found</response>
        /// <response code= "500">Internal Server Error</response>
        [HttpPatch("UpdatePrecipitation")]
        //[ApiKey(Roles.TEACHER)]
        public ActionResult UpdatePrecipitation(string id, WeatherDataDTO dataDto)
        {
            try
            {
                //Validation check to make sure the string is the correct length for an ObjectId
                if (id.Length != 24)
                {
                    return BadRequest("ID is invalid. It must be 24 characters long");
                }

                var data = new WeatherData
                {
                    Precipitation = dataDto.Precipitation
                };


                //Pass the details of the request to the repository to be processed.
                _repository.UpdatePrecipitation(id, data);
            }
            // If the id doesn't exist, show 404 response
            catch(InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            


            return Ok();
        }
    }
}
