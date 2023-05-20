using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BattleShipServer;
public class GameRoomManager
{
    private List<GameRoom> _gameRooms;

    public GameRoomManager()
    {
        _gameRooms = new List<GameRoom>();
    }
    public bool IsAvailableRoomExists()
    {
        GameRoom availableRoom = GetAvailableRoom();
        if (availableRoom != null)
            return true;
        else
            return false;
    }

    public void AddPlayerToAvailableRoom(TcpClient player)
    {
        GameRoom availableRoom = GetAvailableRoom();
        availableRoom.AddPlayer(player);
    }
    public void AddPlayerToNewRoom(TcpClient player, Port port) 
    {
        GameRoom newRoom = CreateGameRoom(port);
        newRoom.AddPlayer(player);
    }

    private GameRoom GetAvailableRoom()
    {
        GameRoom availableRoom = null;
        foreach (GameRoom room in _gameRooms)
        {
            if (!room.IsFull())
            {
                availableRoom = room;
                break;
            }
        }
        return availableRoom;
    }

    public void RemovePlayerFromGameRoom(GameRoom gameRoom, TcpClient player)
    {
        if (gameRoom == null)
            throw new ArgumentNullException(nameof(gameRoom));

        gameRoom.RemovePlayer(player);

        if (gameRoom.IsEmpty())
            RemoveGameRoom(gameRoom);
    }

    public void RemoveGameRoom(GameRoom gameRoom)
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