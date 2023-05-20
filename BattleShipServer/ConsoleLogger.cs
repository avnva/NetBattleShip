using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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