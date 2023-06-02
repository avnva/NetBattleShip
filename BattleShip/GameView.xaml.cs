using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BattleShip;

/// <summary>
/// Логика взаимодействия для GameView.xaml
/// </summary>
public partial class GameView : Window
{
    public GameView()
    {
        InitializeComponent();
    }

    public GameView(GameViewModel vm)
    {
        DataContext = vm;
        Closing += OnClose;
        (DataContext as ViewModelBase).MessageBoxRequest +=
            ViewMessageBoxRequest;
        InitializeComponent();
    }


    private void ViewMessageBoxRequest(object sender, MessageBoxEventArgs e)
    {
        e.Show();
    }

    private void OnClose(object sender, CancelEventArgs e)
    {
        Closing -= OnClose;
        (DataContext as ViewModelBase).MessageBoxRequest -= ViewMessageBoxRequest;
    }
}
