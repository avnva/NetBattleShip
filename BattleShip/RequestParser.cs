using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip;

public class RequestParser
{
    private byte directoryTreeRequest = 1;
    private byte fileContentsRequest = 2;
    private byte fileNameRequest = 3;
    private byte _createNewGameRequest = 0;
    private byte _getNewPortRequest = 2;

    public string Parse(string request)
    {
        if (request == @"Create new game")
            return Encoding.UTF8.GetString(new[] { _createNewGameRequest }) + request;
        if (request == @"Get new port")
            return Encoding.UTF8.GetString(new[] { _getNewPortRequest }) + request;
        else
            return null;

        //FileAttributes attributes = File.GetAttributes(request);
        //if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
        //    return Encoding.UTF8.GetString(new[] { directoryTreeRequest }) + request;
        //else
        //    return Encoding.UTF8.GetString(new[] { fileContentsRequest }) + request;
    }
}
