using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip.ViewModels;
public class CellViewModel : ViewModelBase
{
    private int _row;
    private int _column;

    public int Row
    {
        get { return _row; }
        set
        {
            _row = value;
            OnPropertyChange(nameof(Row));
        }
    }

    public int Column
    {
        get { return _column; }
        set
        {
            _column = value;
            OnPropertyChange(nameof(Column));
        }
    }

    public CellViewModel(int row, int column)
    {
        Row = row;
        Column = column;
    }
}
