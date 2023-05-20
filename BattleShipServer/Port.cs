using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShipServer;
public class Port
{
    public int PortValue { get; }
    public bool Occupied { get; set; }

    public Port(int _portValue, bool _occupied)
    {
        PortValue = _portValue;
        Occupied = _occupied;
    }
}