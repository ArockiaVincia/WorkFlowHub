namespace EmployeeWorkFlowHub.Models
{
    /// <summary>
    /// Represents a dynamic support data / master lookup item.
    /// Exposes standard fields: id, code, value, type.
    /// </summary>
    public class SupportDataItem
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
