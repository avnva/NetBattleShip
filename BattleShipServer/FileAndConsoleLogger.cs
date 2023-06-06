

namespace BattleShipServer;
public class FileAndConsoleLogger : ILogger
{
    private ILogger fileLogger;
    private ILogger consoleLogger;

    public FileAndConsoleLogger(string path)
    {
        if (String.IsNullOrEmpty(path))
            throw new ArgumentNullException(nameof(path));
        fileLogger = new FileLogger(path);
        consoleLogger = new ConsoleLogger();
    }
    public void Log(string message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));
        fileLogger.Log(message);
        consoleLogger.Log(message);
    }
}