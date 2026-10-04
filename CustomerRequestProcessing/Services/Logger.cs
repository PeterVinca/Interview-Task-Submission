
namespace CustomerRequestProcessing.Services;

public interface ILogger
{
    void Info(string msg, bool onlyToFile = false, bool includeTimestampAndLevel = true);
    void Warn(string msg);
    void Error(string msg);
}

public class Logger : ILogger
{
    private readonly string _logFile;

    public Logger()
    {
    }

    public Logger(string logFilePath)
    {
        _logFile = logFilePath ?? "app.log";
    }

    public void Info(string msg, bool onlyToFile = false, bool includeTimestampAndLevel = true) => Write("INFO", msg, onlyToFile, includeTimestampAndLevel);
    public void Warn(string msg) => Write("WARN", msg);
    public void Error(string msg) => Write("ERROR", msg);

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
