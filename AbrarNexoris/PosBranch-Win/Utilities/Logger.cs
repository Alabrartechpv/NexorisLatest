using System;
using System.IO;

namespace PosBranch_Win.Utilities
{
    public static class Logger
    {
        private const string LogDirectory = @"C:\Nexoris\Logs";
        private const string LogFileName = "error_log.txt";
        private static readonly object LockObj = new object();

        /// <summary>
        /// Logs an error message and optional exception details to a persistent file.
        /// </summary>
        public static void LogError(string message, Exception ex = null)
        {
            try
            {
                lock (LockObj)
                {
                    if (!Directory.Exists(LogDirectory))
                    {
                        Directory.CreateDirectory(LogDirectory);
                    }

                    string logFilePath = Path.Combine(LogDirectory, LogFileName);
                    using (StreamWriter writer = new StreamWriter(logFilePath, true))
                    {
                        writer.WriteLine($"==================== {DateTime.Now:yyyy-MM-dd HH:mm:ss} ====================");
                        writer.WriteLine($"Message: {message}");
                        if (ex != null)
                        {
                            writer.WriteLine($"Exception Type: {ex.GetType().FullName}");
                            writer.WriteLine($"Exception Message: {ex.Message}");
                            writer.WriteLine($"Stack Trace:");
                            writer.WriteLine(ex.StackTrace);
                        }
                        writer.WriteLine();
                    }
                }
            }
            catch
            {
                // Fail-safe: prevent the app from crashing if logging itself fails (e.g. read-only file system, disk full)
            }
        }
    }
}
