using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip;

public class RequestParser
{
    private byte _createNewGameRequest = 0;
    private byte _connectToExistingGameRoom = 1;
    private byte _waitingOpponent = 2;
    private byte _getNewPortRequest = 3;
    private byte _checkOnlineRequest = 4;
    private byte _startGameRequest = 8;
    private byte _checkCellRequest = 9;
    private byte _opponentMoveRequest = 10;
    private byte _sendCellState = 11;
    private byte _reconnect = 12;

    public string Parse(string request)
    {
        if (request == @"Create new game")
            return Encoding.UTF8.GetString(new[] { _createNewGameRequest }) + request;
        if (request == @"Get game room")
            return Encoding.UTF8.GetString(new[] { _getNewPortRequest }) + request;
        if(request.Contains("Connect to existing game room: "))
            return Encoding.UTF8.GetString(new[] { _connectToExistingGameRoom }) + request;
        if (request == @"Check online")
            return Encoding.UTF8.GetString(new[] { _checkOnlineRequest }) + request;
        if (request == @"Waiting opponent")
            return Encoding.UTF8.GetString(new[] { _waitingOpponent }) + request;
        if (request == @"Start game")
            return Encoding.UTF8.GetString(new[] { _startGameRequest }) + request;
        if (request.Contains("Hits cell:") )
            return Encoding.UTF8.GetString(new[] { _checkCellRequest }) + request;
        if (request == @"Wait opponent...")
            return Encoding.UTF8.GetString(new[] { _opponentMoveRequest }) + request;
        if (request.Contains("Cell state:"))
            return Encoding.UTF8.GetString(new[] { _sendCellState }) + request;
        if(request.Contains("Reconnect to port:"))
            return Encoding.UTF8.GetString(new[] { _reconnect }) + request;
        else
            return null;

    }
}
