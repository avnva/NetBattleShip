

namespace BattleShipServer;
public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));
        Console.WriteLine(message);
    }
}