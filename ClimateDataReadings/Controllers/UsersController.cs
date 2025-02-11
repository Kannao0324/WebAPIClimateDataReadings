using ClimateDataReadings.AttributeTags;
using ClimateDataReadings.Models.DTOs;
using ClimateDataReadings.Models;
using ClimateDataReadings.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZstdSharp.Unsafe;
using MongoDB.Bson;

namespace ClimateDataReadings.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // Create a variable to hold a reference to our repository.
        private readonly IUserRepository _repository;
        // Create a constractor to request access to the repository 
        // from the dependency injection by naming it as a parameter in our constructor.
        public UsersController(IUserRepository repository)
        {
            _repository = repository;
        }


        //POST: api/Users/PostUser
        /// <summary>
        ///  For the user with TEACHER role, the user create a single user to be stored in the database
        /// </summary>
        /// 
        /// <remarks>
        /// The same email address cannot be used.
        /// Users need to fill in the required fields.
        /// To use this endpoint the user must have an API key which needs to be provided in the request headers
        /// </remarks>
        /// 
        /// <param name="userDto">Users can only see the specific feilds to fill in using userDTO</param>
        /// <returns>Inserts a new user in the database</returns>
        /// <response code= "201">Created</response>
        /// <response code= "400">Bad Request</response>
        /// <response code= "401">Unauthorised</response>
        /// <response code= "403">Access Denied</response>
        /// <response code= "500">Internal Server Error</response>
        [HttpPost("PostUser")]
        [ApiKey(Roles.TEACHER)]
        public ActionResult PostUser(UserDTO userDto)
        { 

            //Check the provided role in the DTO is valid and respond with an error code and message if it is not.
            if (!Enum.TryParse(userDto.Role.ToUpper(), out Roles userRoleType))
            {
                // 400 response code shown in a swagger.
                return BadRequest("Invalid user role provided.");
            }
            //Map the details from the dto and output of the enum check to our proper ApiUser model 
            //so it can be passed to the repository.
            var user = new ApiUser
            {
                UserName = userDto.UserName,
                ContactName = userDto.ContactName,
                Email = userDto.Email,
                Role = userRoleType.ToString()
            };
            //Pass the model to the repository to be saved and store the response
            var result = _repository.CreateUser(user);
            //If the result is false, meaning the user didn't save, return an error.
            if (result == false)
            {
                return BadRequest("Error. A user with this email already exists.");
            }
            //User input validation
            if (userDto == null)
            {
                return BadRequest("Fill in the all the details to be added.");
            }
            //Return a 201 response if successfully saved. 
            return CreatedAtAction("PostUser", user);

        }


        //PUT api/Users/UpdateUsersRoleByDate
        /// <summary>
        ///  Teachers is allowed to update the role of the multiple existing records in the certain time range
        /// </summary>
        /// 
        ///  <remarks>
        /// The start date cannot be after the end date and the date must not be the future date.
        /// To use this endpoint the user must have an API key which needs to be provided in the request headers
        /// </remarks>
        /// 
        /// <param name="start">start time representing the desired document in the database</param>
        /// <param name="end">end time representing the desired document in the database</param>
        /// <param name="requireRole">The role that is seleced by user (teacher) to update</param>
        /// <returns>Update the multiple records with role in the certain time range</returns>
        /// 
        /// <response code= "200">OK</response>
        /// <response code= "400">Bad Request</response>
        /// <response code= "401">Unauthorised</response>
        /// <response code= "403">Access Denied</response>
        /// <response code= "404">Not Found</response>
        /// <response code= "500">Internal Server Error</response>
        [HttpPut("UpdateUsersRoleByDate")]
        [ApiKey(Roles.TEACHER)]
        public ActionResult UpdateUsersRoleByDate(DateTime? start, DateTime? end, string requireRole)
        {
            // Check the user entry for the start date and the end date are null
            if (start == null || end == null)
            {
                return BadRequest("The start date and the end date cannot be blank or null.");
            }
                //Set a current time
                DateTime current = DateTime.UtcNow;
            //Check the provided role in the DTO is valid and respond with an error code and message if it is not.
            if (!Enum.TryParse(requireRole.ToUpper(), out Roles userRoleType))
            {
                // 400 response code shown in a swagger.
                return BadRequest("Invalid user role provided.");
            }
            // Check if the start date is after the end date
            if (start > end)
            {
                return BadRequest("The start date cannot be after the end date!");
            }
            // Check if the start date and the end date is the future date 
            if (start > current || end > current)
            {
                return BadRequest("The date cannot be the future!"); 
            }
           
            _repository.UpdateUsersRoleByDate(start, end, requireRole);
            return Ok();
        }


        //DELETE: api/Users/DeleteUser
        /// <summary>
        /// Delete an existing user from the database based upon the object id
        /// </summary>
        /// 
        /// <remarks>
        /// Ensure the id field is provided with a 24 charactor string representing a correct Mongo DB obejct id.
        /// the user must have an API key which needs to be provided in the request headers.
        /// </remarks>
        /// 
        /// <param name="id">id representing the particular record in the database</param>
        /// <returns>the specific user deleted from the database</returns>
        /// 
        /// <response code= "200">OK</response>
        /// <response code= "400">Bad Request</response>
        /// <response code= "401">Unauthorised</response>
        /// <response code= "403">Access Denied</response>
        /// <response code= "404">Not Found</response>
        /// <response code= "500">Internal Server Error</response>
        [HttpDelete("DeleteUser")]
        [ApiKey(Roles.TEACHER)]
        public ActionResult DeleteUser(string id)
        {
            try {
            //Validation check to make sure the string is the correct length for an ObjectId
            if (id.Length != 24)
            {
                return BadRequest("ID is invalid. It must be 24 characters long");
            }
            // Catch invalid ID
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("This ID is invalid.");
            }

            //Pass the request to the repository to be processed.
            _repository.DeleteUser(id);
             }
            //If the id doesn't exist, show 404 response
            catch(InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            return Ok();
        }

        //DELETE: api/Notes/DeleteStudentsByDate
        /// <summary>
        /// Delete multiple users with "STUDENT" role in the selected tiem range from the database 
        /// </summary>
        /// 
        /// <remarks>
        /// Ensure the time field must be in the past based on the last login.
        /// the user must have an API key which needs to be provided in the request headers
        /// </remarks>
        /// 
        /// <param name="start">The start time of the last login representing the desired document in the database</param>
        /// <param name="end">The end time of the last login representing the desired document in the database</param>
        /// <returns>The multiple users with "STUDENT" role deleted from the database.</returns>
        /// 
        /// <response code= "200">OK</response>
        /// <response code= "400">Bad Request</response>
        /// <response code= "401">Unauthorised</response>
        /// <response code= "403">Access Denied</response>
        /// <response code= "404">Not Found</response>
        /// <response code= "500">Internal Server Error</response>
        [HttpDelete("DeleteStudentsByDate")]
        [ApiKey(Roles.TEACHER)]
        public ActionResult DeleteStudentsByDate(DateTime? start, DateTime? end)
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
            // Check if the start date and the end date is the future date 
            if (start > current || end > current)
            {
                return BadRequest("The date cannot be in the future!");
            }
            
            //Pass the request and the required date time paramenters to the repository to be processed.
            _repository.DeleteStudentsByDate(start, end);
            return Ok();
        }
    }
}
