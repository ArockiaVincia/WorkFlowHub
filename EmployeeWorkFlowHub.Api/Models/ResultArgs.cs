namespace EmployeeWorkFlowHub.Models
{
    /// <summary>
    /// Standard API response envelope for the application architecture.
    /// Encapsulates status code, descriptive message, title, and payload data.
    /// </summary>
    public class ResultArgs
    {
        public long StatusCode { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public string MessageTitle { get; set; } = string.Empty;
        public object? ResultData { get; set; }
    }

    /// <summary>
    /// Strongly-typed API response envelope for the application architecture.
    /// </summary>
    /// <typeparam name="T">Payload entity or list type.</typeparam>
    public class ResultArgs<T>
    {
        public long StatusCode { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public string MessageTitle { get; set; } = string.Empty;
        public T? ResultData { get; set; }
    }
}
