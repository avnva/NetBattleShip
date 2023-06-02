﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip;

public class Cell
{
    public int Row { get; set; }
    public int Column { get; set; }
    public CellState State { get; set; }

    public Cell(int row, int column)
    {
        Row = row;
        Column = column;
        State = CellState.Empty;
    }
}
