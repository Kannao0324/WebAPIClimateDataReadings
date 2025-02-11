using ClimateDataReadings.Models;
using ClimateDataReadings.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ClimateDataReadings.AttributeTags
{
    [AttributeUsage(validOn: AttributeTargets.Method | AttributeTargets.Class)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        //Variable to store the array the allowed roles to be passed to the filter attribute
        public Roles[] AllowedRoles { get; set; }
        //Constructor which allows the allowed roles to be passed to the filter attribute.
        public ApiKeyAttribute(params Roles[] roles)
        {
            AllowedRoles = roles;
        }
        //Method that runs the filter and passes the request onto the next item in the path
        //which will either be a controller or endpoint. The context parameter is the request
        //data being sent through the system, the next parameter is a reference to the next item 
        //in the path.
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //Try to tretrieve the apikey from the header data,
            //if it cannot be found the if statement will run.
            if (!context.HttpContext.Request.Headers.TryGetValue("apiKey", out var key))
            {
                //Create a class to set a response code and message
                //and put it into the result/response section of the request.
                context.Result = new ContentResult
                {
                    StatusCode = 401,
                    Content = "No API key provided with request!"
                };

                //Return our of the method, which will cause the request to procedd no futher
                //and start excuting the response actions of the api.turn;
                return;
            }
            //Convert the key from a String values data type to a standard string
            //and trim the leading and trailing curly braces. 
            var validkey = key.ToString().Trim('{', '}');
            //Request our UserRepository class using an alternate method to our constructor request.
            var userRepo = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            //Pass the apikey and allowed roles to the authenticate method to see if the user's 
            //credentials are correct and are allowed to perform their desired request

            if (userRepo.AuthenticateUser(validkey, AllowedRoles) == false)
            {
                //Create a class to set a response code and message
                //and put it into the result/response section of the request.
                context.Result = new ContentResult
                {
                    StatusCode = 403,
                    Content = "The provided API key is invalid or does not have the required permissions!"
                };
                //Return our of the method, which will cause the request to procedd no futher
                //and start excuting the response actions of the api.
                return;
            }
            //Update the users details to change their last access date
            userRepo.UpdateLastLogin(validkey);
            //Trigger the next variable which will run the next item in the route path.
            await next();
        }
    }
}
