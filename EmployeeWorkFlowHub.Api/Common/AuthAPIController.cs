namespace EmployeeWorkFlowHub.Common
{
    /// <summary>
    /// Shared constants for API controllers, for the application architecture.
    /// Use these in controller-level attributes instead of hardcoded strings.
    /// </summary>
    public static class AuthAPIController
    {
        /// <summary>
        /// HTTP content-type constants for [Produces] attribute on API controllers.
        /// </summary>
        public static class InputType
        {
            /// <summary>application/json content type for all API responses.</summary>
            public const string ApplicationJson = "application/json";
        }

        /// <summary>
        /// Route template constants for [Route] attribute on API controllers.
        /// </summary>
        public static class Property
        {
            /// <summary>Default REST route: api/[controller] (e.g. api/task, api/project).</summary>
            public const string APIController = "api/[controller]";
        }
    }
}
