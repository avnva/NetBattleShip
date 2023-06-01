using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip;
public class Ship
{
    public string Name { get; set; } // Название корабля
    public int Size { get; set; } // Размер корабля (количество клеток, которые он занимает)
    public int Hits { get; set; } // Количество попаданий по кораблю

    public ShipDirection Direction { get; set; }

    public bool IsSunk => Hits == Size; // Свойство, указывающее, потоплен ли корабль

    public Ship(string name, int size)
    {
        Name = name;
        Size = size;
        Hits = 0;
    }

    public void Hit()
    {
        Hits++;
    }
}
