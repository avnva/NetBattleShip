using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BattleShip;

public class CellStateToColorConverter : IValueConverter
{
    BrushConverter converter = new BrushConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is CellState state)
        {
            switch (state)
            {
                case CellState.Empty:
                    return Brushes.White; // Цвет для пустой клетки
                case CellState.Ship:
                    
                    Brush customBrush = (Brush)converter.ConvertFrom("#1a153f");
                    return customBrush; // Цвет для занятой клетки (корабль)
                // Добавьте другие состояния и соответствующие цвета
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