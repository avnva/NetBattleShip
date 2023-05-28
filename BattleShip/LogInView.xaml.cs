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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace BattleShip;

/// <summary>
/// Логика взаимодействия для LogInView.xaml
/// </summary>
public partial class LogInView : Window
{
    public LogInView()
    {
        DataContext = new LogInViewModel();
        (DataContext as ViewModelBase).MessageBoxRequest += ViewMessageBoxRequest;
        InitializeComponent();
    }


    private void ViewMessageBoxRequest(object sender, MessageBoxEventArgs e)
    {
        e.Show();
    }
}
