using System;
using System.IO;

namespace FileDownloader6
{
    public static class Logger
    {
        static private string logDirectory = string.Empty;
        static private string currentLogFilePath = string.Empty;
        static private DateTime currentLogDate;

        static private void UpdateLogFilePath()
        {
            currentLogDate = DateTime.Today;
            string logFileName = $"log_{currentLogDate:yyyy-MM-dd}.log";
            currentLogFilePath = Path.Combine(logDirectory, logFileName);
        }

        static public void Log(string message)
        {
            // Check if the date has changed and update the log file path if necessary
            if (DateTime.Today != currentLogDate)
            {
                UpdateLogFilePath();
            }

            using (StreamWriter writer = new StreamWriter(currentLogFilePath, true))
            {
                writer.WriteLine($"[Info][{DateTime.Now:HH:mm:ss}] {message}");
            }
        }

        static public void ErrorLog(string message)
        {
            // Check if the date has changed and update the log file path if necessary
            if (DateTime.Today != currentLogDate)
            {
                UpdateLogFilePath();
            }

            using (StreamWriter writer = new StreamWriter(currentLogFilePath, true))
            {
                writer.WriteLine($"[Error][{DateTime.Now:HH:mm:ss}] {message}");
            }
        }

        static public void Init()
        {
            logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logDirectory);
        }
    }
}