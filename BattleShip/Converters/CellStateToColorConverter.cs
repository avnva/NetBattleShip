using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;

namespace BattleShip;

public class CellStateToColorConverter : IValueConverter
{
    BrushConverter converter = new BrushConverter();
    Brush customBrush;
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is CellState state)
        {
            switch (state)
            {
                case CellState.Empty:
                    return Brushes.White; // Цвет для пустой клетки
                case CellState.Ship:
                    customBrush = (Brush)converter.ConvertFrom("#1a153f");
                    return customBrush; // Цвет для занятой клетки (корабль)
                case CellState.Hit:
                    customBrush = (Brush)converter.ConvertFrom("#ff3333");
                    return customBrush;
                case CellState.Miss:
                    customBrush = (Brush)converter.ConvertFrom("#b5b5b5");
                    return customBrush;
                default:
                    return Brushes.Transparent;
            }
        }

        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}