using System.ComponentModel;
using System.Windows;

namespace BattleShip;

/// <summary>
/// Логика взаимодействия для GameView.xaml
/// </summary>
public partial class GameView : Window
{
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
