using Amazon.Runtime.Internal.Transform;
using ClimateDataReadings.Models;
using ClimateDataReadings.Models.DTOs;
using ClimateDataReadings.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IO.IsolatedStorage;

namespace ClimateDataReadings.Repositories
{
    public class UserRepository : IUserRepository
    {
        //Create a readonly field for storing a reference to the connection to our database collection.
        private readonly IMongoCollection<ApiUser> _users;
        //Request access to the Mongo Connection builder class
        //by naming it as a parameter in our constructor.
        public UserRepository(MongoConnectionBuilder connection)
        {
            //Use the provided connection builder to get access to our collection
            //and set it up to map to the ApiUser model.
            _users = connection.GetDatabase().GetCollection<ApiUser>("ApiUsers");
        }

        public bool AuthenticateUser(string apikey, params Roles[] requireRoles)
        {
            //Create a filter to check each user's apikey against the key provided in the parameters.
            var filter = Builders<ApiUser>.Filter.Eq(u => u.ApiKey, apikey);
            //Pass the filter to the find method to find the matching user and store the result.
            var user = _users.Find(filter).FirstOrDefault();
            //If no user was found, return false to indicate an error
            if (user == null)
            {
                return false;
            }
            //Run the method to check the user's credetials against the requried roles
            //for the endpoint they are tring to run.
            if (IsAllowedRole(user.Role, requireRoles) == false)
            {
                // If the user fails the check, return false to indicate an error
                return false;
            }
            //If the user details pass all the checks, return true.
            return true;
        }


        public bool CreateUser(ApiUser user)
        {
            //Create a filter to check the provided user email against all the documents in the collection
            var filter = Builders<ApiUser>.Filter.Eq(u => u.Email, user.Email);
            //Pass the filter to the database with a find command and store the result if a match is found
            var existingUser = _users.Find(filter).FirstOrDefault();
            //If the user already has an account, return false to indicate this.
            if (existingUser != null)
            {
                return false;
            }

            //Generate a unique 36 charactor string to be used as the ApiKey.
            //A Guid is a global unique identifier which is considered to have an alomost
            //impossible chance of being generated twice.
            user.ApiKey = Guid.NewGuid().ToString();
            //Set the date time fields using the system clock.
            user.Created = DateTime.UtcNow;
            user.LastAccess = DateTime.UtcNow;
            //Pass the user details to the database to be saved then return true.
            _users.InsertOne(user);
            return true;
        }

        public void DeleteStudentsByDate(DateTime? start, DateTime? end)
        {
                // Create a filter builder for building our filter rules
                var builder = Builders<ApiUser>.Filter;
                // Set a greater and less than or equal to filters to check the created date
                // against our provided start and end dates and the roles to match only student.
                var filter = builder.Gte(u => u.LastAccess, start) &
                             builder.Lte(u => u.LastAccess, end) &
                             builder.Eq(u => u.Role, Roles.STUDENT.ToString());

                //Pass the filter to the find method to find the matching user and store the result.
                var users = _users.Find(filter).ToList();

                _users.DeleteMany(filter);
            
        }

        public void DeleteUser(string id)
        {
            
                // Convert the string version of our Id back on its original objectId format.
                ObjectId objectId = ObjectId.Parse(id);
                // Create a filter to match the object Id against the _id in the collection
                var filter = Builders<ApiUser>.Filter.Eq(n => n._id, objectId);

                //Pass the filter to the find method to find the matching user and store the result.
                var existingUser = _users.Find(filter).FirstOrDefault();
                ////Pass the filter to Mongo and tell it to delte the record.
                if (existingUser != null)
                {
                    _users.DeleteOne(filter);
                }
                else
                {
                // If the id doesn't exist, show an error
                   throw new InvalidOperationException("The ID is not existed.");
                }

        }

        public void UpdateLastLogin(string apikey)
        {
            //Get the current date time so we can pass it to the database
            var currentDate = DateTime.UtcNow;
            //Create a filter to find the user entry that mathces the provided ApiKey.
            var filter = Builders<ApiUser>.Filter.Eq(u => u.ApiKey, apikey);
            //Create an update rule to change the user's last access filed to the current date.
            var update = Builders<ApiUser>.Update.Set(u => u.LastAccess, currentDate);
            
            //Pass the filter and update rule to the database to be processed.
            _users.UpdateOne(filter, update);
        }

        public void UpdateUsersRoleByDate(DateTime? start, DateTime? end, string requireRole)
        {
            // Create a filter builder for building our filter rules
            var builder = Builders<ApiUser>.Filter;
            // Set a greater and less than or equal to filters to check the created date
            // against our provided start and end dates.
            var filter = builder.Gte(u => u.Created, start) &
                         builder.Lte(u => u.Created, end);
            // Pass the valid role selected 
            var update = Builders<ApiUser>.Update.Set(u => u.Role, requireRole.ToString());
            //Pass the filter and update rule to the database to be processed.
            _users.UpdateMany(filter,update);
        }


        // The method to validate the role of users to still activated or accurate
        private bool IsAllowedRole(string userRole, Roles[] allowedRoles)
        {
            //Use an if statement to run the tryparse on our enum to see if
            //the provied user role matches one of our pre-defined options.
            if (!Enum.TryParse(userRole, out Roles userRoleType))
            {
                //if not, return false to indicate a failure
                return false;
            }
            //Cycle through all the roles in our array and compare each one against the proided user role.
            foreach (var role in allowedRoles)
            {
                //If a match is found, return true to indicate success.
                if (userRoleType.Equals(role))
                {
                    return true;
                }
            }
            //if not, return false to indicate a failure
            return false;
        }
    }
}
