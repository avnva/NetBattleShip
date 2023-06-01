using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BattleShip;
public enum CellState
{
    Empty,
    Ship,
    Hit,
    Miss
}
public class CellViewModel : ViewModelBase
{
    private CellState _state;

    public int Row { get; }
    public int Column { get; }

    public CellState State
    {
        get { return _state; }
        set
        {
            _state = value;
            OnPropertyChange(nameof(State));
            OnPropertyChange(nameof(CellColor));
        }
    }

    public SolidColorBrush CellColor
    {
        get
        {
            return (SolidColorBrush)new 
                CellStateToColorConverter().Convert(State, typeof(SolidColorBrush), null, CultureInfo.CurrentCulture);
        }
    }

    public CellViewModel(int row, int column)
    {
        Row = row;
        Column = column;
        State = CellState.Empty;
    }
}
