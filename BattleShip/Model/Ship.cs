using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip;
public class Ship
{
    private string _name;
    public string Name { 
        get { return _name; }
        set { _name = value; } 
    } 
    public int Size { get; set; } // Размер корабля (количество клеток, которые он занимает)
    public int Hits { get; set; } // Количество попаданий по кораблю

    public ShipDirection Direction { get; set; }

    public bool IsSunk => Hits == Size; // Свойство, указывающее, потоплен ли корабль

    public Ship(int size)
    {
        Size = size;
        if (Size == 1)
            _name = "Торпедный катер";
        else if (Size == 2)
            _name = "Эсминец";
        else if (Size == 3)
            _name = "Крейсер";
        else if (Size == 4)
            _name = "Линкор";
        else
            throw new ArgumentException("Неверный размер корабля.");
        Hits = 0;
    }

    public void Hit()
    {
        Hits++;
    }
}
