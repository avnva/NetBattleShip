using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;


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
        Closing += OnClose;
        InitializeComponent();
        tbIP.PreviewTextInput += TextBox_PreviewTextInput;
        (DataContext as ViewModelBase).Close += () => { Close(); };
        (DataContext as ViewModelBase).Hide += () => { Hide(); };
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
    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Проверяем, является ли вводимый символ цифрой, точкой или двоеточием
        if (!IsNumeric(e.Text) && e.Text != "." && e.Text != ":")
        {
            // Если символ не соответствует требуемому формату, отменяем его ввод
            e.Handled = true;
        }
    }
    private bool IsNumeric(string text)
    {
        foreach (char c in text)
        {
            if (!Char.IsDigit(c))
            {
                return false;
            }
        }
        return true;
    }
}
