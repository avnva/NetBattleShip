using System.Net.Sockets;

namespace BattleShipServer;
public class GameRoomManager
{
    private List<GameRoom> _gameRooms;
    public GameRoomManager()
    {
        _gameRooms = new List<GameRoom>();
    }
    public bool IsPlayerReady(Port port)
    {
        GameRoom room = FindGameRoom(port);
        return room.IsPlayerReady;
    }
    public void SetPlayerReady(bool value, Port port)
    {
        GameRoom room = FindGameRoom(port);
        room.IsPlayerReady = value;
    }
    public bool IsFirstPlayer(TcpClient client, Port port)
    {
        GameRoom room = FindGameRoom(port);
        if (!room.IsFirstPlayerSet)
        {
            room.SetFirstPlayer();
            return true;
        }
        return false;   
    }
    public void AddPlayerToExistsRoom(TcpClient player, Port port)
    {
        GameRoom connectionRoom = GetConnectionRoom(port);
        connectionRoom.AddPlayer(player);
    }
    public void AddPlayerToNewRoom(TcpClient player, Port port) 
    {
        GameRoom newRoom = CreateGameRoom(port);
        newRoom.AddPlayer(player);
    }
    public GameRoom GetConnectionRoom(Port port)
    {
        GameRoom connectionRoom = null;
        foreach (GameRoom room in _gameRooms)
        {
            if (room.Port == port)
            {
                if (!room.IsFull())
                {
                    if (!room.IsEmpty())
                    {
                        connectionRoom = room;
                        return connectionRoom;
                    }
                    else
                        throw new ArgumentException("Комната не найдена");
                }
                else
                    throw new ArgumentException("Комната уже занята");
            } 
        }
        throw new ArgumentException("Комната не найдена");
    }
    public void RemovePlayerFromGameRoom(Port port, TcpClient player)
    {
        GameRoom room = FindGameRoom(port);
        if (room != null)
        {
            room.RemovePlayer(player);
            if (room.IsEmpty())
                RemoveGameRoom(room);
        }
    }
    public GameRoom? FindGameRoom(Port port)
    {
        foreach (GameRoom room in _gameRooms)
        {
            if (room.Port == port)
            {
                return room;
            }
        }
        return null;
    }
    public bool CheckPlayersConnection(Port port)
    {
        foreach(GameRoom room in _gameRooms)
        {
            if (room.Port == port)
            {
                return room.IsFull();
            }
        }
        return false;
    }
    public TcpClient GetOpponent(TcpClient client, Port port)
    {
        foreach (GameRoom room in _gameRooms)
        {
            if (room.Port == port)
            {
                if (room.players[0] == client)
                    return room.players[1];
                else return room.players[0];
            }
        }
        return client;
    }
    private void RemoveGameRoom(GameRoom gameRoom)
    {
        _gameRooms.Remove(gameRoom);
    }

    private GameRoom CreateGameRoom(Port port)
    {
        GameRoom gameRoom = new GameRoom(port);
        _gameRooms.Add(gameRoom);
        return gameRoom;
    }
}