using System;
using System.IO;
using System.Text;

namespace Nexoris.CentralApi.Logging
{
    public static class FileLogger
    {
        private static readonly object _fileLock = new object();
        private static readonly string _logDir;

        static FileLogger()
        {
            try
            {
                _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(_logDir))
                {
                    Directory.CreateDirectory(_logDir);
                }
            }
            catch
            {
                _logDir = AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public static void Info(string message, params object[] args)
        {
            WriteLog("INFO", ConsoleColor.Cyan, message, args);
        }

        public static void Success(string message, params object[] args)
        {
            WriteLog("OK", ConsoleColor.Green, message, args);
        }

        public static void Warn(string message, params object[] args)
        {
            WriteLog("WARN", ConsoleColor.Yellow, message, args);
        }

        public static void Error(string message, params object[] args)
        {
            WriteLog("ERROR", ConsoleColor.Red, message, args);
        }

        public static void Error(Exception ex, string message, params object[] args)
        {
            string formatted = args != null && args.Length > 0 ? string.Format(message, args) : message;
            string fullMessage = formatted + "\n" + ex?.ToString();
            WriteLog("ERROR", ConsoleColor.Red, fullMessage, null);
        }

        private static void WriteLog(string level, ConsoleColor color, string message, object[] args)
        {
            string formattedMessage = args != null && args.Length > 0 ? string.Format(message, args) : message;
            var now = DateTime.Now;
            string logLine = string.Format("[{0:yyyy-MM-dd HH:mm:ss.fff}] [{1}] [T:{2}] {3}",
                now, level.PadRight(5), System.Threading.Thread.CurrentThread.ManagedThreadId, formattedMessage);

            // Mirror to console
            Console.ForegroundColor = color;
            Console.WriteLine(string.Format("[{0}] [{1}] {2}", level.PadRight(5), now.ToString("HH:mm:ss"), formattedMessage));
            Console.ResetColor();

            // Write to daily rolling log file
            try
            {
                lock (_fileLock)
                {
                    string filePath = Path.Combine(_logDir, string.Format("central-api-{0:yyyy-MM-dd}.log", now));
                    File.AppendAllText(filePath, logLine + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // Fallback: don't let logging failures crash the service
            }
        }
    }
}
