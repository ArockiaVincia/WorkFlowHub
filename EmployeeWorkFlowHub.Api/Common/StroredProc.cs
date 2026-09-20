namespace EmployeeWorkFlowHub.Common
{
    /// <summary>
    /// Holds stored procedure names as constants, Constants for database stored procedure names.
    /// Eliminates hardcoded stored procedure names across ADO.NET DataAccess methods.
    /// </summary>
    public static class StroredProc
    {
        public static class Department
        {
            public const string CRUD = "sp_Department_CRUD";
        }

        public static class Employee
        {
            public const string CRUD = "sp_Employee_CRUD";
        }

        public static class Project
        {
            public const string CRUD = "sp_Project_CRUD";
        }

        public static class Task
        {
            public const string CRUD = "sp_Task_CRUD";
        }

        public static class User
        {
            public const string Authenticate = "sp_User_Authenticate";
        }

        public static class Lookup
        {
            public const string CRUD = "sp_Lookup_CRUD";
        }
    }
}
