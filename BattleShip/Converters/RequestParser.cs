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
    private byte _checkOnline = 4;

    public string Parse(string request)
    {
        if (request == @"Create new game")
            return Encoding.UTF8.GetString(new[] { _createNewGameRequest }) + request;
        if (request == @"Get game room")
            return Encoding.UTF8.GetString(new[] { _getNewPortRequest }) + request;
        if(request.Contains("Connect to existing game room: "))
            return Encoding.UTF8.GetString(new[] { _connectToExistingGameRoom }) + request;
        if (request == @"Check online")
            return Encoding.UTF8.GetString(new[] { _checkOnline }) + request;
        if (request == @"Waiting opponent")
            return Encoding.UTF8.GetString(new[] { _waitingOpponent }) + request;
        else
            return null;

        //FileAttributes attributes = File.GetAttributes(request);
        //if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
        //    return Encoding.UTF8.GetString(new[] { directoryTreeRequest }) + request;
        //else
        //    return Encoding.UTF8.GetString(new[] { fileContentsRequest }) + request;
    }
}
