using System;
using System.IO;

namespace EmployeeWorkFlowHub.Common.Helper
{
    /// <summary>
    /// File-based error and trace logging helper for the application architecture.
    /// Writes timestamped log files organized by Year and Month under ErrorLog directory.
    /// </summary>
    public class ErrorLog : IDisposable
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrorLog", DateTime.Today.Year.ToString(), DateTime.Today.Month.ToString());
        private readonly string _logFileName;

        public ErrorLog()
        {
            try
            {
                if (!Directory.Exists(LogFilePath))
                {
                    Directory.CreateDirectory(LogFilePath);
                }
            }
            catch
            {
                // Fallback directory creation safety
            }
            _logFileName = Path.Combine(LogFilePath, $"Log_{DateTime.Today:yyyy_MM_dd}.log");
        }

        /// <summary>
        /// Writes custom text message to daily error log file.
        /// </summary>
        public void WriteLog(string message)
        {
            try
            {
                using var sw = File.AppendText(_logFileName);
                sw.WriteLine($"Date/Time : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sw.WriteLine($"Message   : {message}");
                sw.WriteLine("-----------------------------------------------------------------------------------------");
            }
            catch
            {
                // Suppress file I/O exceptions in logging to avoid breaking call stack
            }
        }

        /// <summary>
        /// Writes structured exception details (source, message, stack trace) to daily error log file.
        /// </summary>
        public void WriteLog(Exception ex)
        {
            try
            {
                using var sw = File.AppendText(_logFileName);
                sw.WriteLine("-----------------------------------------------------------------------------------------");
                sw.WriteLine($"Date/Time  : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sw.WriteLine($"Source/MSG : {ex.Source} / {ex.Message}");
                sw.WriteLine($"StackTrace : {ex.StackTrace}");
                sw.WriteLine("-----------------------------------------------------------------------------------------");
            }
            catch
            {
                // Suppress file I/O exceptions in logging to avoid breaking call stack
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
