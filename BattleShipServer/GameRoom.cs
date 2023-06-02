using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BattleShipServer;
public class GameRoom
{
    public List<TcpClient> players;
    public bool IsGameRoomBusy { get; set; }
    public bool IsGameStarted { get; set; }
    public Port Port { get; set; }
    private int _maxPlayers = 2;
    public GameRoom(Port port)
    {
        players = new List<TcpClient>();
        Port = port;
    }

    public void AddPlayer(TcpClient player)
    {
        players.Add(player ?? throw new ArgumentNullException());
    }

    public void RemovePlayer(TcpClient player)
    {
        players.Remove(player ?? throw new ArgumentNullException());
    }

    public bool IsFull()
    {
        return players.Count >= _maxPlayers;
    }

    public bool IsEmpty()
    {
        return players.Count == 0;
    }


}
