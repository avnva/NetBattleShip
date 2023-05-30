using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip;

public struct Response
{
    public RequestType Type { get; }

    public string Contents { get; }
    public bool Flag { get; }

    public Response(RequestType type, string contents)
    {
        Type = type;
        Contents = contents;
    }
    public Response(RequestType type, bool flag)
    {
        Type = type;
        Flag = flag;
    }
}
