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
    private byte disksRequest = 7;

    public string Parse(string request)
    {
        if (request == @"\")
            return Encoding.UTF8.GetString(new[] { disksRequest }) + request;

        FileAttributes attributes = File.GetAttributes(request);
        if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            return Encoding.UTF8.GetString(new[] { directoryTreeRequest }) + request;
        else
            return Encoding.UTF8.GetString(new[] { fileContentsRequest }) + request;
    }
}
