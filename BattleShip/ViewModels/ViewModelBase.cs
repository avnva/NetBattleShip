using System;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Windows;
using BattleShip.ViewModels;
using System.Threading.Tasks;

namespace BattleShip;

public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler<MessageBoxEventArgs> MessageBoxRequest;
    public event EventHandler<OpenViewEventArgs> OpenNewWindow;
    public Action Close { get; set; }
    public Action Hide { get; set; }
    protected void MessageBox_Show(Action<MessageBoxResult> resultAction, string messageBoxText,
        string caption = "", MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None)
    {
        MessageBoxRequest?.Invoke(this,
            new MessageBoxEventArgs(resultAction, messageBoxText, caption,
                button, icon, defaultResult, options));
    }
    //protected void MessageBox_Show(Action<MessageBoxResult> resultAction, string messageBoxText,
    //string caption = "", MessageBoxButton button = MessageBoxButton.OK,
    //MessageBoxImage icon = MessageBoxImage.None,
    //MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None)
    //{
    //    var args = new MessageBoxEventArgs(resultAction, messageBoxText, caption, button, icon, defaultResult, options);
    //    MessageBoxRequest?.Invoke(this, args);
    //    args.Show();
    //}
    protected void MessageBox_ShowAsync(Func<MessageBoxResult, Task> resultAction, string messageBoxText,
    string caption = "", MessageBoxButton button = MessageBoxButton.OK,
    MessageBoxImage icon = MessageBoxImage.None,
    MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None)
    {
        var args = new MessageBoxEventArgs(resultAction, messageBoxText, caption, button, icon, defaultResult, options);
        MessageBoxRequest?.Invoke(this, args);
        //args.Show();
    }


    protected void OnPropertyChange(string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    protected void OpenNewView(Window view)
    {
        OpenNewWindow?.Invoke(this, new OpenViewEventArgs(view));
    }

    protected void Dispose()
    {
    }
}
