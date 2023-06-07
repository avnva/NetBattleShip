using System.Net.Sockets;


public class GameRoom
{
    private int _maxPlayers = 2;
    public List<TcpClient> players;
    public Port Port { get; set; }
    public bool IsFirstPlayerSet;
    public bool IsPlayerReady { get; set; }
    public GameRoom(Port port)
    {
        players = new List<TcpClient>();
        Port = port;
    }
    public void AddPlayer(TcpClient player)
    {
        players.Add(player ?? throw new ArgumentNullException(nameof(players)));
    }
    public void RemovePlayer(TcpClient player)
    {
        players.Remove(player ?? throw new ArgumentNullException(nameof(players)));
    }
    public bool IsFull()
    {
        return players.Count == _maxPlayers;
    }
    public bool IsEmpty()
    {
        return players.Count == 0;
    }
    public void SetFirstPlayer()
    {
        IsFirstPlayerSet = true;
    }
}
