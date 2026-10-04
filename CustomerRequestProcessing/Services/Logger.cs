using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerRequestProcessing.Services;

internal class Logger
{
    private readonly string _logFile;

    internal Logger(string logFilePath)
    {
        _logFile = logFilePath ?? "app.log";
    }

    internal void Info(string msg, bool onlyToFile = false, bool includeTimestampAndLevel = true) => Write("INFO", msg, onlyToFile, includeTimestampAndLevel);
    internal void Warn(string msg) => Write("WARN", msg);
    internal void Error(string msg) => Write("ERROR", msg);

    private void Write(string level, string msg, bool onlyToFile = false, bool includeTimestampAndLevel = true)
    {
        string line = includeTimestampAndLevel 
            ? $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {msg}" 
            : msg;

        File.AppendAllText(_logFile, line + Environment.NewLine);

        if (!onlyToFile)
            Console.WriteLine(line);
    }
}
