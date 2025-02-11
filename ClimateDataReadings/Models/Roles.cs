using System.Runtime.Serialization;

namespace ClimateDataReadings.Models
{
    public enum Roles
    {
        [EnumMember(Value = "TEACHER")]
        TEACHER,
        [EnumMember(Value = "STUDENT")]
        STUDENT,
        [EnumMember(Value = "SENSOR")]
        SENSOR



    }
}
