using ClimateDataReadings.Models;
using ClimateDataReadings.Models.DTOs;

namespace ClimateDataReadings.Repositories
{
    public interface IUserRepository
    {
        // Basic CRUD Operations
        bool CreateUser(ApiUser user);
        void DeleteUser(string id);
        // Bulk CRUD Operations
        void UpdateUsersRoleByDate(DateTime? start, DateTime? end, string requireRole);
        void DeleteStudentsByDate(DateTime? start, DateTime? end);

        bool AuthenticateUser(string apikey, params Roles[] requireRoles);
        void UpdateLastLogin(string apikey);
    }
}
