using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip;

public enum RequestType : byte
{
    CreateNewGame = 0,
    JoinToGame = 1,
    Port = 2,
    Online = 3,
    Ping = 4,
    Disconnect = 5,
    Exception = 6


}
