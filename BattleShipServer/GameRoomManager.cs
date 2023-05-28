﻿using System;
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
    //public bool IsAvailableRoomExists()
    //{
    //    GameRoom connectionRoom = GetConnectionRoom();
    //    if (connectionRoom != null)
    //        return true;
    //    else
    //        return false;
    //}

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

    private GameRoom GetConnectionRoom(Port port)
    {
        GameRoom connectionRoom = null;
        foreach (GameRoom room in _gameRooms)
        {
            if (room.Port == port || !room.IsFull())
            {
                if (!room.IsFull())
                {
                    if (!room.IsEmpty())
                    {
                        connectionRoom = room;
                        break;
                    }
                    else
                        throw new ArgumentException("Противник отключился от игры");
                }
                else
                    throw new ArgumentException("Комната уже занята");
            }
            else
                throw new ArgumentException("Комната не найдена");
        }

        return connectionRoom;
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