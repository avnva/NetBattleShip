namespace BattleShipServer;
public class Program
{
    public static async Task Main()
    {
        await Run();
    }

    private static async Task Run()
    {
        TCPServer tcpServer = new TCPServer(new FileAndConsoleLogger("server_log.txt"));
        await tcpServer.Start();
    }
}