using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BattleShip.ViewModels;

public class OpenViewEventArgs
{
    private readonly Window _view;

    public OpenViewEventArgs(Window view)
    {
        _view = view;
    }

    public void Show()
    {
        _view.Show();
    }
}