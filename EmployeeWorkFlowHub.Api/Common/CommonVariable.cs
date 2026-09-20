namespace EmployeeWorkFlowHub.Common
{
    /// <summary>
    /// Holds application-wide constants, parameters, default values, and session keys
    /// for enterprise workflow orchestration.
    /// </summary>
    public static class CommonVariable
    {
        /// <summary>
        /// Numeric default values for database action operations.
        /// </summary>
        public abstract class DefaultValues
        {
            public static int Zero = 0;
            public static int ONE = 1;
            public static int TWO = 2;
            public static int THREE = 3;
            public static int FOUR = 4;
            public static int FIVE = 5;
            public static int SIX = 6;
            public static int SEVEN = 7;
            public static int EIGHT = 8;
            public static int NINE = 9;
            public static int TEN = 10;
            public static string TRUE = "True";
            public static string FALSE = "False";
        }

        /// <summary>
        /// SQL Stored Procedure Action IDs mapped per entity domain.
        /// Eliminates magic numbers across all DataAccess layers.
        /// </summary>
        public static class ActionId
        {
            // Generic Action IDs
            public const int SelectAll = 1;
            public const int SelectById = 2;
            public const int Insert = 3;
            public const int Update = 4;
            public const int Delete = 5;

            // Department CRUD Actions (sp_Department_CRUD)
            public static class Department
            {
                public const int SelectAll = 1;
                public const int SelectById = 2;
                public const int Insert = 3;
                public const int Update = 4;
                public const int Delete = 5;
            }

            // Employee CRUD Actions (sp_Employee_CRUD)
            public static class Employee
            {
                public const int SelectAll = 1;
                public const int SelectById = 2;
                public const int Insert = 3;
                public const int Update = 4;
                public const int Delete = 5;
            }

            // Project CRUD Actions (sp_Project_CRUD)
            public static class Project
            {
                public const int SelectAll = 1;
                public const int SelectById = 2;
                public const int SelectByManager = 3;
                public const int Insert = 4;
                public const int Update = 5;
                public const int Delete = 6;
            }

            // Task CRUD Actions (sp_Task_CRUD)
            public static class Task
            {
                public const int SelectAll = 1;
                public const int SelectById = 2;
                public const int SelectByProjectLead = 3;
                public const int SelectByEmployee = 4;
                public const int SelectForQC = 5;
                public const int Insert = 6;
                public const int Update = 7;
                public const int Delete = 8;
            }

            // Lookup Master Actions (sp_Lookup_CRUD)
            public static class Lookup
            {
                public const int SelectAll = 1;
                public const int SelectByType = 2;
                public const int Insert = 3;
                public const int Update = 4;
                public const int Delete = 5;
            }
        }

        /// <summary>
        /// Stored procedure SQL parameter names.
        /// Prevents hardcoded parameter strings in ADO.NET SqlCommand instances.
        /// </summary>
        public static class Parameter
        {
            public const string ActionId = "@ActionId";
            public const string Id = "@Id";
            public const string Name = "@Name";
            public const string DepartmentId = "@DepartmentId";
            public const string ProjectManagerId = "@ProjectManagerId";
            public const string TeamMembers = "@TeamMembers";
            public const string Status = "@Status";
            public const string EmployeeCode = "@EmployeeCode";
            public const string FullName = "@FullName";
            public const string Email = "@Email";
            public const string Designation = "@Designation";
            public const string IsActive = "@IsActive";
            public const string Title = "@Title";
            public const string Description = "@Description";
            public const string ProjectId = "@ProjectId";
            public const string EmployeeId = "@EmployeeId";
            public const string Priority = "@Priority";
            public const string DueDate = "@DueDate";
            public const string StartDate = "@StartDate";
            public const string EndDate = "@EndDate";
            public const string Username = "@Username";
            public const string PasswordHash = "@PasswordHash";
            public const string Role = "@Role";
            public const string LookupType = "@LookupType";
            public const string LookupCode = "@LookupCode";
            public const string LookupValue = "@LookupValue";
            public const string DisplayOrder = "@DisplayOrder";
        }

        /// <summary>
        /// Application connection string configuration keys.
        /// </summary>
        public static class ConnectionString
        {
            public const string DefaultConnection = "DefaultConnection";
        }

        /// <summary>
        /// Session key names used across controllers and middleware.
        /// </summary>
        public static class SessionField
        {
            public const string Token = "Token";
            public const string UserName = "UserName";
            public const string FullName = "FullName";
            public const string Role = "Role";
            public const string EmployeeId = "EmployeeId";
        }

        /// <summary>
        /// System user role designations.
        /// </summary>
        public static class RoleName
        {
            public const string Manager = "Manager";
            public const string TeamLead = "Team Lead / Project Lead";
            public const string Developer = "Developer / Team Member";
            public const string QC = "Quality Analyst / QC";
            public const string Admin = "Admin";
        }
    }
}
